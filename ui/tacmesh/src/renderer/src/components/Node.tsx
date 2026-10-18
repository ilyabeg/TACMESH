import '../assets/styles/component-styles/nodeStyle.css';

// define the node props
interface NodeProps {
    nodeId: string,
    color?: string,
    size?: number,
}

export default function Node({
    nodeId,
    color = "#D9D9D9", // default color
    size = 75, // default size in pixels,
}: NodeProps) {

    // return a circle svg (source: www.figma.com)
    return (
        <>
            <svg className="svg-style" width={size} height={size} viewBox="0 0 31 32" fill="none" xmlns="http://www.w3.org/2000/svg">
                <g filter="url(#filter0_d_1_4)" className='draggable'>
                    <circle cx="15.5" cy="12.5" r="12.5" fill={color} />
                    <circle cx="15.5" cy="12.5" r="12.25" stroke="black" stroke-opacity="0.7" stroke-width="0.5" />
                </g>
                <defs>
                    <filter id="filter0_d_1_4" x="0" y="0" width="31" height="32" filterUnits="userSpaceOnUse" color-interpolation-filters="sRGB">
                        <feFlood flood-opacity="0" result="BackgroundImageFix" />
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
                    <div className="node-text">{nodeId}</div>
                </foreignObject>
            </svg>
        </>
    );
}