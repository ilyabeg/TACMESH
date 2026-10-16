import { AgentInfo } from "../../../shared/types/agentInfo"

// define custom window events for the renderer process
//
export default interface CustomWindowEvents {
    'display-agent': AgentInfo,
}
