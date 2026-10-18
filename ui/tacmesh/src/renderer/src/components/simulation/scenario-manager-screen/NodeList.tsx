import { useEffect, useState } from "react";
import { AgentInfo } from "../../../../../shared/types/agentInfo";
import NodeListItem from './NodeListItem';
import '../../../assets/styles/component-styles/agentListStyle.css';


// define the node list component props for the type 
interface NodeListProps {
    expandedFlag: boolean
}

export default function NodeList({ expandedFlag }: NodeListProps) {

    // keep node list state in memory
    const [nodeList, updateNodeList] = useState<Array<AgentInfo>>([
        { nodeId: "Node-01", port: 51001, processId: 10101010 },
        { nodeId: "John Doe III", port: 51002, processId: 10101001010110 }
    ]);

    useEffect(() => {
        // attach an node-received event listener
        const onNodeReceived = (event: Event) => {

            // cast the received window event and pull the detail
            const e = event as CustomEvent<AgentInfo>;
            const receivedNode = e.detail;

            // update the node list
            updateNodeList(prevNodes => [...prevNodes, receivedNode]);
        }

        // attach the listener
        window.addEventListener('display-agent', onNodeReceived);

        // cleanup
        return () => {
            window.removeEventListener('display-agent', onNodeReceived);
        };
    }, []);

    return (
        <>
            <ol className="agent-list">
                {nodeList.map((node) => (
                    <NodeListItem key={node.nodeId} node={node} expandedFlag={expandedFlag} />
                ))}
            </ol>
        </>
    );
}
