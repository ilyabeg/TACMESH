using System.Text.Json;
using TacMesh.Core.assets;
using TacMesh.Core.assets.node;
using TacMesh.Core.graph_related;
using TacMesh.Core.interfaces.beacon_and_destination;
using TacMesh.Core.utils.ipc.types;

namespace TacMesh.Core.launcher
{
    public class LauncherControl
    {
        private readonly string _nodeId;
        private readonly NodeConfigs _nodeConfigs; // hold ref to configs to update at runtime
        private readonly NodeAdvancer _advancer;

        // an emitter to TEMPORARILY emit a beacon to each radio model on the net of the updated
        // location of the node upon advancing it on it's path.
        // when full data packet with position containing heartbeats is implemented, this emitter
        // will be removed. it is currently strictly temporary for the simplified position advancing.
        private readonly IBeaconEmitter _emitter;

        public LauncherControl(string nodeId, NodeConfigs nodeConfigs, IBeaconEmitter emitter)
        {
            _nodeId = nodeId;
            _nodeConfigs = nodeConfigs;
            _advancer = new NodeAdvancer(nodeConfigs);
            _emitter = emitter;

            // attach node position update event
            _advancer.NodePositionUpdate += (s, newLocation) => OnNodePositionUpdate(newLocation);
        }

        // listens for StdInput pushed via the StdInputStream from the Launcher (the scenario manager)
        public void ListenForStdInput()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try {
                        // parse input pushed from launcher
                        string input = Console.ReadLine();
                        TryParseStdInput(input);

                        // test received input
                        Console.WriteLine($"[LAUNCHER CONTROL] received: \"{input}\"");
                    }
                    catch (Exception e) {
                        Console.WriteLine($"[LAUNCHER CONTROL] Error: \"{e.Message}\"\n");
                    }
                }
            });
        }

        // handler of node moved event
        private void OnNodePositionUpdate(Location newLocation)
        {
            // set and print out the new location
            _nodeConfigs.UpdateConfigurations(location: newLocation);
            Console.WriteLine($"[LAUNCHER CONTROL] Moved Node <{_nodeId}> to new position: ({newLocation.X},{newLocation.Y}).\n");

            // TEMOPRARILY update the position for all engine prcesses via mcasting to all of the engine
            // processes using the beacon emitter. this will be removed as soon as a full data packet
            // with node position reports in the heartbeats is implemented.
            //
            _emitter.EmitBeacon(newLocation);

            // push to the new pos to std output stream
            MoveMessage posUpdate = new MoveMessage(_nodeId, newLocation);
            string jsonPosUpdate = JsonSerializer.Serialize(posUpdate);            
            PushToLauncher(jsonPosUpdate);
        }

        // pushes a std io message to the launcher 
        private void PushToLauncher(string payload) => Console.WriteLine(payload);


        // --- parsing methods ---

        // parse launcher position change inputs
        private void TryParseStdInput(string input)
        {
            // parse inputs
            if (TryParseEmitBeacon(input)) return;
            if (TryParseMov(input)) return;
            if (TryParseAdv(input).Result) return;
            if (TryParseConfigs(input)) return;

            Console.WriteLine($"[Launcher Control] Couldn't parse StdInput due to: Unknown Input.");
        }

        // parsing the emit beacon signal
        private bool TryParseEmitBeacon(string input)
        {
            if (input.Equals("EMIT_INITIATION_BEACON"))
            {
                // emit an initial beacon so other nodes recognize this node
                _emitter.EmitBeacon(_nodeConfigs.Location);
                return true;
            }
            return false;
        }

        // parsing a move command updates the node location instantly
        private bool TryParseMov(string input)
        {
            try
            {
                // try to parse the move command json string
                MoveMessage movCommand = JsonSerializer.Deserialize<MoveMessage>(input);

                // ignore a message from the launcher not meant for this specific node
                string nodeId = movCommand.NodeId;
                if (!nodeId.Equals(_nodeId)) return false;

                // if successfull, pull the engine data
                Location newLocation = movCommand.Location;
                _nodeConfigs.UpdateConfigurations(location: newLocation);

                // TEST
                //Console.WriteLine($"[LAUNCHER CONTROL] Updated node location to: {movCommand.Location} successfuly");

                // push the same exact message back to the launcher to acknowledge the
                // position update to synchronize with the ui
                PushToLauncher(input);

                return true;
            }
            catch (Exception) {
                return false;
            }
        }

        // parse node advance signal
        private async Task<bool> TryParseAdv(string input)
        {
            try {
                if (!input.Equals("INIT_ADVANCE")) return false;

                // advance waypoints of the node
                if (!await _advancer.AdvancePosition())
                    Console.WriteLine($"[Launcher Control] Node Advancer failed to advance the node \'{_nodeId}\'.\n");
                return true;
            }
            catch (Exception ) {
                return false;
            }
        }

        // parse the new node configurations and update them
        private bool TryParseConfigs(string input)
        {
            try
            {
                NodeConfigs newConfigs = JsonSerializer.Deserialize<NodeConfigs>(input);
                if (newConfigs == null) return false;                

                _nodeConfigs.UpdateConfigurations(newConfigs);

                // push to the launcher process the same exact message to acknowledge that
                // the new config settings have been successfuly updated
                //
                PushToLauncher(input);

                return true;
            }
            catch (Exception) {
                return false;
            }
        }
    }
}
