import { EngineInfo } from '../types/engineInfo'
import { ServerError } from '../types/serverError'

// define push notification 'events' from the main process 
// to the renderer
//
export interface pushNotification {
    'error': {
        payload: ServerError
    },
    'return-engines': {
        payload: Array<EngineInfo>
    }   
    'engine-up': {
        payload: EngineInfo
    },
    'engine-down': {
        payload: string
    },
    'engine-update': {
        payload: {
            engineId: string,
            // location: Location
        }
    },
}
