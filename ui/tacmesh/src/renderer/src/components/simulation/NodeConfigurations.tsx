import { useState } from 'react';
import '../../assets/styles/component-styles/nodeConfigsStyle.css'
import { EngineInfo } from '../../../../shared/types/engineInfo';
import { NodeLocation } from '../../../../shared/types/engineLocation';


interface NodeConfigsProps {
    allNodes: Array<EngineInfo>,
    nodeId: string,
}

// component for modifying the configuration settings for each engine process node
//
export default function NodeConfigurations({ allNodes, nodeId }: NodeConfigsProps) {

    // --- configs states ---

    const [id, setId] = useState<string>(nodeId);
    const [speed, setSpeed] = useState<number>(1);
    const [location, setLocation] = useState<NodeLocation>({ X: 0, Y: 0 });
    const [permittedNodes, setPermitted] = useState<Array<string>>([])
    const [waypoints, setWaypoints] = useState<Array<NodeLocation>>([]);

    // --- click handlers ---

    const validateInput = () => {

    }

    const cancelConfig = () => {

    }

    const addPermittedNode = () => {

    }

    const delPermittedNode = () => {

    }

    return (
        <div className='bg-screen'>
            <div className="configs-container">

                <div className="title-container">
                    <span className='configs-title'>Engine Process Node Configuration Settings:</span>
                </div>

                <hr className='divider-line' />

                <div className="items-container">
                    <div className="titles">
                        <span className='item-title'>Node-ID:</span>
                        <span className='item-title'>Speed:</span>
                        <span className='item-title'>Location:</span>
                        <span className='item-title'>Permitted:</span>
                        <span className='item-title'>Waypoints:</span>
                    </div>
                    <div className="inputs">

                        <div className='input-field-container'>
                            <input className='input' type='text' />
                        </div>

                        <div className='input-field-container'>
                            <div className='speed-input-container'>
                                <input className='input speed' type='number' min="1" max="6" step="1" />
                                <span className='speed-units'>m/s</span>
                            </div>
                        </div>

                        <div className='input-field-container'>
                            <div className='location-input-container'>
                                <span className='location-field'>X:</span>
                                <input className='input X' type='number' />

                                <span className='location-field'>Y:</span>
                                <input className='input Y' type='number' />
                            </div>
                        </div>

                        <div className='input-field-container'>
                            <div className='permitted-container'>
                                <select className='nodes-menu'>
                                    {allNodes.map(node => (
                                        <option key={node.NodeId} value={node.NodeId}>
                                            {node.NodeId}
                                        </option>
                                    ))}
                                </select>
                                <button className='add-btn' onClick={addPermittedNode}>add</button>
                                <button className='del-btn' onClick={delPermittedNode}>del</button>
                            </div>
                        </div>

                        <div className='input-field-container'>
                            <div className='waypoints-container'>
                                <span className='location-field'>X:</span>
                                <input className='input X' type='number' />

                                <span className='location-field'>Y:</span>
                                <input className='input Y' type='number' />
                            </div>
                        </div>

                    </div>
                </div>

                <div className="preview-container">

                    <span className='preview-title'>Preview:</span>

                    <hr className='divider-line' />

                    <div className='preview-items-container'>
                        <div className="preview-titles">
                            <span className='preview-item-title'>Node-ID:</span>
                            <span className='preview-item-title'>Speed:</span>
                            <span className='preview-item-title'>Location:</span>
                            <span className='preview-item-title'>Permitted:</span>
                            <span className='preview-item-title'>Waypoints:</span>
                        </div>

                        <div className="preview-values">
                            <span className='preview-item-value'>{id}</span>
                            <span className='preview-item-value'>{speed}</span>
                            <span className='preview-item-value'>({location.X},{location.Y})</span>
                            <span className='preview-item-value'>{(permittedNodes.length == 0) ? 'all' : permittedNodes.toString()}</span>
                            <span className='preview-item-value'>{(waypoints.length == 0) ? 'none' : waypoints.toString()}</span>
                        </div>
                    </div>
                </div>

                <div className="configs-footer-container" >
                    <button className="footer-btn" style={{ marginLeft: "auto" }} onClick={cancelConfig}>Cancel</button>
                    <button className="footer-btn" onClick={validateInput}>Done</button>
                </div>

            </div>
        </div >
    );
}
