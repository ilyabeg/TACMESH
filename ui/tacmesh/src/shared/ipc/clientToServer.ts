
// define the 'events' the renderer process invokes
// to the main process
//
export interface ipcMethods {
    'get-engine-port': {
        'request': void,
        'response': number
    },
    'get-engine-state': {
        'request': void,
        'response': boolean
    },
    'get-node-processes': {
        'request': void,
        'response': string
    },
}

// define client push notifications to the server that
// don't expect a response
//
export interface clientPushNotification {
    'launcher-stdin': {
        'params': {
            stdInput: string
        }
    }
}
