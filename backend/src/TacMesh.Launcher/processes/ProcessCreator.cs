using System.Diagnostics;

namespace TacMesh.Launcher.processes
{
    /// <summary>
    /// Agent creator class
    /// </summary>
    public class ProcessCreator
    {
        // paths relative to folder: Final-Project-2027/project-files/TACMESH/backend/src/TacMesh.Launcher/
        private const string _agaent_exe = @"..\TacMesh.Agent\bin\Debug\net10.0\TacMesh.Agent.exe";
        private const string _config_file = @"..\TacMesh.Agent\config.json";        


        // generate N agents
        public static Dictionary<string, Process> CreateAgents(int num_of_agents)
        {
            Dictionary<string, Process> agents = new();
            string configPath = Path.GetFullPath(_config_file);

            for (int i = 0; i < num_of_agents; i++)
            {
                string nodeId = $"Node-0{i + 1}";

                // init start info
                ProcessStartInfo start_info = InitStartInfo(_agaent_exe, nodeId, configPath);

                // init agent process with the start info
                Process agent = InitAgent(start_info);
                
                agents.Add(nodeId, agent);
            }
            return agents;
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
