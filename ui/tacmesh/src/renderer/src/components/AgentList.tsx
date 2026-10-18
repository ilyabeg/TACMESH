import { useEffect, useState } from "react";
import { AgentInfo } from "../../../shared/types/agentInfo";
import '../assets/styles/component-styles/agentListStyle.css';


// define the agent list component props for the type 
interface AgentListProps {
    expandedFlag: boolean
}

export default function AgentList({ expandedFlag }: AgentListProps) {

    // keep agents list state in memory
    const [agentList, updateAgentList] = useState<Array<AgentInfo>>([
        { nodeId: "Node-01", port: 51001, processId: 10101010 },
        { nodeId: "John Doe III", port: 51002, processId: 10101001010110 }
    ]);

    useEffect(() => {
        // attach an agent-received event listener
        const onAgentReceived = (event: Event) => {

            // cast the received window event and pull the detail
            const e = event as CustomEvent<AgentInfo>;
            const receivedAgent = e.detail;

            // update the agent list
            updateAgentList(prevAgents => [...prevAgents, receivedAgent]);
        }

        // attach the listener
        window.addEventListener('display-agent', onAgentReceived);

        // cleanup
        return () => {
            window.removeEventListener('display-agent', onAgentReceived);
        };
    }, []);

    return (
        <>
            <ol className="agent-list">
                {agentList.map((agent) => (
                    <li className={`list-item ${expandedFlag ? '' : 'collapsed'}`} key={agent.nodeId}>

                        {/* if the sidebar is expanded show regular list */
                            expandedFlag ? (
                                <>
                                    {/* agent id and port on the same baseline */}
                                    <div className="agent-info-container">
                                        <span className="agent-node-id">NodeID: {agent.nodeId}</span>
                                        <span className="agent-port">port: {agent.port}</span>
                                    </div>

                                    {/* process id on the right of the li */}
                                    <span className="agent-pid">PID: {agent.processId}</span>
                                </>
                            ) : (
                                <>
                                    {/* show initials if collapsed sidebar */}
                                    <span className="agent-initials">{`${getAgentInitials(agent.nodeId)}`}</span>
                                </>
                            )
                        }

                    </li>
                ))}
            </ol>
        </>
    );
}


// helper function to pull agent initials from his node id
function getAgentInitials(nodeId: string): string {

    let initials: string = "";
    const _firstLetterindex = 0;

    // check if the agent id is made up of more that 1 word
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