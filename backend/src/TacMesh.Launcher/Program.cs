using System.Diagnostics;

namespace TacMesh.Launcher 
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("[LAUNCHER] Launching Agent Processes...");

            List<Process> agents = CreateAgents(2);
            StartAgents(agents);

            Console.WriteLine("Press ENTER to end.");
            Console.ReadLine();

            Shutdown(agents);
        }

        static List<Process> CreateAgents(int num_of_agents)
        {
            string agaent_exe = @"..\..\..\..\TacMesh.Agent\bin\Debug\net10.0\TacMesh.Agent.exe";
            List<Process> agents = new();

            for (int i = 0; i < num_of_agents; i++)
            {
                string nodeID = $"Node-0{i + 1}";
                string tmp_path = "tmp string";

                // init start info
                ProcessStartInfo start_info = InitStartInfo(agaent_exe, nodeID, tmp_path);

                // init agent process with the start info
                Process agent = InitAgent(start_info);

                // attach echo data received event handler
                agent.OutputDataReceived += (s,e) => EchoOutput(s, e);
                agents.Add(agent);
            }
            return agents;
        }

        static ProcessStartInfo InitStartInfo(string filename, string nodeId, string path)
        {
            return new ProcessStartInfo(filename, [nodeId, path]) {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true // doesn't open agent window
            };
        }

        static Process InitAgent(ProcessStartInfo start_info)
        {
            return new Process() { StartInfo = start_info };
        }

        static void StartAgents(List<Process> agents)
        {
            foreach (Process agent in agents)
            {
                agent.Start();
                agent.BeginOutputReadLine(); // lets see output
            }
        }

        static void EchoOutput(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data)) return;

            Console.WriteLine($"[LAUNCHER] Received: '{e.Data}'");
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
                catch (Exception e)
                {
                    Console.WriteLine($"[LAUNCHER] Failed to kill agent: {e.Message}");
                }
                finally
                {
                    agent.Dispose();
                }
            }
        }
    }
}
