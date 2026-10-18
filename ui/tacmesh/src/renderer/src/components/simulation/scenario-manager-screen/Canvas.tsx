import Draggable from 'react-draggable';
import '../../../assets/styles/component-styles/canvasStyle.css';
import Node from '../../Node';
import { useRef } from 'react';

export default function Canvas() {

    // React Draggable employs the deprecated findDOMNode method, meaning the drag
    // won't work as intended.
    // This issue can be resolved by attaching a ref directly to the targeted element
    //
    const nodeRef = useRef(null);

    return (
        <>
            <div className="canvas-container">

                {/* make the node components draggable in the parent container (the canvas) */}
                <Draggable nodeRef={nodeRef} bounds="parent">

                    {/* attach the same ref to connect between the draggable tag and the node itself.
                        the react-draggable needs a reference to know who to drag */}
                    <div ref={nodeRef} style={{ position: "absolute" }}>
                        <Node nodeId='Node-01' /> {/* temporary test */}
                    </div>

                </Draggable>

            </div>
        </>
    );
}