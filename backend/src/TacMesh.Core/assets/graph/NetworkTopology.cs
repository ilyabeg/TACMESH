using System.Collections.Concurrent;
using System.Text;
using TacMesh.Core.utils.design;

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

            //PrintTopology(); // print out topology when updated
        }

        /// <summary>
        /// Updates the specific node's location in the topology state if the node exists
        /// </summary>
        /// <param name="nodeId">The node identifier</param>
        /// <param name="newLocation">The node's location</param>
        public void UpdateNodeLocation(string nodeId, Location newLocation)
        {
            // ignore state updates with the same exact location
            //
            if (State.TryGetValue(nodeId, out NodeState? currState) && currState.Location.Equals(newLocation))
                return;

            // if neighbor does not exist, create new record of his topology.
            // else, set new state but keep previous link state in tact
            //
            NodeState newNodeState = new NodeState(new List<string>(), newLocation);
            State.AddOrUpdate(nodeId, newNodeState, (key, prevState) => new NodeState(prevState.LinkState, newLocation));

            //PrintTopology(); // print out topology when updated
        }

        /// <summary>
        /// private printer method to test out the network topology structure
        /// </summary>
        private void PrintTopology()
        {
            StringBuilder str = new StringBuilder();

            str.AppendLine("┌────────────────────────────────────────────────────────────┐");

            string title = $" NETWORK TOPOLOGY (Count: {State.Count})";
            str.AppendLine($"│{title,ConsoleDesign._m60_padRight}│");

            str.AppendLine("├────────────┬────────────┬──────────────────────────────────┤");
            str.AppendLine("│ Node ID    │ Location   │ Link State (Neighbors)           │");
            str.AppendLine("├────────────┼────────────┼──────────────────────────────────┤");

            foreach (KeyValuePair<string, NodeState> kvp in State)
            {
                string nodeId = kvp.Key;
                Location? nodeLocation = kvp.Value.Location;
                List<string>? nodeLinkState = kvp.Value.LinkState;

                // convert location string
                string location = (nodeLocation != null) ? nodeLocation.ToString() : "None";

                // convert links to single line with ',' between node id's
                string links = nodeLinkState != null && nodeLinkState.Count > 0
                    ? string.Join(", ", nodeLinkState)
                    : "None";

                string padded_string = FormatNodeState(nodeId, location, links);
                str.AppendLine(padded_string);
            }

            str.AppendLine("└────────────┴────────────┴──────────────────────────────────┘");
            Console.WriteLine(str);
        }

        // helper method to pad the node state record
        private string FormatNodeState(string id, string location, string links)
        {
            // TEMPORARILY USE MAGIC NUMBERS FOR TESTING !!!
            //
            if (id.Length > 10) id = id.Substring(0, 7) + "...";
            if (location.Length > 13) location = location.Substring(0, 10) + "...";
            if (links.Length > 29) links = links.Substring(0, 26) + "...";
            
            string padded_id = $"{id,ConsoleDesign._10_padRight}";
            string padded_location = $"{location,ConsoleDesign._10_padRight}";
            string padded_links = $"{links,ConsoleDesign._32_padRight}";

            return $"│ {padded_id} │ {padded_location} │ {padded_links} │";
        }
    }
}
