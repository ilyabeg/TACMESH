using System.Diagnostics;
using System.Text.Json;
using TacMesh.Core.assets.node;

namespace TacMesh.Launcher.processes
{
    /// <summary>
    /// Engine creator class
    /// </summary>
    public class ProcessCreator
    {
        // get the full path of the engine exe
        private static readonly string _baseDir = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string _enginePath = Path.GetFullPath(Path.Combine(_baseDir, @"..\..\..\..\TacMesh.Engine\bin\Debug\net10.0\TacMesh.Engine.exe"));


        // generate N engines
        public static Dictionary<string, Process> CreateEngines(int num_of_engines)
        {
            Dictionary<string, Process> engines = new();

            // create a default permitted nodes list containing all of the node id's
            List<string> permittedNodes = CreateDefaultPermitted(num_of_engines);

            for (int i = 0; i < num_of_engines; i++)
            {
                // init engine process with the start info
                string nodeId = $"Node-{i+1:D2}";
                Process engine = CreateNewEngineInstance(nodeId, permittedNodes);
                engines.Add(nodeId, engine);
            }
            return engines;
        }

        private static List<string> CreateDefaultPermitted(int num_of_engines)
        {
            List<string> allNodeIds = new List<string>();

            for (int i = 0; i < num_of_engines; i++)
                allNodeIds.Add($"Node-{i+1:D2}");

            return allNodeIds;
        }

        // single Process generator
        public static Process CreateNewEngineInstance(string nodeId, List<string> permittedNodes)
        {
            // init start info

            // temporarly create node info with same node role for everyone
            NodeInfo nodeInfo = new NodeInfo(nodeId, NodeRole.Soldier);
            string nodeInfoJson = JsonSerializer.Serialize(nodeInfo);

            // if node dies and restarted, the default configs are initiated with it again
            NodeConfigs def_configs = NodeConfigs.GetDefaultEngineConfigs(permittedNodes);
            string configsJson = JsonSerializer.Serialize(def_configs);

            // pass in arguments
            ProcessStartInfo start_info = InitStartInfo(_enginePath, [nodeInfoJson, configsJson]);

            // init engine process with the start info
            return new Process() { StartInfo = start_info };
        }        

        private static ProcessStartInfo InitStartInfo(string filename, string[] args)
        {
            return new ProcessStartInfo(filename, args)
            {
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(filename),
                RedirectStandardOutput = true, // redirect output stream to the launcher process
                RedirectStandardInput = true, // redirect input stream to the launcher process
                CreateNoWindow = false,
                StandardOutputEncoding = System.Text.Encoding.UTF8
            };
        }
    }
}
