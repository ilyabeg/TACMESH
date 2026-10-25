import { useEffect, useState } from "react";
import { EngineInfo } from "../../../../../shared/types/engineInfo";
import NodeListItem from './NodeListItem';
import '../../../assets/styles/component-styles/engineListStyle.css';
import invokeServer from "@renderer/ipc/invokeServer";


// define the node list component props for the type 
interface NodeListProps {
    expandedFlag: boolean
}

export default function NodeList({ expandedFlag }: NodeListProps) {

    // keep node list state in memory
    const [nodeList, updateNodeList] = useState<Array<EngineInfo>>([
        { nodeId: "Node-01", processId: "101010", port: 51001 }
    ]);

    useEffect(() => {

        // attach an event listener for node updates
        const onNodeUpdate = (event: Event) => {

            // cast the received window event and pull the detail
            const e = event as CustomEvent<EngineInfo>;
            const receivedNode = e.detail;

            // update the node list
            updateNodeList(prevNodes => [...prevNodes, receivedNode]);
        }

        // ask for the nodes from the main process
        // const getNodes = async () => await invokeServer('get-node-processes',);

        // execute the client to server event
        // getNodes();

        // attach the listener
        window.addEventListener('display-engine', onNodeUpdate);

        // cleanup
        return () => {
            window.removeEventListener('display-engine', onNodeUpdate);
        };
    }, []);

    return (
        <>
            <ol className="engine-list">
                {nodeList.map((node) => (
                    <NodeListItem key={node.nodeId} node={node} expandedFlag={expandedFlag} />
                ))}
            </ol>
        </>
    );
}
