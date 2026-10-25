import { EngineInfo } from "../../../../../shared/types/engineInfo";
import '../../../assets/styles/component-styles/nodeListItemStyle.css';
import { useState } from "react";

// define the list item props
interface ItemProps {
    node: EngineInfo
    expandedFlag: boolean
}

export default function EngineListItem({ node, expandedFlag }: ItemProps) {

    // flag to expand or collapse the list item options
    const [openMenu, setOpenMenu] = useState<boolean>(false);

    // handle the list item click -> trigger dropdown menu
    //
    const handleItemClick = () => {
        // update the openMenu flag if the sidebar is extended
        if (expandedFlag)
            setOpenMenu(!openMenu);
    }

    const pushToLauncher = (input: string) => {
        // push to launcher using the pushToLauncher API 
        // from the preload script        
        window.electronAPI.pushNotification('launcher-stdin', { stdInput: input });
    }

    // dropdown menu click handlers
    //
    const onStartProcess = () => {
        try {
            // start command: 'start <node-id>'
            const input: string = `start ${node.nodeId}`;
            pushToLauncher(input);
            setOpenMenu(false); // update the openMenu flag
        }
        catch (error) {
            console.error(`error caught while starting process: \"${error}\"`);
        }
    }
    const onKillProcess = () => {
        try {
            // kill command: 'kill <node-id>'
            const input: string = `kill ${node.nodeId}`;
            pushToLauncher(input);
            setOpenMenu(false); // update the openMenu flag
        }
        catch (error) {
            console.error(`error caught while terminating process: \"${error}\"`);
        }
    }
    const onChgLink = () => {

        // to do ...

        // change link range command: 'link-r <node-id> <range>'
        // const input: string = `link-r ${node.nodeId} ${}`;
        // pushToLauncher(input);

        setOpenMenu(false); // update the openMenu flag
    }

    return (
        <li className="list-item-container">

            <div className={`item-content ${expandedFlag ? '' : 'collapsed'}`} onClick={handleItemClick}>
                {/* if the sidebar is expanded show regular list */
                    expandedFlag ? (
                        <>
                            {/* node id and port on the same baseline */}
                            <div className="node-info-container">
                                <span className="node-id">NodeID: {node.nodeId}</span>
                                <span className="node-port">port: {node.port}</span>
                            </div>

                            {/* process id on the right of the li */}
                            <span className="node-pid">PID: {node.processId}</span>
                        </>
                    ) : (
                        <>
                            {/* show initials if collapsed sidebar */}
                            <span>{`${getNodeInitials(node.nodeId)}`}</span>
                        </>
                    )
                }
            </div>

            {/* show dropdown menu only when expanded sidebar */
                (expandedFlag && openMenu) && (
                    <div className="dropdown-menu">
                        <button className="dropdown-btn" style={{ borderBottom: "1px solid #424242" }} onClick={onStartProcess}>Start Process</button>
                        <button className="dropdown-btn" style={{ borderBottom: "1px solid #424242" }} onClick={onKillProcess}>Terminate Process</button>
                        <button className="dropdown-btn" onClick={onChgLink}>Change Link Range</button>
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
