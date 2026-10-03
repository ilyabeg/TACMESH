using System.Collections.Concurrent;
using System.Diagnostics;
using TacMesh.Launcher.info;

namespace TacMesh.Launcher.startup
{
    public class TerminalManager
    {
        // keep record for every Node process by their id
        private ConcurrentDictionary<string, ProcessRecord> _nodeRecords = new();

        // keep track of all agent processes
        private Dictionary<string, Process> _agents;


        // constructor
        public TerminalManager(Dictionary<string, Process> agents)
        {
            _agents = agents;
        }

        // attach data received event to each process before starting it
        public void StartAgents()
        {
            foreach (string agentId in _agents.Keys)
            {
                Process agent_process = _agents[agentId];

                // attach echo data received event handler and pass nodeId using the lambda,
                // this saves the nodeId for every Agent's own event handler with this specific id
                agent_process.OutputDataReceived += (s, e) => EchoOutput(agentId, s, e);

                agent_process.Start();
                agent_process.BeginOutputReadLine(); // lets see output
            }
        }

        // each EchoOutput gets spesific nodeId by the node attatching this handler with their id on creation
        private void EchoOutput(string nodeId, object sender, DataReceivedEventArgs e)
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

        public void AddRecord(string nodeId, string pId, int port)
        {
            // add record for every node by their id
            _nodeRecords.TryAdd(nodeId, new ProcessRecord(nodeId, pId, port));
        }

        public void Shutdown()
        {
            foreach (Process agent in _agents.Values)
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
                    // Laucnher's console, the OS automatically sends all processes that are linked to the launcher
                    // an interrupt exit signal, and then the Launcher runs this Shutdown() method. this causes a race
                    // condition: when the launcher checks !agent.HasExited on an agent instance, he may be alive at
                    // that microsecond, but at that exact moment he suddenly got the exit signal from the OS, then
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
