import { contextBridge, ipcRenderer  } from 'electron'
import { electronAPI } from '@electron-toolkit/preload'

// Custom APIs for renderer
const api = {}

// Use `contextBridge` APIs to expose Electron APIs to
// renderer only if context isolation is enabled, otherwise
// just add to the DOM global.
if (process.contextIsolated) {
  try {
    contextBridge.exposeInMainWorld('electron', electronAPI)
    contextBridge.exposeInMainWorld('api', api)
  } catch (error) {
    console.error(error)
  }
} else {
  // @ts-ignore (define in dts)
  window.electron = electronAPI
  // @ts-ignore (define in dts)
  window.api = api
}

// To fire a one-way IPC message from a renderer process to the main process, 
// the ipcRenderer.send API can send a message that is then received 
// by the ipcMain.on API
//
// Exposing this API in the preload script allows the renderer process to use it.
//
contextBridge.exposeInMainWorld('electronAPI', {
  pushNotification: (mathod: string, params: any) => ipcRenderer.send(mathod, params)
})
