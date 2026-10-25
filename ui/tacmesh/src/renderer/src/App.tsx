import { useEffect, useState } from "react";
import useServerEventHandlers from './hooks/serverEventHandler.hook';
import './assets/styles/main.css';
import invokeServer from "./ipc/invokeServer";
import { onServerEvent } from "./ipc/onServerEvent";
import Header from "./components/Header";
import Sidebar from "./components/simulation/Sidebar";
import Canvas from './components/simulation/scenario-manager-screen/Canvas';

function App(): React.JSX.Element {
  // use server event handlers
  useServerEventHandlers();

  // remember engine state (running/down)
  const [engineRunning, setEngineRunning] = useState<boolean>(false);

  // on engine info recevied
  useEffect(() => {

    // ASK for the engine state when the renderer process starts up...
    const getEngineState = async () => {
      try {
        const engineState = await invokeServer('get-engine-state', undefined);
        setEngineRunning(engineState);
      }
      catch (error) {
        console.error(`Could not get the engine state due to: ${error}`);
      }
    }

    // ask for the engine's state when the renderer starts up
    getEngineState();

    // engine state update
    onServerEvent('engine-state-updated', (_event, newState: boolean) => {
      setEngineRunning(newState);
    }, false);

    // window event handler to display engine
    const handleDisplayEngine = (event: Event) => {
      // const e = event as CustomEvent<EngineInfo>;
    }

    // attach the custom event listeners to the window    
    window.addEventListener('display-engine', handleDisplayEngine);

    // remove listeners on component re-mount
    return () => {
      window.removeEventListener('display-engine', handleDisplayEngine);
    };
  }, []);

  return (
    <>
      <Header />
      <div className="body-container">
        <Sidebar />
        <Canvas />
      </div>
    </>
  )
}

export default App
