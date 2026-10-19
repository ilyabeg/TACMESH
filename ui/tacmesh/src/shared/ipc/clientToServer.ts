
// define the 'events' the renderer process invokes
// to the main process
//
export interface ipcMethods {
    'get-agent-port': {
        'request': void,
        'response': number
    },
    'get-agent-state': {
        'request': void,
        'response': boolean
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
