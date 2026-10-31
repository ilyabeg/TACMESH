import '../../../assets/styles/component-styles/canvasStyle.css';
import Node from '../../Node';
import { EngineProcess } from '../../../../../shared/types/engineProcess';
import { useEffect, useRef, useState } from 'react';


// define the canvas props
type CanvasProps = {
    items: Array<EngineProcess>
}

// define the offset type
type offsetType = {
    x_offset: number,
    y_offset: number
}

// --- scale constants ---

// scale formula -> scale = pixels / meters
const MAX_SCALE = 10; // 1 meter = 10 pixels cap
const MIN_SCALE = 0.5;// 2 meters = 1 pixel 
const ZOOM_IN_MULTIPLIER = 1.1; // grow 10% on zooming in
const ZOOM_OUT_MULTIPLIER = 0.9; // shrink 10% on zooming in


export default function Canvas({ items }: CanvasProps) {

    // attach a canvas ref to observe it's resize dynamically
    //
    const canvasRef = useRef<HTMLDivElement>(null);

    // offset to center the (0,0) point to the center of the screen
    //
    const [offset, setOffset] = useState<offsetType>({ x_offset: 0, y_offset: 0 });

    // scale state to set the scale multiplier for each node. 
    // changes upon zooming in and out
    //
    const [scale, setScale] = useState<number>(1);

    // handle mouse wheel zoom in/out
    //
    const wheelSpinHandler = (event: React.WheelEvent<HTMLDivElement>) => {

        // negative delta y = zoom in = grow scale
        if (event.deltaY < 0) {
            setScale(prev => Math.min(prev * ZOOM_IN_MULTIPLIER, MAX_SCALE)); // take min between max scale or the current scale
        }

        // positive delta y = zoom out = shrink scale
        else if (event.deltaY > 0) {
            setScale(prev => Math.max(prev * ZOOM_OUT_MULTIPLIER, MIN_SCALE)); // take max between min scale and the current scale
        }
    }

    // dynamically calculate the center of the Canvas
    //
    useEffect(() => {
        if (!canvasRef.current) return;

        // observer that observer html elements and fires a callback every time 
        // their pixel dimentions change. entries is an array that contains the 
        // data of every element this observer is watching
        //
        const observer = new ResizeObserver(entries => {

            // pull the width and height of the canvas container at index 0 of the
            // entires array (since he is the only html element that the array observes)
            //
            const { width, height } = entries[0].contentRect;

            // calculate the center of the screen for every kind of size of the canvas 
            // container which is always width/2 and height/2
            // 
            setOffset({ x_offset: width / 2, y_offset: height / 2 });
        });

        // make the observer watch the canvas container
        observer.observe(canvasRef.current);

        // cleanup on renders
        return () => observer.disconnect();
    }, []);

    return (
        <>
            <div className="canvas-container" ref={canvasRef} onWheel={wheelSpinHandler} >
                {items.map(item =>
                    <Node node={item} scale={scale} x_offset={offset.x_offset} y_offset={offset.y_offset} />
                )}
            </div>
        </>
    );
}