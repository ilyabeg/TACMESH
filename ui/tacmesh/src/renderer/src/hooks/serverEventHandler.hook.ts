import { useCallback, useEffect } from 'react';
import type { IpcRendererEvent } from 'electron';
import { onServerEvent } from '../ipc/onServerEvent';
import { ServerError } from '../../../shared/types/serverError';

// custom hook to re-use server event handlers anywhere
//
export default function useServerEventHandlers() {

  // callback handler to display main process errors
  const onServerError = useCallback(
    (_event: IpcRendererEvent, err: ServerError) => {
      console.error(`Error from server: ${err.message}`);
  }, []);


  useEffect(() => {

    // attach event handler for server error event from main process
    const offErrorrEvent = onServerEvent('error', onServerError, false); // once-flag: false

    // --- cleanup event handlers on renders ---
    return () => {

      // remove error handler on render
      offErrorrEvent();
    };
  }, [onServerError]);
}
