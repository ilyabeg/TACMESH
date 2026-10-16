import { IpcMain, IpcMainInvokeEvent } from 'electron';
import { ipcMethods } from '../../shared/ipc/clientToServer';

// defines a general event dispatcher for events that were sent from the
// renderer to the main process
//
export default function dispatchEvent<T extends keyof ipcMethods>(
    ipcMain: IpcMain,
    method: T,
    handler: (params: ipcMethods[T]['request']) => Promise<ipcMethods[T]['response']>
): void {
    type Request = ipcMethods[T]['request'];
    type Response = ipcMethods[T]['response'];

    // handle the ipc message using a callback wrapper to keep the handler
    // function clean and independent.
    //  
    ipcMain.handle(
        method, 
        async (_event: IpcMainInvokeEvent, params: Request): Promise<Response> => {
            return handler(params);
        }
    );
}