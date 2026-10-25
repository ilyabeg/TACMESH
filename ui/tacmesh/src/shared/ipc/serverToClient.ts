import { EngineInfo } from '../types/engineInfo'
import { ServerError } from '../types/serverError'

// define push notification 'events' from the main process 
// to the renderer
//
export interface pushNotification {
    'error': {
        payload: ServerError
    },
    'engine-received': {
        payload: EngineInfo
    },    
    'engine-state-updated': {
        payload: boolean
    },    
}
