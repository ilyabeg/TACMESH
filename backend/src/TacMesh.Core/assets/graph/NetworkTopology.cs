using System.Collections.Concurrent;

namespace TacMesh.Core.assets.graph
{
    /// <summary>
    /// Node's current state in the graph
    /// </summary>
    public class NodeState
    {
        public List<string> LinkState { get; private set; } // the node's adjacency list
        public Location Location { get; private set; } // node's location on the graph

        public NodeState(List<string> linkState, Location location)
        {
            LinkState = linkState;
            Location = location;
        }
    }

    /// <summary>
    /// The local network topology each node builds in his memory from the 
    /// colection of link state packets received from the net.
    /// </summary>
    public class NetworkTopology
    {
        // network topology is represented by dictionary of adjacency list
        // for every node and it's location in the graph
        // 
        public ConcurrentDictionary<string, NodeState> State { get; private set; }

        // constructor
        public NetworkTopology()
        {
            State = new ConcurrentDictionary<string, NodeState>();
        }

        /// <summary>
        /// Updates the specific node's link state in the topology state if the node exists
        /// </summary>
        /// <param name="nodeId">The node identifier</param>
        /// <param name="linkState">The node's link state</param>
        public void UpdateLinkState(string nodeId, List<string> linkState)
        {
            // if neighbor does not exist, create new record of his topology.
            // else, set new state but keep previous location in tact
            //
            NodeState newNodeState = new NodeState(linkState, Location.DEFAULT_LOCATION);
            State.AddOrUpdate(nodeId, newNodeState, (key, prevState) => new NodeState(linkState, prevState.Location));
        }

        /// <summary>
        /// Updates the specific node's location in the topology state if the node exists
        /// </summary>
        /// <param name="nodeId">The node identifier</param>
        /// <param name="nodeLocation">The node's location</param>
        public void UpdateNodeLocation(string nodeId, Location nodeLocation)
        {
            // if neighbor does not exist, create new record of his topology.
            // else, set new state but keep previous link state in tact
            //
            NodeState newNodeState = new NodeState(new List<string>(), nodeLocation);
            State.AddOrUpdate(nodeId, newNodeState, (key, prevState) => new NodeState(prevState.LinkState, nodeLocation));
        }
    }
}
