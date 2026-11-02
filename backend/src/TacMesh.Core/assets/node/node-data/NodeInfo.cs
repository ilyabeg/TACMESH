using System.Text.Json.Serialization;

namespace TacMesh.Core.assets.node
{
    // node rolde per the proposal
    //
    public enum NodeRole
    {
        Soldier,
        Medic,
        Commander
    }

    /// <summary>
    /// define a class to hold the engine process node's imutable information, such as 
    /// the node id or the role, which are defines before starting the scenario mission.
    /// </summary>
    public class NodeInfo
    {
        [JsonInclude]
        public string NodeId { get; private set; }
        [JsonInclude]
        public NodeRole Role { get; private set; }

        // empty constructor for json deserialization
        public NodeInfo() { }
        public NodeInfo(string nodeId, NodeRole role)
        {
            NodeId = nodeId;
            Role = role;
        }
    }
}
