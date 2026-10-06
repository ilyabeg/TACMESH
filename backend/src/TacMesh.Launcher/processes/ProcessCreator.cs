using System.Diagnostics;

namespace TacMesh.Launcher.processes
{
    /// <summary>
    /// Agent creator class
    /// </summary>
    public class ProcessCreator
    {
        // paths relative to folder: Final-Project-2027/project-files/TACMESH/backend/src/TacMesh.Launcher/
        private const string _agaentExe = @"..\TacMesh.Agent\bin\Debug\net10.0\TacMesh.Agent.exe";
        private const string _scenario_file = @"..\TacMesh.Agent\scenario.json";        
        private static string _scenarioPath = Path.GetFullPath(_scenario_file);

        // generate N agents
        public static Dictionary<string, Process> CreateAgents(int num_of_agents)
        {
            Dictionary<string, Process> agents = new();

            for (int i = 0; i < num_of_agents; i++)
            {
                string nodeId = $"Node-{i+1:D2}";

                // init start info
                ProcessStartInfo start_info = InitStartInfo(_agaentExe, nodeId, _scenarioPath);

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
            ProcessStartInfo start_info = InitStartInfo(_agaentExe, nodeId, _scenarioPath);

            // init agent process with the start info
            return InitAgent(start_info);
        }

        private static ProcessStartInfo InitStartInfo(string filename, string nodeId, string path)
        {
            return new ProcessStartInfo(filename, [nodeId, path])
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = false
            };
        }

        private static Process InitAgent(ProcessStartInfo start_info) => new Process() { StartInfo = start_info };
    }
}
