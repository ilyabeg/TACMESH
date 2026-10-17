import { useState } from "react";
import { AgentInfo } from "../../../shared/types/agentInfo";
import '../assets/styles/component-styles/agentListStyle.css';

export default function AgentList() {

    // keep agents list state in memory
    const [agentList, updateAgentList] = useState<Array<AgentInfo>>([
        { nodeId: "Node-01", port: 51001, processId: 10101010 },
        { nodeId: "Node-02jjjjjjjjjjjjjjjjjjjjjjjjjjjjj", port: 51002, processId: 10101001010110 }
    ]);

    return (
        <>
            <ol className="agent-list">
                {agentList.map((agent) => (
                    <li className="list-item" key={agent.nodeId}>

                        <div className="agent-info-container">
                            <span className="agent-node-id">NodeID: {agent.nodeId}</span>
                            <span className="agent-port">port: {agent.port}</span>
                        </div>

                        <span className="agent-pid">PID: {agent.processId}</span>

                    </li>
                ))}
            </ol>
        </>
    );
}