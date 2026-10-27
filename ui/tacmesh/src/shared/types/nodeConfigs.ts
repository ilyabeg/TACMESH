import { NodeLocation } from "./engineLocation"

// defines the node's configuration settings
//
export type NodeConfigs = {
    NodeId: string,
    Speed: number,
    Location: NodeLocation,
    LinkRange: number,
    PermittedNeighbors: Array<string>,
    Waypoints: Array<NodeLocation>
}

// define the config constants per proposal
//
export enum ConfigConstants {
    // speeds (m/s)
    MIN_SPEED = 1,
    MAX_SPEED = 6,   
    
    // link range (meters)
    MIN_LINK_RANGE = 150,
    MAX_LINK_RANGE = 400,
}

// return a clean node configs object
//
export function buildConfigs(
    nodeId: string,
    speed: number,
    location: NodeLocation,
    linkRange: number,
    permittedNeighbors: Array<string>,
    waypoints: Array<NodeLocation>
): NodeConfigs {
    return {
        NodeId: nodeId,
        Speed: speed,
        Location: location,
        LinkRange: linkRange,
        PermittedNeighbors: permittedNeighbors,
        Waypoints: waypoints
    }
}