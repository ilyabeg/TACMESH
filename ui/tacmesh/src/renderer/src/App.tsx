import { useEffect } from "react";
import useServerEventHandlers from './hooks/serverEventHandler.hook';
import './assets/styles/main.css';
import Header from "./components/Header";
import Sidebar from "./components/simulation/Sidebar";
import Canvas from './components/simulation/scenario-manager-screen/Canvas';
import { StdIOMessage } from "../../shared/types/stdIOMessage";
import { EngineInfo } from "../../shared/types/engineInfo";
import useLauncherEventHandlers from "./hooks/launcherEventHandler.hook";

function App(): React.JSX.Element {

  // use server event handlers
  useServerEventHandlers();

  // get the engines processes using the launcher events hook.
  // when the engines state updates the hook will automatically update and render
  // the array for the App component
  //
  const engineProcesses: Array<EngineInfo> = useLauncherEventHandlers();


  useEffect(() => {

    // when the ui starts up, it must ask for the engine nodes from the backend logic.
    // to do that, the program invokes the main process to push to the launcher into it's
    // stdinput the command to return the engine nodes. when the launcher receives that,
    // it sends an ipc message containing the engines, then the main process forwards it
    // to the renderer through another ipc message.
    //
    const askForNodes = () => {
      // build the actual stdio message
      const inputMessage: StdIOMessage = {
        Type: 'ListEngines',
        Payload: null
      }

      // convert to json format and forward to the launcher process
      const jsonMessage: string = JSON.stringify(inputMessage);
      window.electronAPI.pushNotification('launcher-stdin', { stdInput: jsonMessage });
    }
    askForNodes();

  }, []); // run effect once on mount

  return (
    <>
      <Header />
      <div className="body-container">

        {/* pass the engine processes through the component props */}
        <Sidebar listItems={engineProcesses} />
        <Canvas items={engineProcesses} />

      </div>
    </>
  )
}

export default App
