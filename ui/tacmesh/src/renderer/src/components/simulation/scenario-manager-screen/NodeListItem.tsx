import { EngineInfo } from "../../../../../shared/types/engineInfo";
import { StdIOMessage } from "../../../../../shared/types/stdIOMessage";
import '../../../assets/styles/component-styles/nodeListItemStyle.css';
import { useState } from "react";

// define the list item props
interface ItemProps {
    engine: EngineInfo
    expandedFlag: boolean
}

export default function EngineListItem({ engine, expandedFlag }: ItemProps) {

    // flag to expand or collapse the list item options
    const [openMenu, setOpenMenu] = useState<boolean>(false);

    // handle the list item click -> trigger dropdown menu
    const handleItemClick = () => {
        // update the openMenu flag if the sidebar is extended
        if (expandedFlag)
            setOpenMenu(!openMenu);
    }

    // push to launcher using the pushToLauncher API 
    // from the preload script   
    const pushToLauncher = (input: string) => {
        try {
            window.electronAPI.pushNotification('launcher-stdin', { stdInput: input });
        }
        catch (error) {
            console.error(`error caught while trying to push a io msg to the launcher: \"${error}\"`);
        }
    }


    // --- constant ipc commands to the launcher that reffer to this node ---

    // start engine command
    const startEngineMsg: StdIOMessage = { Type: 'StartEngine', Payload: engine.NodeId }
    const jsonStartMsg: string = JSON.stringify(startEngineMsg);

    // kill engine command
    const killEngineMsg: StdIOMessage = { Type: 'KillEngine', Payload: engine.NodeId }
    const jsonkillMsg: string = JSON.stringify(killEngineMsg);

    // advance engine node command
    const advEngineMsg: StdIOMessage = { Type: 'AdvanceEngine', Payload: null }
    const jsonAdvMsg: string = JSON.stringify(advEngineMsg);


    // --- dropdown menu click handlers ---

    const onStartProcess = () => {
        pushToLauncher(jsonStartMsg);
        setOpenMenu(false); // close menu on click
    }
    const onKillProcess = () => {
        pushToLauncher(jsonkillMsg);
        setOpenMenu(false);
    }
    const onAdvance = () => {
        pushToLauncher(jsonAdvMsg);
        setOpenMenu(false);
    }

    return (
        <li className="list-item-container">

            <div className={`item-content ${expandedFlag ? '' : 'collapsed'}`} onClick={handleItemClick}>
                {/* if the sidebar is expanded show regular list */
                    expandedFlag ? (
                        <>
                            {/* node id and port on the same baseline */}
                            <div className="node-info-container">
                                <span className="node-id">NodeID: {engine.NodeId}</span>
                                {(!engine.down) && <span className="node-port">port: {engine.Port}</span>}
                            </div>

                            {/* process id on the right of the li */}
                            {(!engine.down) && <span className="node-pid">PID: {engine.ProcessId}</span>}

                            {/* show engine down when the process is terminated */}
                            {(engine.down && <span className="node-down-card">down</span>)}
                        </>
                    ) : (
                        <>
                            {/* show initials if collapsed sidebar */}
                            <span>{`${getNodeInitials(engine.NodeId)}`}</span>
                        </>
                    )
                }
            </div>

            {/* show dropdown menu only when expanded sidebar */
                (expandedFlag && openMenu) && (
                    <div className="dropdown-menu">
                        <button className="dropdown-btn" style={{ borderBottom: "1px solid #424242" }} onClick={onStartProcess}>Start Process</button>
                        <button className="dropdown-btn" style={{ borderBottom: "1px solid #424242" }} onClick={onKillProcess}>Terminate Process</button>
                        <button className="dropdown-btn" onClick={onAdvance}>Advance Node</button>
                    </div>
                )
            }
        </li>
    );
}


// helper function to pull node initials from his node id
function getNodeInitials(nodeId: string): string {

    let initials: string = "";
    const _firstLetterindex = 0;

    // check if the node id is made up of more that 1 word
    if (nodeId.includes(' ')) {

        // split via the space and take the first 2 "names"
        const parts: string[] = nodeId.trim().split(' ');
        const firstName = parts[0];
        const lastName = parts[1];

        // get the two first characters of the names
        const char1 = firstName.charAt(_firstLetterindex);
        const char2 = lastName.charAt(_firstLetterindex);

        // concatenate the two letters
        initials = char1.concat(char2).toUpperCase();
    }
    else {
        // if the name is a single "word" simply get the 1st letter
        initials = nodeId.trim().charAt(_firstLetterindex).toUpperCase();
    }

    return initials;
}
