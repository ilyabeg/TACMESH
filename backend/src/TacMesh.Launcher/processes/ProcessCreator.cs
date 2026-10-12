using System.Diagnostics;
using System.Text.Json;
using TacMesh.Core.graph_related;

namespace TacMesh.Launcher.processes
{
    /// <summary>
    /// Agent creator class
    /// </summary>
    public class ProcessCreator
    {
        // paths relative to folder: Final-Project-2027/project-files/TACMESH/backend/src/TacMesh.Launcher/
        private const string _agaentExe = @"..\TacMesh.Agent\bin\Debug\net10.0\TacMesh.Agent.exe";

        // TEMPORARY STATIC LOCATION DICT FOR EVERY NODE
        private static Dictionary<string, Location> _locations = new()
        {
            ["Node-01"] = new Location(1, 1),
            ["Node-02"] = new Location(1, 2),
            ["Node-03"] = new Location(2, 2),
            ["Node-04"] = new Location(2, 1),
            ["Node-05"] = new Location(1.5, 1.5)
        };


        // generate N agents
        public static Dictionary<string, Process> CreateAgents(int num_of_agents)
        {
            Dictionary<string, Process> agents = new();

            for (int i = 0; i < num_of_agents; i++)
            {
                string nodeId = $"Node-{i+1:D2}";
                string jsonLocation = JsonSerializer.Serialize(_locations[nodeId]);

                // init start info
                ProcessStartInfo start_info = InitStartInfo(_agaentExe, nodeId, jsonLocation);

                // init agent process with the start info
                Process agent = InitAgent(start_info); 
                agents.Add(nodeId, agent);
            }
            return agents;
        }

        // single Process generator
        public static Process CreateNewAgentInstance(string nodeId)
        {
            // init start info
            // if node dies and restarted, the initial location is initiated with it again
            string jsonLocation = JsonSerializer.Serialize(_locations[nodeId]);
            ProcessStartInfo start_info = InitStartInfo(_agaentExe, nodeId, jsonLocation);

            // init agent process with the start info
            return InitAgent(start_info);
        }

        private static ProcessStartInfo InitStartInfo(string filename, string nodeId, string location)
        {
            return new ProcessStartInfo(filename, [nodeId, location])
            {
                UseShellExecute = false,
                RedirectStandardOutput = true, // redirect output stream to the launcher process
                RedirectStandardInput = true, // redirect input stream to the launcher process
                CreateNoWindow = false
            };
        }

        private static Process InitAgent(ProcessStartInfo start_info) => new Process() { StartInfo = start_info };
    }
}
