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
        // get the full path of the agent exe
        private static readonly string _baseDir = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string _agentPath = Path.GetFullPath(Path.Combine(_baseDir, @"..\..\..\..\TacMesh.Agent\bin\Debug\net10.0\TacMesh.Agent.exe"));


        // TEMPORARY STATIC LOCATION DICT FOR EVERY NODE UNTIL THE USER INPUTS IN THE FUTURE 
        private static readonly Dictionary<string, Location> _locations = new()
        {
            ["Node-01"] = new Location(1, 1),
            ["Node-02"] = new Location(1, 2),
            ["Node-03"] = new Location(2, 2),
            ["Node-04"] = new Location(2, 1),
            ["Node-05"] = new Location(1.5, 1.5)
        };

        // TEMPORARY STATIC SPEEDS DICT FOR EVERY NODE UNTIL THE USER INPUTS IN THE FUTURE 
        private static readonly Dictionary<string, double> _speeds = new()
        {
            ["Node-01"] = 2,
            ["Node-02"] = 1,
            ["Node-03"] = 1.5,
            ["Node-04"] = 1,
            ["Node-05"] = 2
        };

        // TEMPORARY STATIC PERMITTED NODES LIST FOR EVERY NODE UNTIL THE USER INPUTS IN THE FUTURE 
        private static readonly List<string> _permittedNodes = new()
        {
            "Node-01", "Node-02", "Node-03", "Node-04", "Node-05"
        };


        // generate N agents
        public static Dictionary<string, Process> CreateAgents(int num_of_agents)
        {
            Dictionary<string, Process> agents = new();

            for (int i = 0; i < num_of_agents; i++)
            {
                // init agent process with the start info
                string nodeId = $"Node-{i+1:D2}";
                Process agent = CreateNewAgentInstance(nodeId);
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
            double speed = _speeds[nodeId];
            string permittedNodes = JsonSerializer.Serialize(_permittedNodes);

            ProcessStartInfo start_info = InitStartInfo(_agentPath, nodeId, jsonLocation, speed, permittedNodes);

            // init agent process with the start info
            return new Process() { StartInfo = start_info };
        }

        private static ProcessStartInfo InitStartInfo(string filename, string nodeId, string location, double speed, string permittedNodes)
        {
            return new ProcessStartInfo(filename, [nodeId, location, speed.ToString(), permittedNodes])
            {
                UseShellExecute = false,
                RedirectStandardOutput = true, // redirect output stream to the launcher process
                RedirectStandardInput = true, // redirect input stream to the launcher process
                CreateNoWindow = false
            };
        }
    }
}
