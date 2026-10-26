import { BrowserWindow } from "electron";
import { ChildProcess, spawn } from "node:child_process";
import pushToClient from "../../../ipc/pushToClient";
import { StdIOMessage } from "../../../../shared/types/stdIOMessage";
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

// std output buffer to easily pull the output up to the '\n'
// this buffer acts as a queue for the std output receivede. the reason
// it is needed is because when ipc messages are sent through the io streams
// they can sometimes get attached together with a couple of messages that
// were pushed to the stream in the same instant. 
// the buffer fixes this by splitting the messages by the new line character ('\n')
//
let _stdOutBuffer: string = '';

// standard output received from the child process' output stream
function onStdout() {
    if (_launcher) {
        _launcher.stdout?.on('data', (output) => {
            
            const strOutput: string = output.toString();
            _stdOutBuffer += strOutput; // append to the buffer

            let newLineIndex = _stdOutBuffer.indexOf('\n');

            // if the buffer contains a new line
            while (newLineIndex !== -1) {

                // get the output from the begining up to the '\n'
                const slicedOutput = _stdOutBuffer.slice(0, newLineIndex).trim();                

                // print the launcher output if couldnt parse as json
                if (!TryParseStdOut(slicedOutput))
                    console.log(`${slicedOutput}`);

                // slice off the string up to the '\n', keep string as is from the new line
                // character up to the end, and get the next index of the '\n'
                //
                _stdOutBuffer = _stdOutBuffer.slice(newLineIndex + 1);
                newLineIndex = _stdOutBuffer.indexOf('\n');
            }                                  
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


// function that parses the launcher stdoutput and determines what to do with it
function TryParseStdOut(outout: string): boolean {
    try {

        // test
        // console.log(` sudhfshdfsdfh ${outout}`);

        // parse the output message
        const outputMessage: StdIOMessage = JSON.parse(outout);

        // test
        console.log(`received output message ${outputMessage.Type}. payload: ${outputMessage.Payload}`);


        switch (outputMessage.Type) {
            // launcher pushed the engine processes
            case 'ReturnEngines': {
                pushToClient(_browserWindow, 'return-engines', outputMessage.Payload);
                break;
            }
            // engine process started
            case 'EngineUp': {
                pushToClient(_browserWindow, 'engine-up', outputMessage.Payload);
                break;
            }
            // engine process terminated
            case 'EngineDown': {
                pushToClient(_browserWindow, 'engine-down', outputMessage.Payload);
                break;
            }
            // engine process location updated
            case 'EngineUpdate': {
                pushToClient(_browserWindow, 'engine-update', outputMessage.Payload);
                break;
            }
        }
        return true;
    }
    catch (error) {
        // ignore failed attempts at parsing the json outputs

        // test
        // console.log(`couldn't parse json: ${outout} due to: \"${error}\"`);

        return false;
    }
}