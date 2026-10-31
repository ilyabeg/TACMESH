using TacMesh.Core.graph_related;

namespace TacMesh.Core.utils.ipc.types
{
    // defines the move command ipc message structure
    //
    public class MoveMessage
    {
        public string NodeId { get; set; }
        public Location Location { get; set; }

        // constructor
        public MoveMessage(string nodeId, Location location)
        {
            NodeId = nodeId;
            Location = location;
        }
    }
}
