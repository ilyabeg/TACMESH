import { ChildProcess, spawn } from 'node:child_process';
import pushToClient from './ipc/pushToClient';
import path from 'node:path';
import { BrowserWindow } from 'electron';
import { EngineProcess } from '../shared/types/enginePeocess';

let _browserWindow: BrowserWindow; // the electron window instance
let _engine: ChildProcess | null = null; // the engine process instance
let _assignedPort: number; // the assigned port of the engine instance
let _engineRunning: boolean = false; // tells if the engine process is running or down

// get the absoulte relative path for the engine exe file.
// __dirmame contains the absolute path of the directory 
// containing the currently executing JavaScript/TypeScript file
//
const _engineExe = path.resolve(__dirname, '..', '..', '..', '..',
    'backend', 
    'src', 
    'TacMesh.Engine',
    'bin',
    'Debug',
    'net10.0',
    'TacMesh.Engine.exe'
);

// starts an engine instance for this app, and returns true if  
// succeeded or false otherwise
//
export function startEngine(window: BrowserWindow): boolean {

    // save the browser window instance for future use
    _browserWindow = window;

    // check the path
    // console.log(`Path to Engine.exe: ${_engineExe}`);

    // spawn the engine process as a child process    
    _engine = spawn(_engineExe, ["Node-01", "{}", "0", "[]"]); // TEMPORARILY init engine process with default arguments

    if (_engine) {
        // indicate if the engine is running or not
        updateEngineState(true);

        // attach event handlers
        attachHandlers();
        return true;
    }
    else {
        return false;
    }
}

// attaches the engine child process event handlers
function attachHandlers() {
    onStdout();
    onStderr();
    onError();
    onExit();
    onClose();
}

// standard output received from the child process' output stream
function onStdout() {
    if (_engine) {
        _engine.stdout?.on('data', (output) => {
            
            const strOutput: string = output.toString();
            console.log(`[Main Process] Received std output: \n${strOutput}`);

            if (strOutput.includes('PORT=')) {
                // try parse the port number 
                _assignedPort = tryParsePort(strOutput);

                if (_assignedPort != -1) {
                    console.log(`[Main Process] Engine initiated on PORT: ${_assignedPort}`);

                    // build the engine information
                    const engineInfo : EngineProcess = {
                        nodeId: "tempId", // TEMPORARILY use a temp id
                        port: _assignedPort,
                        processId: "12345" // TEMPORARILY use a temp pid
                    }
                    // push the engine info to renderer process
                    pushToClient(_browserWindow, 'engine-received', engineInfo);
                }
            }            
        });  
    }
}

// the constant parsing base (10 = decimal)
const _base : number = 10;

// parsing the assigned port
function tryParsePort(output: string) : number {

    // split the outputted string
    const parts = output.trim().split('=');
    const portNumber : number = parseInt(parts[1], _base);

    // if the parsing was successful
    if (!isNaN(portNumber)) {
        return portNumber;
    }
    // if failed to parse port, return -1
    return -1;
}

// error received from the CHILD PROCESS
function onStderr() {
    if (_engine) { 
        _engine.stderr?.on('data', (error) => {
            console.error(`[Engine Manager] Received error from child process due to: \"${error}\"`);
            pushToClient(_browserWindow, 'error', error);
        });
    }
}

// error received from the OS
function onError() {
    if (_engine) {
        _engine.on('error', (error) => {
            console.error(`[Engine Manager] Failed to start child process due to: \"${error}\"`);
            pushToClient(_browserWindow, 'error', error);
            // attempt to restart
            restartEngine();
        }); 
    }
}

// exit event fires when the process teminates but the underlying
// stream pipes might still be open
//
function onExit() {
    if (_engine) {
        _engine.on('exit', (exit) => {

            // When clicking the X button the exit code is evaluated to null and
            // the browser window object is destroyed, meaning sending a push notification 
            // to the client window is impossible and throws an error. Thus, this if block
            // prevents the error from popping up.
            //
            if (exit !== null) {
                console.log(`[Engine Manager] Child process exited \"${exit}\"`);    
                // update the engine's state
                updateEngineState(false);
            }
        });
    }
}

// child event fires when the process terminates and all of the 
// stream pipes have been closed
//
function onClose() {
    if (_engine) {
        _engine.on('close', (code) => {

            // When clicking the X button the exit code is evaluated to null and
            // the browser window object is destroyed, meaning sending a push notification 
            // to the client window is impossible and throws an error. Thus, this if block
            // prevents the error from popping up.
            //
            if (code !== null) {
                console.log(`[Engine Manager] Child process closed with code ${code}`);
                // update the engine's state
                updateEngineState(false);
                // attemp to restart engine after all io stream pipes were closed
                restartEngine();
            }
        });
    }
}

// closing the engine
export function killEngine() {

    // The subprocess.exitCode property indicates the exit code
    // of the child process. If the child process is still running,
    // the field will be null
    //
    if (_engine?.exitCode === null) {

        // calling ChildProcess.kill() without params sends the process
        // SIGTERM (terminate signal) via the OS, and returns true if successful
        //
        if (!_engine.kill()) {
            console.log(`Couldn't terminate engine process...`);
        }
    }
}

// return the assigned port number of the engine instance
export const getAssignedPort = (): number => _assignedPort;

// returns the current state of the engine (running/down)
export const getEngineState = (): boolean => _engineRunning;

// function to update the engine state and push to the renderer process
function updateEngineState(state: boolean) {
    _engineRunning = state;
    // noify the client if the browser window is open
    if (_browserWindow)
        pushToClient(_browserWindow, 'engine-state-updated', _engineRunning);
}

// callback to return a promise of a delay
const sleep = (ms: number) => new Promise(resolve => setTimeout(resolve, ms));
const _restartDelay = 2000;//ms
const _maxAttempts = 5;

// attempts to restart the engine
export async function restartEngine(): Promise<boolean> {
    if (!_engineRunning) {

        let attempts: number = 0;
        let success: boolean = false;        

        while (!success && attempts < _maxAttempts) {
            // await delay before trying to restart again too fast
            await sleep(_restartDelay);

            // attemp to start the engine
            success = startEngine(_browserWindow);
            if (!success) 
                attempts++;
        }
        // if succeeded update the renderer
        if (success) {
            updateEngineState(true);
            return success;
        }        
    }
    console.error('[Main Process] Failed to restart the Engine process ...');
    return false;
}
