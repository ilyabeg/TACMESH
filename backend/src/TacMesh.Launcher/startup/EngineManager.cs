using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using TacMesh.Core.graph_related;
using TacMesh.Core.utils.ipc.types;
using TacMesh.Launcher.info;
using TacMesh.Launcher.processes;

namespace TacMesh.Launcher.startup
{
    public class EngineManager
    {
        // keep record for every Node process by their id
        // concurrent because many threads may write to the dictionary when receiving 'PORT=...' on stdout
        private readonly ConcurrentDictionary<string, ProcessRecord> _nodeRecords = new();

        // keep track of all engine processes by the engine ID
        // regular dictionary because only the main thread alters this (command executions)
        private readonly Dictionary<string, Process> _engines;

        // Semaphore to limit the number of processes the EngineManager creates at a time
        private readonly Semaphore _semaphore;
        private const int _initialCount = 0; // the initial number of requests for the semaphore that can be granted
        private const int _maximumCount = 1; // the maximum number of requests for the semaphore that can be granted
        private const string _semName = "TacMesh-Global-Sem"; // the name that the semaphore uses

        // public event to invoke the waypoint manager upon a node reaching the destination waypoint
        public event EventHandler<string> WaypointReached;


        // constructor
        public EngineManager(Dictionary<string, Process> engines)
        {
            // create a named semaphore that can be shared accross processes
            _semaphore = new Semaphore(_initialCount, _maximumCount, _semName);
            _engines = engines;
        }


        // returns the current engines processes
        public List<ProcessRecord> GetEnginesList() => _nodeRecords.Values.ToList();


        // attach data received event to each process before starting it
        public void StartEngines()
        {
            // create a list of the sorted id's in the dict
            // O(nlogn) + O(n) foreach loop, and O(n) extra space for the list
            List<string> sortedIds = _engines.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Key).ToList();
            
            foreach (string engineId in sortedIds)
            {
                Process engine_process = _engines[engineId];

                // attach echo data received event handler and pass engineId using the lambda,
                // this saves the engineId for every Engine's own event handler with this specific id
                engine_process.OutputDataReceived += (s, e) => OnStdOutReceived(engineId, s, e);

                engine_process.Start();
                engine_process.BeginOutputReadLine(); // lets see output

                // freeze creation here until semaphore is released
                _semaphore.WaitOne();
            }

            // make the engines emit the beacon only after starting up all of the processes
            foreach (string engine in _engines.Keys)
                WriteToInputStream(engine, "EMIT_INITIATION_BEACON");
        }

        // each EchoOutput gets spesific engineId by the engine attatching this handler with their id on creation
        private void OnStdOutReceived(string engineId, object sender, DataReceivedEventArgs e)
        {            
            string output = e.Data;
            if (output == null) return;

            // print each engine's output in a FIXED format
            Printer.AddNewLine(engineId, output);

            // parse output if possible
            TryParseStdout(engineId, sender, output);
        }

        // tries to parse the stdoutput
        private void TryParseStdout(string engineId, object sender, string output)
        {
            // save process in record dict
            if (output.StartsWith("PORT="))
            {
                int port = int.Parse(output.Split('=')[1]);
                string processId = ((Process)sender).Id.ToString();
                AddRecord(engineId, processId, port);
            }

            // invoke the waypoint manager if advance signal received
            if (output.Equals("adv."))
                WaypointReached?.Invoke(this, engineId);
        }

        // writes input to the input stream of a specfic engine
        public void WriteToInputStream(string engineId, string input)
        {
            // find the specific process and write a line to it's input stream
            Process engineProcess = _engines[engineId];
            engineProcess.StandardInput.WriteLine(input);
        }

        // TEMPORARY initializer method to start the waypoint advances
        // this will be changed in the future to handle the pos advances differently
        public void InitAdvances()
        {
            // send all engines their first waypoint
            foreach (string engineId in _engines.Keys)
                WaypointReached?.Invoke(this, engineId);
        }


        // tries to add record by engine id
        private void AddRecord(string engineId, string pId, int port) => _nodeRecords.TryAdd(engineId, new ProcessRecord(engineId, pId, port));


        // tries to remove record by engine id
        private void RemoveRecord(string engineId) => _nodeRecords.TryRemove(engineId, out _);


        // kills a specific process of an engine by their ID
        public void KillEngine(string engineId)
        {
            try
            {
                Process engine = _engines[engineId];
                if (engine.HasExited) throw new ArgumentException();

                engine.Kill(true);
                engine.WaitForExit();
                RemoveRecord(engineId);

                // notify the ui about the agent death
                StdOut(IOMessageType.EngineDown, engineId);
            }
            catch (KeyNotFoundException) {
                Console.WriteLine($"[LAUNCHER] Failed to kill engine {engineId} due to: Engine does not exist.");
            }
            catch (ArgumentException) {
                Console.WriteLine($"[LAUNCHER] Failed to kill engine {engineId} due to: Engine already terminated.");
            }
            catch (InvalidOperationException) {
                // ignoring InvalidOperationException for the same reason as in 'Shutdown()' method...
            }
            catch (Exception e) {
                Console.WriteLine($"[LAUNCHER] Failed to kill engine: {e.Message}");
            }
        }

        // start a specific process of an engine by their ID
        public void StartEngine(string engineId)
        {
            try
            {
                // give access to a new Process instance only if id existed
                Process engine = _engines[engineId];
                if (!engine.HasExited) throw new ArgumentException();

                // re-create Process instance to not use the same 'dead' one
                engine = ProcessCreator.CreateNewEngineInstance(engineId);

                // attach output handler, and update engines Dictionary
                engine.OutputDataReceived += (s,e) => OnStdOutReceived(engineId, s, e);
                _engines[engineId] = engine;

                // should automatically add record back with new process id
                // because each engine prints his port on startup ("PORT=<number>")
                engine.Start();
                engine.BeginOutputReadLine();

                // notify the ui that the agent is back online
                StdOut(IOMessageType.EngineUp, _nodeRecords[engineId]);
            }
            catch (KeyNotFoundException) {
                Console.WriteLine($"[LAUNCHER] Failed to start engine {engineId} due to: Engine does not exist.");
            }
            catch (ArgumentException) {
                Console.WriteLine($"[LAUNCHER] Failed to start engine {engineId} due to: Engine already running.");
            }
            catch (InvalidOperationException) {
                // ignoring InvalidOperationException for the same reason as in 'Shutdown()' method...
            }
            catch (Exception e) {
                Console.WriteLine($"[LAUNCHER] Failed to start engine: {e.Message}");
            }
        }

        // writes to each engine process the new position of a specific engine
        public void MoveEngine(string engineId, Location newPosition)
        {
            // can't move an engine who's process is currently dead
            if (_engines[engineId].HasExited)
            {
                Console.WriteLine($"[ENGINE MANAGER] Couldn't move Engine '{engineId}' due to: Engine currently offline.");
                return;
            }

            // move only the specific engine
            string jsonMovCommand = JsonSerializer.Serialize(new MoveCommand(engineId, newPosition));
            WriteToInputStream(engineId, jsonMovCommand);
        }

        // prints the engine processes
        public void PrintEngines()
        {
            Console.WriteLine("[LAUNCHER] Engine Processes:\n");
            if (_nodeRecords.IsEmpty)
            {
                Console.WriteLine("Empty.");
                return;
            }

            foreach (ProcessRecord record in _nodeRecords.Values)
                Console.WriteLine(record);
            Console.WriteLine();
        }

        // prints out a std io message in json format
        private void StdOut(IOMessageType messageType, object data)
        {
            StdIOMessage outputMessage = new StdIOMessage(messageType, data);
            string jsonMessage = JsonSerializer.Serialize(outputMessage);
            Console.WriteLine(jsonMessage);
        }

        public void Shutdown()
        {
            foreach (Process engine in _engines.Values)
            {
                try
                {
                    if (!engine.HasExited)
                    {
                        engine.Kill(true);
                        engine.WaitForExit();
                    }
                }
                catch (InvalidOperationException)
                {
                    // InvalidOperationException is thrown if the Launcher tries to kill any process that is already
                    // terminated. i've added this ignoring catch block because when pressing CTRL+C in the 
                    // Laucnher's console, the OS automatically sends all processes that are linked to the launcher
                    // an interrupt exit signal, and then the Launcher runs this Shutdown() method. this causes a race
                    // condition: when the launcher checks !engine.HasExited on an engine instance, he may be alive at
                    // that microsecond, but at that exact moment he suddenly got the exit signal from the OS, then
                    // the launcher tries engine.Kill(true), and a InvalidOperationException is thrown.
                    // the reason i'm ignoring this exception is because either way i get the outcome that i want:
                    // the engine process is terminated. it doesn't matter if the launcher won the race against the OS
                    // and killed the instance or vice versa.
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[LAUNCHER] Failed to kill engine: {e.Message}");
                }
                finally
                {
                    engine.Dispose();                    
                }
            }

            _semaphore.Dispose(); // dispose the semaphore on shutdown

            // Note: creating and disposing the samaphore at the class level fixes the engine termination
            // and restart semaphore crash problem
        }
    }
}
