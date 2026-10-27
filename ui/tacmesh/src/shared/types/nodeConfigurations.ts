import { NodeLocation } from "./engineLocation"

// defines the node's configuration settings
//
export type NodeConfigurations = {
    NodeId: string,
    Speed: number,
    Location: NodeLocation,
    PermittedNeighbors: Array<string>,
    Waypoints: Array<NodeLocation>
}
