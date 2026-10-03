using System.Collections.Concurrent;
using System.Diagnostics;

namespace TacMesh.Launcher 
{
    public class Program
    {
        // class to easily hold each node's record
        private class ProcessRecord
        {
            // record fields
            public string NodeId { get; private set; }
            public string ProcessId { get; private set; }
            public int Port { get; private set; }
            public ProcessRecord(string nodeId, string pId, int port)
            {
                NodeId = nodeId;
                ProcessId = pId;
                Port = port;
            }            
        }

        // Ctrl+c event handler
        public static event ConsoleCancelEventHandler? CancelKeyPress;

        // keep record for every Node process by their id
        private static ConcurrentDictionary<string, ProcessRecord> nodeProcesses = new();

        static void Main(string[] args)
        {
            // color output for better visibility
            Console.ForegroundColor = ConsoleColor.Green;

            // reject execution without arguments
            if (args.Length < 1)
                throw new ArgumentException("[LAUNCHER] Error: Expected to get Number of Nodes as an Argument.");

            Console.WriteLine("[LAUNCHER] Launching Agent Processes...");

            int numOfAgents = int.Parse(args[0]);
            List<Process> agents = CreateAgents(numOfAgents);
            StartAgents(agents);

            // kill processes when Ctrl+C is pressed
            Console.CancelKeyPress += (s, e) => myHandler(s, e, agents);

            Console.WriteLine("Press ENTER to end.");
            Console.ReadLine();

            Shutdown(agents);
        }

        static void myHandler(object sender, ConsoleCancelEventArgs args, List<Process> agents)
        {
            args.Cancel = true;
            Shutdown(agents);
        }

        static List<Process> CreateAgents(int num_of_agents)
        {
            // paths relative to folder: Final-Project-2027/project-files/TACMESH/backend/src/TacMesh.Launcher/
            string agaent_exe = @"..\TacMesh.Agent\bin\Debug\net10.0\TacMesh.Agent.exe";
            string config_file = @"..\TacMesh.Agent\config.json";

            string configPath = Path.GetFullPath(config_file);
            List<Process> agents = new();

            for (int i = 0; i < num_of_agents; i++)
            {
                string nodeId = $"Node-0{i + 1}";

                // init start info
                ProcessStartInfo start_info = InitStartInfo(agaent_exe, nodeId, configPath);

                // init agent process with the start info
                Process agent = InitAgent(start_info);

                // attach echo data received event handler and pass nodeId using the lambda,
                // this saves the nodeId for every Agent's own event handler with this specific id
                agent.OutputDataReceived += (s,e) => EchoOutput(nodeId, s, e);  
                agents.Add(agent);
            }
            return agents;
        }

        static ProcessStartInfo InitStartInfo(string filename, string nodeId, string path)
        {
            return new ProcessStartInfo(filename, [nodeId, path]) {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = false
            };
        }

        static Process InitAgent(ProcessStartInfo start_info) => new Process() { StartInfo = start_info };

        static void StartAgents(List<Process> agents)
        {
            foreach (Process agent in agents)
            {
                agent.Start();
                agent.BeginOutputReadLine(); // lets see output
            }
        }

        // each EchoOutput gets spesific nodeId by the node attatching this handler with their id on creation
        static void EchoOutput(string nodeId, object sender, DataReceivedEventArgs e)
        {
            string output = e.Data;
            if (string.IsNullOrEmpty(output)) return;

            // print each node's output
            Console.WriteLine($"[{nodeId}] " + output);

            // save process in record dict
            if (output.StartsWith("PORT="))
            {
                int port = int.Parse(output.Split('=')[1]);
                string processId = ((Process)sender).Id.ToString();
                AddRecord(nodeId, processId, port);
            }
        }

        static void AddRecord(string nodeId, string pId, int port)
        {
            // add record for every node by their id
            nodeProcesses.TryAdd(nodeId, new ProcessRecord(nodeId, pId, port));
        }

        static void Shutdown(List<Process> agents)
        {
            foreach (Process agent in agents)
            {
                try
                {
                    if (!agent.HasExited)
                    {
                        agent.Kill(true);
                        agent.WaitForExit();
                    }
                }
                catch (InvalidOperationException)
                {
                    // InvalidOperationException is thrown if the Launcher tries to kill any process that is already
                    // terminated. i've added this ignoring catch block because when pressing CTRL+C in the 
                    // Lucnher's console, the OS automatically sends all processes that are linked to the launcher
                    // an interupt exit code, and then the Launcher runs this Shutdown() method. this causes a race
                    // condition: when the launcher checks !agent.HasExited on an agent instance, he may be alive at
                    // that microsecond, but at that exact moment he suddenly got the exit signal from ths OS, then
                    // the launcher tries agent.Kill(true), and a InvalidOperationException is thrown.
                    // the reason i'm ignoring this exception is because either way i get the outcome that i want:
                    // the agent process is terminated. it doesn't matter if the launcher won the race against the OS
                    // and killed the instance or vice versa.
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[LAUNCHER] Failed to kill agent: {e}");
                }
                finally
                {
                    agent.Dispose();
                }
            }
        }
    }
}
