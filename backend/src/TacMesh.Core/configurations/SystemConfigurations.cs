using System.Net;
using System.Text.Json;

namespace TacMesh.Core.configurations
{
    public class SystemConfigurations
    {
        // IMPORTANT NOTE: I USE THE CONFIGURATIONS JSON FILE TO TEMPORARILY HOLD THE 
        // STATIC NODES ONLY FOR THE TEMPORARY TEST, I AM AWARE THAT IS WRONG AND DO 
        // NOT INTEND ON LEAVING IT LIKE THIS, IT IS ONLY FOR THIS SPESIFIC MISSION TEST.
        public static Dictionary<string, IPEndPoint> StaticNodes { get; private set; } = new Dictionary<string, IPEndPoint>();

        // loads provided configs to memory
        public static void LoadConfigurations(string configFilePath, string sourceNodeId)
        {
            // read the json and deserialize
            string configs = File.ReadAllText(configFilePath);
            Dictionary<string, int> nodes = JsonSerializer.Deserialize<Dictionary<string, int>>(configs);

            if (nodes == null) throw new ArgumentNullException("Configurations file can't be empty.");

            foreach (string nodeId in nodes.Keys)
            {
                IPEndPoint endpoint = new IPEndPoint(IPAddress.Loopback, nodes[nodeId]);
                StaticNodes.TryAdd(nodeId, endpoint);
            }
        }
    }
}
