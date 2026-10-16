
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
