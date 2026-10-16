import { ChildProcess, spawn } from 'node:child_process';
import pushToClient from './ipc/pushToClient';
import path from 'node:path';
import { BrowserWindow } from 'electron';
import { AgentInfo } from '../shared/types/agentInfo';

let _browserWindow: BrowserWindow; // the electron window instance
let _agent: ChildProcess | null = null; // the agent process instance
let _assignedPort: number; // the assigned port of the agent instance
let _agentRunning: boolean = false; // tells if the agent process is running or down

// opening the agent
export function startAgent(window: BrowserWindow) {

    // save the browser window instance for future use
    _browserWindow = window;

    // get the absoulte relative path for the agent exe file.
    // __dirmame contains the absolute path of the directory 
    // containing the currently executing JavaScript/TypeScript file
    //
    const agent_exe = path.resolve(__dirname, '..', '..', '..', '..',
        'backend', 
        'src', 
        'TacMesh.Agent',
        'bin',
        'Debug',
        'net10.0',
        'TacMesh.Agent.exe'
    );
    // check the path
    console.log(`Path to Agent.exe: ${agent_exe}`);

    // spawn the agent process as a child process    
    _agent = spawn(agent_exe, ["Node-01", "{}", "0", "[]"]); // TEMPORARILY init agent process with default arguments

    if (_agent) {

        // indicate if the agent is running or not
        _agentRunning = true;

        // standard output received from the child process' output stream
        _agent.stdout?.on('data', (output) => {
            
            const strOutput: string = output.toString();
            console.log(`[Main Process] received output: \n${strOutput}`);

            if (strOutput.includes('PORT=')) {
                // try parse the port number 
                _assignedPort = tryParsePort(strOutput);

                if (_assignedPort != -1) {
                    console.log(`[Main Process] Agent initiated on PORT: ${_assignedPort}`);

                    // build the agent information
                    const agentInfo : AgentInfo = {
                        nodeId: "tempId", // TEMPORARILY use a temp id
                        port: _assignedPort
                    }
                    // push the agent info to renderer process
                    pushToClient(_browserWindow, 'agent-received', agentInfo);
                }
            }            
        });
        
        // error received from the child process
        _agent.stderr?.on('data', (error) => {
            console.error(`received error from child process: ${error}`);
        });

        // error received from the OS
        _agent.on('error', (error) => {
            console.error(`failed to start child process: ${error}`);
        }); 

        // exit event fires when the process teminates but the underlying
        // stream pipes might still be open
        //
        _agent.on('exit', (exit) => {
            console.log(`child process exited \"${exit}\"`);    
            // update the agent's state
            updateAgentState(false);
        });
        
        // child event fires when the process terminates and all of the 
        // stream pipes have been closed
        //
        _agent.on('close', (code) => {
            console.log(`child process closed with code ${code}`);
            // update the agent's state
            updateAgentState(false);
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

    if (!isNaN(portNumber)) {
        return portNumber;
    }
    // if failed to parse port, return -1
    return -1;
}

// closing the agent
export function killAgent() {

    // The subprocess.exitCode property indicates the exit code
    // of the child process. If the child process is still running,
    // the field will be null
    //
    if (_agent?.exitCode === null) {

        // calling ChildProcess.kill() without params sends the process
        // SIGTERM (terminate signal) via the OS, and returns true if successful
        //
        if (!_agent.kill()) {
            console.log(`Couldn't terminate agent process...`);
        }
    }
}

// return the assigned port number of the agent instance
export function getAssignedPort(): number {
    return _assignedPort;
}

// returns the current state of the agent (running/down)
export function getAgentState(): boolean {
    return _agentRunning;
}

// function to update the agent state and push to the renderer process
function updateAgentState(state: boolean): void {
    _agentRunning = state;
    // noify the client
    pushToClient(_browserWindow, 'agent-state-updated', _agentRunning);
}
