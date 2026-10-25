namespace TacMesh.Core.utils.ipc.types
{
    // defines the change link range command ipc message structure
    //
    public class ChgLinkRangeCommand
    {
        public string EngineId { get; set; }
        public double LinkRange { get; set; }
    }
}
