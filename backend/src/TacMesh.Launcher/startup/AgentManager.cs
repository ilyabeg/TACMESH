using System.Collections.Concurrent;
using System.Diagnostics;
using TacMesh.Launcher.info;
using TacMesh.Launcher.processes;

namespace TacMesh.Launcher.startup
{
    public class AgentManager
    {
        // keep record for every Node process by their id
        // concurrent because many threads may write to the dictionary when receiving 'PORT=...' on stdout
        private readonly ConcurrentDictionary<string, ProcessRecord> _nodeRecords = new();

        // keep track of all agent processes by the agent ID
        // regular dictionary because only the main thread alters this (command executions)
        private readonly Dictionary<string, Process> _agents;

        // Semaphore to limit the number of processes the AgentManager creates at a time
        private readonly Semaphore _semaphore;
        private const int _initialCount = 0; // the initial number of requests for the semaphore that can be granted
        private const int _maximumCount = 1; // the maximum number of requests for the semaphore that can be granted
        private const string _semName = "TacMesh-Global-Sem"; // the name that the semaphore uses

        // public event to invoke the waypoint manager upon a node reaching the destination waypoint
        public event EventHandler<string> WaypointReached;


        // constructor
        public AgentManager(Dictionary<string, Process> agents)
        {
            // create a named semaphore that can be shared accross processes
            _semaphore = new Semaphore(_initialCount, _maximumCount, _semName);
            _agents = agents;
        }


        // attach data received event to each process before starting it
        public void StartAgents()
        {
            // create a list of the sorted id's in the dict
            // O(nlogn) + O(n) foreach loop, and O(n) extra space for the list
            List<string> sortedIds = _agents.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Key).ToList();
            
            foreach (string agentId in sortedIds)
            {
                Process agent_process = _agents[agentId];

                // attach echo data received event handler and pass agentId using the lambda,
                // this saves the agentId for every Agent's own event handler with this specific id
                agent_process.OutputDataReceived += (s, e) => OnStdOutReceived(agentId, s, e);

                agent_process.Start();
                agent_process.BeginOutputReadLine(); // lets see output

                // freeze creation here until semaphore is released
                _semaphore.WaitOne();
            }

            // make the agents emit the beacon only after starting up all of the processes
            foreach (string agent in _agents.Keys)
                WriteToInputStream(agent, "EMIT_INITIATION_BEACON");
        }

        // each EchoOutput gets spesific agentId by the agent attatching this handler with their id on creation
        private void OnStdOutReceived(string agentId, object sender, DataReceivedEventArgs e)
        {            
            string output = e.Data;
            if (output == null) return;

            // print each agent's output in a FIXED format
            Printer.AddNewLine(agentId, output);

            // parse output if possible
            TryParseStdout(agentId, sender, output);
        }

        // tries to parse the stdoutput
        private void TryParseStdout(string agentId, object sender, string output)
        {
            // save process in record dict
            if (output.StartsWith("PORT="))
            {
                int port = int.Parse(output.Split('=')[1]);
                string processId = ((Process)sender).Id.ToString();
                AddRecord(agentId, processId, port);
            }

            // invoke the waypoint manager if advance signal received
            if (output.Equals("adv."))
                WaypointReached?.Invoke(this, agentId);
        }

        // writes input to the input stream of a specfic agent
        public void WriteToInputStream(string agentId, string input)
        {
            // find the specific process and write a line to it's input stream
            Process agentProcess = _agents[agentId];
            agentProcess.StandardInput.WriteLine(input);
        }

        // TEMPORARY initializer method to start the waypoint advances
        // this will be changed in the future to handle the pos advances differently
        public void InitAdvances()
        {
            // send all agents their first waypoint
            foreach (string agentId in _agents.Keys)
                WaypointReached?.Invoke(this, agentId);
        }


        // tries to add record by agent id
        private void AddRecord(string agentId, string pId, int port) => _nodeRecords.TryAdd(agentId, new ProcessRecord(agentId, pId, port));


        // tries to remove record by agent id
        private void RemoveRecord(string agentId) => _nodeRecords.TryRemove(agentId, out _);


        // kills a specific process of an agent by their ID
        public void KillAgent(string agentId)
        {
            try
            {
                Process agent = _agents[agentId];
                if (agent.HasExited) throw new ArgumentException();

                agent.Kill(true);
                agent.WaitForExit();
                RemoveRecord(agentId);
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine($"[LAUNCHER] Failed to kill agent {agentId} due to: Agent does not exist.");
            }
            catch (ArgumentException)
            {
                Console.WriteLine($"[LAUNCHER] Failed to kill agent {agentId} due to: Agent already terminated.");
            }
            catch (InvalidOperationException)
            {
                // ignoring InvalidOperationException for the same reason as in 'Shutdown()' method...
            }
            catch (Exception e)
            {
                Console.WriteLine($"[LAUNCHER] Failed to kill agent: {e.Message}");
            }
        }

        // start a specific process of an agent by their ID
        public void StartAgent(string agentId)
        {
            try
            {
                // give access to a new Process instance only if id existed
                Process agent = _agents[agentId];
                if (!agent.HasExited) throw new ArgumentException();

                // re-create Process instance to not use the same 'dead' one
                agent = ProcessCreator.CreateNewAgentInstance(agentId);

                // attach output handler, and update agents Dictionary
                agent.OutputDataReceived += (s,e) => OnStdOutReceived(agentId, s, e);
                _agents[agentId] = agent;

                // should automatically add record back with new process id
                agent.Start();
                agent.BeginOutputReadLine();
            }
            catch (KeyNotFoundException)
            {
                Console.WriteLine($"[LAUNCHER] Failed to start agent {agentId} due to: Agent does not exist.");
            }
            catch (ArgumentException)
            {
                Console.WriteLine($"[LAUNCHER] Failed to start agent {agentId} due to: Agent already running.");
            }
            catch (InvalidOperationException)
            {
                // ignoring InvalidOperationException for the same reason as in 'Shutdown()' method...
            }
            catch (Exception e)
            {
                Console.WriteLine($"[LAUNCHER] Failed to start agent: {e.Message}");
            }
        }

        // writes to each agent process the new position of a specific agent
        public void MoveAgent(string agentId, string newPosition)
        {
            // can't move an agent who's process is currently dead
            if (_agents[agentId].HasExited)
            {
                Console.WriteLine($"[AGENT MANAGER] Couldn't move Agent '{agentId}' due to: Agent currently offline.");
                return;
            }

            // move only the specific agent
            string input = $"{agentId}|{newPosition}";
            WriteToInputStream(agentId, input);
        }

        // prints the agent processes
        public void PrintAgents()
        {
            Console.WriteLine("[LAUNCHER] Agent Processes:\n");
            if (_nodeRecords.IsEmpty)
            {
                Console.WriteLine("Empty.");
                return;
            }

            foreach (ProcessRecord record in _nodeRecords.Values)
                Console.WriteLine(record);
            Console.WriteLine();
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
                    Console.WriteLine($"[LAUNCHER] Failed to kill agent: {e.Message}");
                }
                finally
                {
                    agent.Dispose();                    
                }
            }

            _semaphore.Dispose(); // dispose the semaphore on shutdown

            // Note: creating and disposing the samaphore at the class level fixes the agent termination
            // and restart semaphore crash problem
        }
    }
}
