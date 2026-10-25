import { EngineInfo } from '../types/engineInfo'
import { ServerError } from '../types/serverError'

// define push notification 'events' from the main process 
// to the renderer
//
export interface pushNotification {
    'error': {
        payload: ServerError
    },
    'ReturnEngines': {
        payload: Array<EngineInfo>
    }   
    'EngineUp': {
        payload: EngineInfo
    },    
    'EngineDown': {
        payload: string
    },
    'EngineUpdate': {
        payload: {
            engineId: string,
            // location: Location
        }
    },
}
