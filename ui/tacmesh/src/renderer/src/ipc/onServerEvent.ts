import { IpcRendererEvent } from 'electron';
import { pushNotification } from '../../../shared/ipc/serverToClient';

// Note: In Electron-Vite, functions such as listenerCount() or off() aren't defined,
// so for now I will not implement a listeener count or off server method until i figure
// out how to do it correctly.

// attaches a callback handler to notification from main process
//
export function onServerEvent<T extends keyof pushNotification>(
  method: T,
  handler: (
    _event: IpcRendererEvent,
    payload: pushNotification[T]['payload']
  ) => void,
  onceFlag: boolean
): void {
  if (onceFlag) {
    window.electron.ipcRenderer.once(method, handler);
  } else {
    window.electron.ipcRenderer.on(method, handler);
  }
}

// removes a specific server event handler
//
// export function offServerEvent<T extends keyof pushNotification>(
//   method: T,
//   handler: (
//     _event: IpcRendererEvent,
//     payload: pushNotification[T]['payload']
//   ) => void
// ): void {
//   window.electron.ipcRenderer.off(method, handler);
// }

// removes all active listeners attached to a specific notification event
//
export function removeAllListenersToServerEvent<T extends keyof pushNotification>
  (method: T): void {
  return window.electron.ipcRenderer.removeAllListeners(method);
}

// debugging tool to see how many listeners are currently attached to 
// the specific notification event
//
// export function listenerCount<T extends keyof pushNotification>
//   (method: T): number {
//   return window.electron.ipcRenderer.listenerCount(method);
// }
