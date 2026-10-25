import EngineList from "./scenario-manager-screen/NodeList";
import '../../assets/styles/component-styles/sidebarStyle.css';
import '../../assets/styles/base.css';
import { useState } from "react";

export default function Sidebar() {

    // keep engine count
    const [count, setCount] = useState<number>(0);

    // boolean to determine if the sidebar is expanded or collapsed
    const [expanded, setExpanded] = useState<boolean>(true);

    // click handler to expand/collapse the sidebar
    const toggleSidebar = () => setExpanded(!expanded);

    // increment or decrement engine count when received


    return (
        <>
            <div className={`sidebar-container ${expanded ? 'expanded' : 'collapsed'}`}>
                <div className="title-container">
                    <span className="sidebar-title">{`${expanded ? 'Engine List' : ''}`}</span>
                    <span className="engine-count">{`${expanded ? `Total: (${count})` : `(${count})`}`}</span>
                </div>
                <hr />
                <EngineList expandedFlag={expanded} />
                <hr className="bottom-divider" />
                <button className="expand-collapse-btn" onClick={toggleSidebar}>{`${expanded ? '<' : '>'}`}</button>
            </div>
        </>
    );
}
