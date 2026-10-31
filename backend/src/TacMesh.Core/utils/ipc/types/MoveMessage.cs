using TacMesh.Core.graph_related;

namespace TacMesh.Core.utils.ipc.types
{
    // defines the move command ipc message structure
    //
    public class MoveMessage
    {
        public string EngineId { get; set; }
        public Location Location { get; set; }

        // constructor
        public MoveMessage(string engineId, Location location)
        {
            EngineId = engineId;
            Location = location;
        }
    }
}
