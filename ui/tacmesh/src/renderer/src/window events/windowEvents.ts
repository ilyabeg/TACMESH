import { EngineProcess } from "../../../shared/types/enginePeocess"

// define custom window events for the renderer process
//
export default interface CustomWindowEvents {
    'display-engine': EngineProcess,
}
