import { NodeConfigs } from "./nodeConfigs"

// define the shared engine process type
//
export type EngineProcess = {
    NodeId: string,
    ProcessId: string,
    Port: number,  
    down?: boolean, // determines if the engine process is down or not (not mandatory)  
    configs?: NodeConfigs,
}
