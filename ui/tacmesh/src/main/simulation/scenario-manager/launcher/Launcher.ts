import { BrowserWindow } from "electron";
import { ChildProcess, spawn } from "node:child_process";
import pushToClient from "../../../ipc/pushToClient";
import path from "node:path";

let _launcher: ChildProcess; // the launcher process
let _browserWindow: BrowserWindow; // the user window

// get the absoulte relative path for the launcher exe file.
// __dirmame contains the absolute path of the directory 
// containing the currently executing JavaScript/TypeScript file
//
const _launcherExe = path.resolve(__dirname, '..', '..', '..', '..',
    'backend', 
    'src', 
    'TacMesh.Launcher',
    'bin',
    'Debug',
    'net10.0',
    'TacMesh.Launcher.exe'
);

// launches the launcher process
export function launchLauncher(window: BrowserWindow, numOfEngines: number) {
    // save the browser window instance for future use
    _browserWindow = window;

    // check the path
    console.log(`Path to Launcher.exe: ${_launcherExe}`);

    // spawn the launcher process as a child process    
    _launcher = spawn(_launcherExe, [numOfEngines.toString()]);
    attachHandlers();
}

// terminate the launcher to kill all the engine processes
export function terminateLauncher(quitCallback: () => void) {

    // The subprocess.exitCode property indicates the exit code
    // of the child process. If the child process is still running,
    // the field will be null
    //
    if (_launcher?.exitCode === null) {

        // calling ChildProcess.kill() without params sends the process
        // SIGTERM which does not let the launcher process terminate all 
        // of his engine child processes before quitting.
        //
        // instead, an 'exit' flag is sent to trigger the cleanup instead 
        // of executing a OS depended "process kill" command
        
        // attach a one time listener for the launcher to quit the app after exiting
        _launcher.once('exit', () => quitCallback());

        // trigger the exit
        pushToLauncher('exit\n');
    }
    else {
        // quit the app
        quitCallback();
    }    
}

// attach event handlers
function attachHandlers() {
    onStdout();
    onStderr();
    onError();
    onExit();
    onClose();
}

// standard output received from the child process' output stream
function onStdout() {
    if (_launcher) {
        _launcher.stdout?.on('data', (output) => {
            // print the launcher output
            const strOutput: string = output.toString();
            console.log(`${strOutput}`);      
        });  
    }
}

// error received from the CHILD PROCESS
function onStderr() {
    if (_launcher) { 
        _launcher.stderr?.on('data', (error) => {
            console.error(`[Launcher] Received error from child process due to: \"${error}\"`);
            pushToClient(_browserWindow, 'error', error);
        });
    }
}

// error received from the OS
function onError() {
    if (_launcher) {
        _launcher.on('error', (error) => {
            console.error(`[Launcher] Failed to start child process due to: \"${error}\"`);
            pushToClient(_browserWindow, 'error', error);   
        }); 
    }
}

// exit event fires when the process teminates but the underlying
// stream pipes might still be open
//
function onExit() {
    if (_launcher) {
        _launcher.on('exit', (exit) => {

            // When clicking the X button the exit code is evaluated to null and
            // the browser window object is destroyed, meaning sending a push notification 
            // to the client window is impossible and throws an error. Thus, this if block
            // prevents the error from popping up.
            //
            if (exit !== null) {
                console.log(`[Launcher] Child process exited \"${exit}\"`); 
            }
        });
    }
}

// child event fires when the process terminates and all of the 
// stream pipes have been closed
//
function onClose() {
    if (_launcher) {
        _launcher.on('close', (code) => {

            // When clicking the X button the exit code is evaluated to null and
            // the browser window object is destroyed, meaning sending a push notification 
            // to the client window is impossible and throws an error. Thus, this if block
            // prevents the error from popping up.
            //
            if (code !== null) {
                console.log(`[Launcher] Child process closed with code ${code}`);
            }
        });
    }
}

// function used to push an input down the launchers stdinput stream
export function pushToLauncher(input: string) { 
    // if launcher and the std input stream is open
    if (_launcher && _launcher.stdin) {
        _launcher.stdin.write(input);
    } 
    else {
        console.error('[Launcher] Failed to write to std input stream...');
    }
}
