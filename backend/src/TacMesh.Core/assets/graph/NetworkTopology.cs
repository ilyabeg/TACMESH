namespace TacMesh.Core.assets.graph
{
    /// <summary>
    /// The local network topology each node builds in his memory from the 
    /// colection of link state packets received from the net.
    /// </summary>
    public class NetworkTopology
    {
        // event to invoke when the local topology state changes
        //
        public event EventHandler? OnTopologyChanged;
    }
}
