import { EngineProcess } from "../../../../../shared/types/enginePeocess";
import EngineListItem from './EngineListItem';
import '../../../assets/styles/component-styles/engineListStyle.css';


// define the node list component props for the type 
interface NodeListProps {
    listItems: Array<EngineProcess>
    expandedFlag: boolean
}

export default function NodeList({ listItems, expandedFlag }: NodeListProps) {

    // map the list items from the props to the sidebar list
    return (
        <>
            <ol className="engine-list">
                {listItems.map(item => (
                    <EngineListItem key={item.NodeId} engine={item} expandedFlag={expandedFlag} />
                ))}
            </ol>
        </>
    );
}
