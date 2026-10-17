import AgentList from "./AgentList";
import '../assets/styles/component-styles/sidebarStyle.css';
import '../assets/styles/base.css';
import { useState } from "react";

export default function Sidebar() {

    // keep agent count
    const [count, setCount] = useState<number>(0);

    // boolean to determine if the sidebar is expanded or collapsed
    const [expanded, setExpanded] = useState<boolean>(true);

    // click handler to expand/collapse the sidebar
    const toggleSidebar = () => setExpanded(!expanded);

    return (
        <>
            <div className={`sidebar-container ${expanded ? 'expanded' : 'collapsed'}`}>
                <div className="title-container">
                    <span className="sidebar-title">Agent List</span>
                    <span className="agent-count">Total: ({count})</span>
                </div>
                <hr />
                <AgentList />
                <hr className="bottom-divider" />
                <button className="expand-collapse-btn" onClick={toggleSidebar} />
            </div>
        </>
    );
}
