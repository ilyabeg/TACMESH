import { app, shell, BrowserWindow, ipcMain } from 'electron'
import { join } from 'path'
import { electronApp, optimizer, is } from '@electron-toolkit/utils'
import icon from '../../resources/icon.png?asset'
import dispatchEvent from './ipc/eventDispatcher'

// import agent management
import { startAgent, killAgent, getAgentState } from './AgentManager'
import { launchLauncher, terminateLauncher, pushToLauncher } from './simulation/scenario manager/Launcher';


// --- window creating ---

function createWindow(): BrowserWindow {
  // Create the browser window.
  const mainWindow = new BrowserWindow({
    width: 900,
    height: 670,
    show: false,
    autoHideMenuBar: true,
    ...(process.platform === 'linux' ? { icon } : {}),
    webPreferences: {
      preload: join(__dirname, '../preload/index.js'),
      sandbox: false
    }
  })

  mainWindow.on('ready-to-show', () => {
    mainWindow.show()
  })

  mainWindow.webContents.setWindowOpenHandler((details) => {
    shell.openExternal(details.url)
    return { action: 'deny' }
  })

  // HMR for renderer base on electron-vite cli.
  // Load the remote URL for development or the local html file for production.
  if (is.dev && process.env['ELECTRON_RENDERER_URL']) {
    mainWindow.loadURL(process.env['ELECTRON_RENDERER_URL'])
  } else {
    mainWindow.loadFile(join(__dirname, '../renderer/index.html'))
  }

  // return the browser window to use it in initializig the agent
  return mainWindow; 
}


// This method will be called when Electron has finished
// initialization and is ready to create browser windows.
// Some APIs can only be used after this event occurs.
app.whenReady().then(() => {
  // Set app user model id for windows
  electronApp.setAppUserModelId('com.electron')

  // Default open or close DevTools by F12 in development
  // and ignore CommandOrControl + R in production.
  // see https://github.com/alex8088/electron-toolkit/tree/master/packages/utils
  app.on('browser-window-created', (_, window) => {
    optimizer.watchWindowShortcuts(window)
  })

  // get the electron window
  const browserWindow = createWindow()

  // execute the launcher instance for this app
  const numOfAgents = 1; // <- TEMPORARY 
  launchLauncher(browserWindow, numOfAgents);

  attachEventDispatchers();
  attachEventListeners();

  app.on('activate', function () {
    // On macOS it's common to re-create a window in the app when the
    // dock icon is clicked and there are no other windows open.
    if (BrowserWindow.getAllWindows().length === 0) createWindow()
  })
})


// --- custom main process event dispatchers ---

const attachEventDispatchers = (): void => {

  // return the agent state
  dispatchEvent(ipcMain, 'get-agent-state', async () => {
    return getAgentState();
  });
}


// --- custom ipc event listeners ---

// Note: difference between the dispacthers and the listeners is that the 
// dispatchers are event handlers that repspond, while the listeners
// are event handlers which only expect an event without a response to it
//
const attachEventListeners = () => {

  // launcher push notif handler
  ipcMain.on('launcher-stdin', (event, args: { stdInput: string }): void => {

    // push input to the launcher
    pushToLauncher(args.stdInput + '\n');
  });
}


// --- app event handlers --- 

// Quit when all windows are closed, except on macOS. There, it's common
// for applications and their menu bar to stay active until the user quits
// explicitly with Cmd + Q.
app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit()
  }
})

// Emitted when all windows have been closed and the application will quit. 
// Calling event.preventDefault() will prevent the default behavior, which 
// is terminating the application.
app.on('will-quit', (event) => {

  // prevent the app from quiting before launcher can
  // clean up his child processes
  // 
  if (!launcherClean) {
    event.preventDefault();

    // flip the switch
    launcherClean = true;
    cleanApp();
  } 

  // before quiting, kill the agent instance.
  // killAgent();
})


// --- app cleanup ---

// boolean indicating if the launcher has been cleaned out before quitting the application
let launcherClean: boolean = false;

// clean up before quitting the app
const cleanApp = (): void => { 

  // currently only cleaning up launcher, but maybe more cleanpus will be added in the future
  terminateLauncher(() => app.quit());
}