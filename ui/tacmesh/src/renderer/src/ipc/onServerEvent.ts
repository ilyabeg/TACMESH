import { IpcRendererEvent } from 'electron';
import { pushNotification } from '../../../shared/ipc/serverToClient';


// attaches a callback handler to notification from main process. and returns the 
// cleanup method (removes the listener).
//
// Note: the reason for returning the on/once events is because the contextBridge
// creates a copy of the handler passed to the on() attachers in memory.
// when passing the same handler to removeListener(), the context bridge creates
// a brand new copy. because the new copy's memory address does not match the original
// copy's address, Electron does not recognize it. the listener is never removed, causing
// memory leaks.
// the vite-electron toolkit author (alex8088) overrode the electron ipcRenderer.on() 
// method to return the safe cleanup method to the exact same copy of the handler in memory.
//
// so when using the onServerEvent method in a useEffect(), in the return block we run the 
// returned 'unsubscriber' nethod to cleanup safely on renders
//
export function onServerEvent<T extends keyof pushNotification>(
  method: T,
  handler: (
    _event: IpcRendererEvent,
    payload: pushNotification[T]['payload']
  ) => void,
  onceFlag: boolean
): () => void {

  if (onceFlag)
    return window.electron.ipcRenderer.once(method, handler);
  else
    return window.electron.ipcRenderer.on(method, handler);
}

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
