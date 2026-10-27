import { useRef, useState } from 'react';
import '../../assets/styles/component-styles/nodeConfigsStyle.css'
import { EngineInfo } from '../../../../shared/types/engineInfo';
import { NodeLocation } from '../../../../shared/types/engineLocation';
import { NodeConfigs, ConfigConstants, buildConfigs } from '../../../../shared/types/nodeConfigs';
import { StdIOMessage } from '../../../../shared/types/stdIOMessage';


interface NodeConfigsProps {
    allNodes: Array<EngineInfo>,
    nodeId: string,
}

// component for modifying the configuration settings for each engine process node
//
export default function NodeConfigurations({ allNodes, nodeId }: NodeConfigsProps) {

    // --- configs states ---

    const [id, setId] = useState<string>(nodeId);
    const [speed, setSpeed] = useState<number>(ConfigConstants.MIN_SPEED);
    const [location, setLocation] = useState<NodeLocation>({ X: 0, Y: 0 });
    const [linkRange, setLinkRange] = useState<number>(ConfigConstants.MIN_LINK_RANGE);
    const [permittedNodes, setPermitted] = useState<Array<string>>([])
    const [waypoints, setWaypoints] = useState<Array<NodeLocation>>([]);

    // node-id input tag ref
    const idInputRef = useRef<HTMLInputElement>(null);

    // select tag ref of the current selected value
    const selectRef = useRef<HTMLSelectElement>(null);

    // waypoint values states
    const [currWaypoint, setWaypoint] = useState<NodeLocation>({ X: 0, Y: 0 });


    // --- click handlers ---

    // done click
    //
    const validateInput = () => {

        // if id wasn't defined or in use already, focus on the input
        if (!id || allNodes.find(node => node.NodeId === id) !== undefined) {
            setId(''); // clear input tag
            idInputRef.current?.focus();
            return;
        }

        // build the node configs
        const doneConfigs: NodeConfigs = buildConfigs(id, speed, location, linkRange, permittedNodes, waypoints);

        // build the IO stream message and convert to json
        const configsMsg: StdIOMessage = { Type: 'EngineConfigs', Payload: doneConfigs };
        const jsonConfigsMsg: string = JSON.stringify(configsMsg);

        // push to the launcher the new configs of this engine process
        pushToLauncher(jsonConfigsMsg);

        // to do: exit the configs window ...
    }

    // push to launcher using the pushToLauncher API 
    // from the preload script   
    const pushToLauncher = (input: string) => {
        try {
            window.electronAPI.pushNotification('launcher-stdin', { stdInput: input });
        }
        catch (error) {
            console.error(`error caught while trying to push a io msg to the launcher: \"${error}\"`);
        }
    }


    // cancel click
    //
    const exitConfig = () => {
        // to do: exit the configs window ...
    }

    // permitted handling
    //
    const addPermittedNode = () => {
        // if current val is not null 
        if (selectRef && !permittedNodes.includes(selectRef.current?.value!))
            setPermitted(prev => ([...prev, selectRef.current?.value!]))
    }
    const delPermittedNode = () => {
        // remove the last appended value
        setPermitted(prev => (prev.slice(0, -1)));
    }

    // waypoint handling
    //
    const addWaypoint = () => {
        // append current waypoint
        setWaypoints(prev => ([...prev, currWaypoint]));
    }
    const delWaypoint = () => {
        // remove the last appended value
        setWaypoints(prev => (prev.slice(0, -1)));
    }


    // --- on change handlers ---

    // speed update 
    //
    const updateSpeed = (val: number) => {
        if (!Number.isNaN(val)) {
            // bring input into bounds
            if (val > ConfigConstants.MAX_SPEED) {
                setSpeed(ConfigConstants.MAX_SPEED);
            }
            else if (val < ConfigConstants.MIN_SPEED) {
                setSpeed(ConfigConstants.MIN_SPEED);
            }
            else {
                setSpeed(val);
            }
        }
        // if input is empty, it defaults to the min speed
        else {
            setSpeed(ConfigConstants.MIN_SPEED)
        }
    }

    // location updates
    //
    const updateLocationX = (val: number) => {
        // if input is empty, it defaults to 0
        Number.isNaN(val)
            ? setLocation(prev => ({ ...prev, X: 0 }))
            : setLocation(prev => ({ ...prev, X: val }))
    }
    const updateLocationY = (val: number) => {
        // if input is empty, it defaults to 0
        Number.isNaN(val)
            ? setLocation(prev => ({ ...prev, Y: 0 }))
            : setLocation(prev => ({ ...prev, Y: val }))
    }

    // link range updates
    //
    const updateLinkRange = (val: number) => {
        if (!Number.isNaN(val)) {
            // bring input into bounds
            if (val > ConfigConstants.MAX_LINK_RANGE) {
                setLinkRange(ConfigConstants.MAX_LINK_RANGE);
            }
            else if (val < ConfigConstants.MIN_LINK_RANGE) {
                setLinkRange(ConfigConstants.MIN_LINK_RANGE);
            }
            else {
                setLinkRange(val)
            }
        }
        // if input is empty, it defaults to the max link range
        else {
            setLinkRange(ConfigConstants.MAX_LINK_RANGE)
        }
    }

    // current waypoint updates
    //
    const updateWaypointX = (val: number) => {
        // if input is empty, it defaults to 0
        Number.isNaN(val)
            ? setWaypoint(prev => ({ ...prev, X: 0 }))
            : setWaypoint(prev => ({ ...prev, X: val }))
    }
    const updateWaypointY = (val: number) => {
        // if input is empty, it defaults to 0
        Number.isNaN(val)
            ? setWaypoint(prev => ({ ...prev, Y: 0 }))
            : setWaypoint(prev => ({ ...prev, Y: val }))
    }


    // helper func to print preview waypoints
    //
    const printWaypoints = (): string => {

        let res = '';
        let len = waypoints.length;

        waypoints.forEach(waypoint => {

            // append waypoint
            res += `(${waypoint.X},${waypoint.Y})`;

            // while not the last element, append arrow to the next waypoint
            if (--len > 0) res += ' -> ';
        });
        return res;
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
                        <span className='item-title'>Link-Range:</span>
                        <span className='item-title'>Permitted:</span>
                        <span className='item-title'>Waypoints:</span>
                    </div>
                    <div className="inputs">

                        {/* node id input */}
                        <div className='input-field-container'>
                            <input ref={idInputRef} className='input' type='text' required onChange={(e) => setId(e.target.value)} />
                        </div>

                        {/* speed input */}
                        <div className='input-field-container'>
                            <div className='speed-input-container'>
                                <input className='input speed' type='number' min={ConfigConstants.MIN_SPEED} max={ConfigConstants.MAX_SPEED} step="1" onChange={e => updateSpeed(parseFloat(e.target.value))} />
                                <span className='speed-units'>m/s</span>
                            </div>
                        </div>

                        {/* location input */}
                        <div className='input-field-container'>
                            <div className='location-input-container'>
                                <span className='location-field'>X:</span>
                                <input className='input X' type='number' onChange={e => updateLocationX(parseFloat(e.target.value))} />

                                <span className='location-field'>Y:</span>
                                <input className='input Y' type='number' onChange={e => updateLocationY(parseFloat(e.target.value))} />
                            </div>
                        </div>

                        {/* link range */}
                        <div className='input-field-container'>
                            <div className='link-range-input-container'>
                                <input className='input link-range' type='number' min={ConfigConstants.MIN_LINK_RANGE} max={ConfigConstants.MAX_LINK_RANGE} step="1" onChange={e => updateLinkRange(parseInt(e.target.value))} />
                                <span className='link-range-units'>meters</span>
                            </div>
                        </div>

                        {/* permitted neighbors input */}
                        <div className='input-field-container'>
                            <div className='permitted-container'>
                                <select className='nodes-menu' ref={selectRef}>
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

                        {/* waypoints input */}
                        <div className='input-field-container'>
                            <div className='waypoints-container'>
                                <span className='location-field'>X:</span>
                                <input className='input X' type='number' onChange={e => updateWaypointX(parseFloat(e.target.value))} />

                                <span className='location-field'>Y:</span>
                                <input className='input Y' type='number' onChange={e => updateWaypointY(parseFloat(e.target.value))} />

                                <button className='add-btn' onClick={addWaypoint}>add</button>
                                <button className='del-btn' onClick={delWaypoint}>del</button>
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
                            <span className='preview-item-title'>Link-Range:</span>
                            <span className='preview-item-title'>Permitted:</span>
                            <span className='preview-item-title'>Waypoints:</span>
                        </div>

                        <div className="preview-values">
                            <span className='preview-item-value'>{id}</span>
                            <span className='preview-item-value'>{speed}</span>
                            <span className='preview-item-value'>({location.X},{location.Y})</span>
                            <span className='preview-item-value'>{linkRange}</span>
                            <span className='preview-item-value'>{(permittedNodes.length == 0) ? 'all' : permittedNodes.toString()}</span>
                            <span className='preview-item-value'>{(waypoints.length == 0) ? 'none' : printWaypoints()}</span>
                        </div>
                    </div>
                </div>

                <div className="configs-footer-container" >
                    <button className="footer-btn" style={{ marginLeft: "auto" }} onClick={exitConfig}>Cancel</button>
                    <button className="footer-btn" onClick={validateInput}>Done</button>
                </div>

            </div>
        </div >
    );
}
