import { useCallback, useEffect, useState } from "react"
import { EngineProcess } from "../../../shared/types/engineProcess"
import { onServerEvent } from "@renderer/ipc/onServerEvent";
import { IpcRendererEvent } from "electron/main";
import { NodeConfigs } from "../../../shared/types/nodeConfigs";
import { NodeLocation } from "../../../shared/types/engineLocation";


// custom launcher events handlers hook to provide
// easy and readable launcher and engines state handling
//
export default function useLauncherEventHandlers() {

    // keep engines state in memory
    const [engines, setEngines] = useState<Array<EngineProcess>>([]);

    // used usecallback here because React would have ran the useEffect every
    // time a dependency from it's dependency array is updated, meaning 
    // attach and remove the main-process event listeners each time the state changes
    
    
    // --- create the handlers ---

    // handle the full engines array change
    const changeEngines = useCallback(
        (_event: IpcRendererEvent, payload: Array<EngineProcess>) => { 
            setEngines(payload); 
        }, 
    []);

    // handle engine process started
    const engineUp = useCallback(
        (_event: IpcRendererEvent, payload: EngineProcess) => {

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
        (_event: IpcRendererEvent, payload: { NodeId: string, Location: NodeLocation }) => {
            
            // spread the previous engine state and change the new location in the configs
            //
            setEngines(prev => prev.map(
                engine => engine.NodeId === payload.NodeId // the engine nodeId matches the event id
                    ? { ...engine, configs: { ...engine.configs!, Location: payload.Location } }
                    : engine // if doesn't match id, keep as is
            ));
        },
    []);

    // handle engine configs acknowledge
    const configsAck = useCallback(
        (_event: IpcRendererEvent, payload: { NodeId: string, Configs: NodeConfigs }) => {

            // update specific engine process configs
            //
            setEngines(prev => prev.map(
                engine => engine.NodeId === payload.NodeId
                    ? {...engine, configs: payload.Configs } // replace new configs
                    : engine // if doesn't match id, keep as is
            ));
        },
    []);


    // --- attach the handlers for the launcher events ---
    useEffect(() => {

        const offReturnEngines = onServerEvent('return-engines', changeEngines, false); // once-flag: false
        const offEngineUp = onServerEvent('engine-up', engineUp, false);
        const offEngineDown = onServerEvent('engine-down', engineDown, false);
        const offEngineUpdate = onServerEvent('engine-update', engineUpdate, false);
        const offConfigsAck = onServerEvent('configs-ack', configsAck, false);

        return () => {
            // remove the handlers when re-rendered
            offReturnEngines();
            offEngineUp();
            offEngineDown();
            offEngineUpdate();
            offConfigsAck();
        }
    }, [changeEngines, engineUp, engineDown, engineUpdate, configsAck]);
    

    // return engines state on render
    return engines;
}
