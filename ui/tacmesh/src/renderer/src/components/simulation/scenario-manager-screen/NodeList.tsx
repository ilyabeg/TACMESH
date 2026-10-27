import { EngineInfo } from "../../../../../shared/types/engineInfo";
import NodeListItem from './NodeListItem';
import '../../../assets/styles/component-styles/engineListStyle.css';


// define the node list component props for the type 
interface NodeListProps {
    listItems: Array<EngineInfo>
    expandedFlag: boolean
}

export default function NodeList({ listItems, expandedFlag }: NodeListProps) {

    // map the list items from the props to the sidebar list
    return (
        <>
            <ol className="engine-list">
                {listItems.map(item => (
                    <NodeListItem key={item.NodeId} engine={item} expandedFlag={expandedFlag} />
                ))}
            </ol>
        </>
    );
}
