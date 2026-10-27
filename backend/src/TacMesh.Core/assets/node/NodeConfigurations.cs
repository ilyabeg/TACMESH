using TacMesh.Core.graph_related;

namespace TacMesh.Core.assets.node
{
    /// <summary>
    /// global public configurations class that defines the node's settings.
    /// provides an easy way to update at runtime.
    /// </summary>
    public class NodeConfigurations
    {
        // --- node settings ---
        public string NodeId { get; private set; }
        public double Speed { get; private set; }
        public Location Location { get; private set; }
        public double LinkRange { get; private set; }
        public List<string> PermittedNeighbors { get; private set; }
        public Queue<Location> Waypoints { get; private set; }

        // constructors
        public NodeConfigurations() { } // empty constructor for json serialization/deserialization

        public NodeConfigurations(string nodeId, double speed, Location location, double linkRange, List<string> permitted, Queue<Location> waypoints)
        {
            NodeId = nodeId;
            Speed = speed;
            Location = location;
            LinkRange = linkRange;
            PermittedNeighbors = permitted;
            Waypoints = waypoints;
        }
    }
}
