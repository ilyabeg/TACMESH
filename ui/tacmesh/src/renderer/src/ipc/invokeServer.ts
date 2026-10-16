import { ipcMethods } from '../../../shared/ipc/clientToServer';

// defines a general and flexible dispatcher for any type of ipc
// method from the renderer to the main process
//
export default async function invokeServer<T extends keyof ipcMethods>(
    method: T,
    params: ipcMethods[T]['request']
): Promise<ipcMethods[T]['response']> {
    return await window.electron.ipcRenderer.invoke(method, params);
}