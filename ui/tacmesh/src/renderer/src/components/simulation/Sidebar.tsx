import EngineList from "./scenario-manager-screen/EngineList";
import '../../assets/styles/component-styles/sidebarStyle.css';
import '../../assets/styles/base.css';
import { useState } from "react";
import { EngineInfo } from "../../../../shared/types/engineInfo";


// define the props structure
type SidebarProps = {
    listItems: Array<EngineInfo>
}

export default function Sidebar({ listItems }: SidebarProps) {

    // determine engine count
    const count = listItems.length;

    // boolean to determine if the sidebar is expanded or collapsed
    const [expanded, setExpanded] = useState<boolean>(true);

    // click handler to expand/collapse the sidebar
    const toggleSidebar = () => setExpanded(!expanded);

    return (
        <>
            <div className={`sidebar-container ${expanded ? 'expanded' : 'collapsed'}`}>
                <div className="title-container">
                    <span className="sidebar-title">{`${expanded ? 'Engine List' : ''}`}</span>
                    <span className="engine-count">{`${expanded ? `Total: (${count})` : `(${count})`}`}</span>
                </div>
                <hr />
                <EngineList listItems={listItems} expandedFlag={expanded} />
                <hr className="bottom-divider" />
                <button className="expand-collapse-btn" onClick={toggleSidebar}>{`${expanded ? '<' : '>'}`}</button>
            </div>
        </>
    );
}
