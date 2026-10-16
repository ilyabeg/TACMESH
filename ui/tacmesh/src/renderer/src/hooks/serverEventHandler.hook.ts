import { useCallback, useEffect } from 'react';
import type { IpcRendererEvent } from 'electron';
import { onServerEvent, removeAllListenersToServerEvent } from '../ipc/onServerEvent';
import dispatchWindowEvent from '../window events/windowEventDispatcher';
import { AgentInfo } from '../../../shared/types/agentInfo';
import { ServerError } from '../../../shared/types/serverError';

// custom hook to re-use server event handlers anywhere
//
export default function useServerEventHandlers() {

  // callback handler to display main process errors
  const onServerError = useCallback(
    (_event: IpcRendererEvent, err: ServerError) => {
      console.error(`Error from server: ${err.message}`);
    }, []);

  // callback handler to display agent in the UI
  const onAgentInfoReceived = useCallback(
    (_event: IpcRendererEvent, agentInfo: AgentInfo) => {

      // render the agent in the ui
      dispatchWindowEvent('display-agent', agentInfo);
    }, []);

  // clean up on re-render
  useEffect(() => {

    // attach event handler for server error event from main process
    onServerEvent('error', onServerError, false); // once-flag: false

    // attach event handler for agent received event from main process
    onServerEvent('agent-received', onAgentInfoReceived, false); // once-flag: false

    // a 'promise' that these functions will be disposed when the 
    // component unmounts/re-renders
    //
    return () => {

      // TEMPORARILY remove ALL event listeners for these events until
      // I figure out how to implement an off server event
      //
      removeAllListenersToServerEvent('error');
      removeAllListenersToServerEvent('agent-received');
    };
  }, [onServerError, onAgentInfoReceived]);
}
