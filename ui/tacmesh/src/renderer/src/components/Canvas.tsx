import '../assets/styles/component-styles/canvasStyle.css';
import Node from './Node';

export default function Canvas() {
    return (
        <>
            <div className="canvas-container">
                <Node nodeId='Node-01' /> {/* temporary test */}
            </div>
        </>
    );
}