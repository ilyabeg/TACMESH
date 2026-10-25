import { useCallback, useEffect } from 'react';
import type { IpcRendererEvent } from 'electron';
import { onServerEvent, removeAllListenersToServerEvent } from '../ipc/onServerEvent';
import dispatchWindowEvent from '../window events/windowEventDispatcher';
import { EngineInfo } from '../../../shared/types/engineInfo';
import { ServerError } from '../../../shared/types/serverError';

// custom hook to re-use server event handlers anywhere
//
export default function useServerEventHandlers() {

  // callback handler to display main process errors
  const onServerError = useCallback(
    (_event: IpcRendererEvent, err: ServerError) => {
      console.error(`Error from server: ${err.message}`);
    }, []);

  // callback handler to display engine in the UI
  const onEngineInfoReceived = useCallback(
    (_event: IpcRendererEvent, engineInfo: EngineInfo) => {

      // render the engine in the ui
      dispatchWindowEvent('display-engine', engineInfo);
    }, []);

  // clean up on re-render
  useEffect(() => {

    // attach event handler for server error event from main process
    onServerEvent('error', onServerError, false); // once-flag: false

    // attach event handler for engine received event from main process
    onServerEvent('engine-received', onEngineInfoReceived, false); // once-flag: false

    // a 'promise' that these functions will be disposed when the 
    // component unmounts/re-renders
    //
    return () => {

      // TEMPORARILY remove ALL event listeners for these events until
      // I figure out how to implement an off server event
      //
      removeAllListenersToServerEvent('error');
      removeAllListenersToServerEvent('engine-received');
    };
  }, [onServerError, onEngineInfoReceived]);
}
