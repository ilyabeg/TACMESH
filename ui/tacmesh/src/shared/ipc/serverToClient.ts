import { AgentInfo } from '../types/agentInfo'
import { ServerError } from '../types/serverError'

// define push notification 'events' from the main process 
// to the renderer
//
export interface pushNotification {
    'error': {
        payload: ServerError
    },
    'agent-received': {
        payload: AgentInfo
    },    
    'agent-state-updated': {
        payload: boolean
    },    
}