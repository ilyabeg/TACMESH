// define the shared engine information type
//
export type EngineInfo = {
    NodeId: string,
    ProcessId: string,
    Port: number,  
    down?: boolean, // determines if the engine process is down or not (not mandatory)  
}
