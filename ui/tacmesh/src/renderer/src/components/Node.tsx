import Draggable, { DraggableData, DraggableEvent } from 'react-draggable';
import '../assets/styles/component-styles/nodeStyle.css';
import { useEffect, useRef, useState } from 'react';
import { EngineProcess } from '../../../shared/types/engineProcess';
import { StdIOMessage } from '../../../shared/types/stdIOMessage';
import { NodeLocation } from '../../../shared/types/engineLocation';
import { DEFAULT_LOCATION } from '../../../shared/types/engineLocation';


// define the node props
interface NodeProps {
    node: EngineProcess,
    scale: number, // scale translating the backend location units to pixel units
    x_offset: number, // offset x axis
    y_offset: number, // offset y axis
    color?: string,
    size?: number,
}

// node position type on the ui screen. units = pixels
type NodeUiPos = {
    x: number,
    y: number
}

export default function Node({
    node,
    scale,
    x_offset,
    y_offset,
    color = "#D9D9D9", // default color
}: NodeProps) {

    // --- position translations ---

    // translate the ui screen coordinates to the backend compatable grid coordinates
    //
    const calcBackendCoords = (uiLocation: NodeUiPos): NodeLocation => {
        return {
            X: (uiLocation.x - x_offset) / scale,
            Y: -(uiLocation.y - y_offset) / scale
        };
    }

    // translate backend coordinates to ui screen pixel coordinates
    //
    const calcUiCoords = (backendLocation: NodeLocation | undefined): NodeUiPos => {

        // if the location isn't defined set it as the default (0,0)
        if (backendLocation === undefined) backendLocation = DEFAULT_LOCATION;

        return {
            x: (backendLocation.X * scale) + x_offset,
            y: (-backendLocation.Y * scale) + y_offset
        }
    }


    // --- variables ---

    const NODE_DIAMETER = 50; // in meters
    const NODE_SIZE = NODE_DIAMETER * scale; // grows/shrinks depending on the scale

    // React Draggable library employs the deprecated findDOMNode method, meaning the drag
    // won't work as intended.
    // This issue can be resolved by attaching a ref directly to the targeted element
    //
    const nodeRef = useRef(null);

    // save the ui pos of the node on screen to make draggable, with
    // the current node position as default
    //
    const [uiPos, setUiPos] = useState<NodeUiPos>(calcUiCoords(node.configs?.Location));

    // update ui pos if the node location changed or the scale and offsets
    //
    useEffect(() => {
        setUiPos(calcUiCoords(node.configs?.Location));
    }, [node.configs?.Location, scale, x_offset, y_offset]);


    // --- drag & drops handlers ---

    // handle drag event
    //
    const handleItemDrag = (_event: DraggableEvent, data: DraggableData) => {

        // pull the position from the event and update the state pos 
        // to move the draggable node on screen
        const ui_position: NodeUiPos = { x: data.x, y: data.y };
        setUiPos(ui_position);
    }

    // handle drop event
    //
    const handleItemDrop = (_event: DraggableEvent, data: DraggableData) => {

        // calculate the new location after drop
        const ui_position: NodeUiPos = { x: data.x, y: data.y };
        const newLocation: NodeLocation = calcBackendCoords(ui_position);

        // create the json input message and push to the launcher
        const moveMsgJson: string = getJsonMsg(newLocation);
        pushToLauncher(moveMsgJson);
    }


    // --- io message handlers ---

    const getJsonMsg = (location: NodeLocation): string => {
        const moveMsg: StdIOMessage = {
            Type: 'MoveEngine',
            Payload: { NodeId: node.NodeId, Location: location }
        }
        return JSON.stringify(moveMsg);
    }

    // push move node io msg to launcher 
    //
    const pushToLauncher = (input: string) => {
        try {
            window.electronAPI.pushNotification('launcher-stdin', { stdInput: input });
        }
        catch (error) {
            console.error(`error caught while trying to push a io msg to the launcher: \"${error}\"`);
        }
    }

    // return a circle svg (source: www.figma.com)
    return (
        <>
            {/* make the node components draggable in the parent container (the canvas) */}
            <Draggable nodeRef={nodeRef} bounds="parent" onDrag={handleItemDrag} onStop={handleItemDrop} position={{ x: uiPos.x, y: uiPos.y }}>

                {/* attach the same ref to connect between the draggable tag and the node itself.
                        the react-draggable needs a reference to know who to drag */}
                <div style={{ width: NODE_SIZE, height: NODE_SIZE, transform: "translate(-50%, -50%)", position: "absolute" }} ref={nodeRef}>
                    <svg className="svg-style" width="100%" height="100%" viewBox="0 0 31 32" fill="none" xmlns="http://www.w3.org/2000/svg">
                        <g filter="url(#filter0_d_1_4)" className='draggable'>
                            <circle cx="15.5" cy="12.5" r="12.5" fill={color} />
                            <circle cx="15.5" cy="12.5" r="12.25" stroke="black" strokeOpacity="0.7" strokeWidth="0.5" />
                        </g>
                        <defs>
                            <filter id="filter0_d_1_4" x="0" y="0" width="31" height="32" filterUnits="userSpaceOnUse" colorInterpolationFilters="sRGB">
                                <feFlood floodOpacity="0" result="BackgroundImageFix" />
                                <feColorMatrix in="SourceAlpha" type="matrix" values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 127 0" result="hardAlpha" />
                                <feOffset dy="4" />
                                <feGaussianBlur stdDeviation="1.5" />
                                <feComposite in2="hardAlpha" operator="out" />
                                <feColorMatrix type="matrix" values="0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0.15 0" />
                                <feBlend mode="normal" in2="BackgroundImageFix" result="effect1_dropShadow_1_4" />
                                <feBlend mode="normal" in="SourceGraphic" in2="effect1_dropShadow_1_4" result="shape" />
                            </filter>
                        </defs>

                        {/* foreign object svg element allows including elements from a different XML namespace such as HTML */}
                        {/* aligned the "text div" to be exactly 2R of the circle */}
                        <foreignObject x="3" y="0" width="25" height="25">
                            <div className="node-text">{node.NodeId}</div>
                        </foreignObject>
                    </svg>
                </div>

            </Draggable>
        </>
    );
}