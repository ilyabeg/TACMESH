
// define the structure to the Std IO IPC messages between electron
// and the launcher process
//
export type StdIOMessage = {
    Type: string,
    Payload: any
}