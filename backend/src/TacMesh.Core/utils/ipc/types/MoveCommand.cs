using TacMesh.Core.graph_related;

namespace TacMesh.Core.utils.ipc.types
{
    // defines the move command ipc message structure
    //
    public class MoveCommand
    {
        public string EngineId { get; set; }
        public Location Location { get; set; }

        // constructor
        public MoveCommand(string engineId, Location location)
        {
            EngineId = engineId;
            Location = location;
        }
    }
}
