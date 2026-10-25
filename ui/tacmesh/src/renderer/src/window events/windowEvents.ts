import { EngineInfo } from "../../../shared/types/engineInfo"

// define custom window events for the renderer process
//
export default interface CustomWindowEvents {
    'display-engine': EngineInfo,
}
