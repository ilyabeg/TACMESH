import { EngineProcess } from '../types/engineProcess'
import { NodeLocation } from '../types/engineLocation'
import { NodeConfigs } from '../types/nodeConfigs'
import { ServerError } from '../types/serverError'

// define push notification 'events' from the main process 
// to the renderer
//
export interface pushNotification {
    'error': {
        payload: ServerError
    },
    'return-engines': {
        payload: Array<EngineProcess>
    }   
    'engine-up': {
        payload: EngineProcess
    },
    'engine-down': {
        payload: string
    },
    'engine-update': {
        payload: {
            engineId: string,
            location: NodeLocation
        }
    },
    'configs-ack': {
        payload: {
            engineId: string,
            configs: NodeConfigs
        }
    },
}
