import { BrowserWindow } from 'electron';
import { pushNotification } from '../../shared/ipc/serverToClient';

// defines a general and flexible dispatcher for any type of ipc
// push notification from the main to the renderer process
//
export default function pushToClient<T extends keyof pushNotification>(
    window: BrowserWindow,
    method: T,
    payload: pushNotification[T]['payload']
): void {
    // dispatches an ipc message from the main to the renderer process
    window.webContents.send(method, payload);
}
