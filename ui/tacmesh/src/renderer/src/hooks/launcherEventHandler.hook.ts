import { useCallback, useEffect, useState } from "react"
import { EngineInfo } from "../../../shared/types/engineInfo"
import { onServerEvent, removeAllListenersToServerEvent } from "@renderer/ipc/onServerEvent";
import { IpcRendererEvent } from "electron/main";


// custom launcher events handlers hook to provide
// easy and readable launcher and engines state handling
//
export default function useLauncherEventHandlers() {

    // keep engines state in memory
    const [engines, setEngines] = useState<Array<EngineInfo>>([]);

    // used usecallback here because React would have ran the useEffect every
    // time a dependency from it's dependency array is updated, meaning 
    // attach and remove the main-process event listeners each time the state changes
    

    // --- create the handlers ---

    // handle the full engines array change
    const changeEngines = useCallback(
        (_event: IpcRendererEvent, payload: Array<EngineInfo>) => { 
            setEngines(payload); 
        }, 
    []);

    // handle engine process started
    const engineUp = useCallback(
        (_event: IpcRendererEvent, payload: EngineInfo) => {

            // map the previous engine's state to the array but with the
            // specific new engine process information
            //
            setEngines(prev => prev.map(
                engine => engine.NodeId === payload.NodeId // the engine nodeId matches the event id
                    ? payload // set as the new engine info
                    : engine  // if doesn't match id, keep as is
            ));
        },
    []);

    // handle engine preocess terminated
    const engineDown = useCallback(
        (_event: IpcRendererEvent, payload: string) => {
            
            // map the previous engine's state to the array but with the
            // specific 'down' engine process marked as down
            //
            setEngines(prev => prev.map(
                engine => engine.NodeId === payload // the engine nodeId matches the event id
                    ? { ...engine, down: true } // spread the engine and mark it as down
                    : engine // if doesn't match id, keep as is
            ));
        },
    []);

    // handle engine update
    const engineUpdate = useCallback(
        (_event: IpcRendererEvent, payload: { engineId: string, /* location: Location */ }) => {
            // to do ...
        },
    []);


    // --- attach the handlers for the launcher events ---
    useEffect(() => {

        const offReturnEngines = onServerEvent('return-engines', changeEngines, false); // once-flag: false
        const offEngineUp = onServerEvent('engine-up', engineUp, false);
        const offEngineDown = onServerEvent('engine-down', engineDown, false);
        const offEngineUpdate = onServerEvent('engine-update', engineUpdate, false);

        return () => {
            // remove the handlers when re-rendered
            offReturnEngines();
            offEngineUp();
            offEngineDown();
            offEngineUpdate();
        }
    }, [changeEngines, engineUp, engineDown, engineUpdate]);
    

    // return engines state on render
    return engines;
}
