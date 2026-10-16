import { useEffect, useState } from "react";
import useServerEventHandlers from './hooks/serverEventHandler.hook';
import { AgentInfo } from "../../shared/types/agentInfo";
import invokeServer from "./ipc/invokeServer";
import { onServerEvent } from "./ipc/onServerEvent";

function App(): React.JSX.Element {
  // use server event handlers
  useServerEventHandlers();

  // remember agent state (running/down)
  const [agentRunning, setAgentRunning] = useState<boolean>(false);

  // on agent info recevied
  useEffect(() => {

    // ASK for the agent state when the renderer process starts up...
    const getAgentState = async () => {
      try {
        const agentState = await invokeServer('get-agent-state', undefined);
        setAgentRunning(agentState);
      }
      catch (error) {
        console.error(`Could not get the agent state due to: ${error}`);
      }
    }

    // ask for the agent's state when the renderer starts up
    getAgentState();

    // agent state update
    onServerEvent('agent-state-updated', (_event, newState: boolean) => {
      setAgentRunning(newState);
    }, false);

    // window event handler to display agent
    const handleDisplayAgent = (event: Event) => {
      // const e = event as CustomEvent<AgentInfo>;

      // Note: For now, no agent display exists, so this event will indicate if the
      // agent is running...
      setAgentRunning(true);
    }

    // attach the custom event listeners to the window    
    window.addEventListener('display-agent', handleDisplayAgent);

    // remove listeners on component re-mount
    return () => {
      window.removeEventListener('display-agent', handleDisplayAgent);
    };
  }, []);

  return (
    <>
      <h1>Agent state: {agentRunning ? 'running.' : 'down.'}</h1>
    </>
  )
}

export default App
