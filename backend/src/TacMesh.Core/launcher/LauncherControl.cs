using TacMesh.Core.assets;
using TacMesh.Core.graph_related;
using TacMesh.Core.interfaces.beacon_and_destination;
using TacMesh.Core.models;

namespace TacMesh.Core.launcher
{
    public class LauncherControl
    {
        // launcher control is specificaly used only during simuation mode, thus
        // using specificaly the VIRTUAL Radio Model.
        private readonly VirtualRadioModel _radioModel;
        private readonly string _srcNode; // the Node that the Launcher is pushing the data to
        private Location _currentLocation; // holds the current node location
        private readonly double _nodeSpeed; // holds the current node speed
        private readonly NodeAdvancer _advancer;

        // an emitter to TEMPORARILY emit a beacon to each radio model on the net of the updated
        // location of the node upon advancing it on it's path.
        // when full data packet with position containing heartbeats is implemented, this emitter
        // will be removed. it is currently strictly temporary for the simplified position advancing.
        private readonly IBeaconEmitter _emitter;

        public LauncherControl(VirtualRadioModel radioModel, string srcNode, Location intialLocation, double speed, IBeaconEmitter emitter)
        {
            _radioModel = radioModel;
            _srcNode = srcNode;
            _currentLocation = intialLocation;
            _nodeSpeed = speed;
            _advancer = new NodeAdvancer();
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

                        if (input.Equals("EMIT_INITIATION_BEACON"))
                        {
                            // emit an initial beacon so other nodes recognize this node
                            _emitter.EmitBeacon(_currentLocation!);
                            continue;
                        }

                        // test receiving
                        //Console.WriteLine($"[LAUNCHER CONTROL] received: \"{input}\"");

                        if (TryParseStdInput(input, out string nodeId, out Location newLocation))
                        {
                            // ignore a message from the launcher not meant for this specific node
                            if (!_srcNode.Equals(nodeId)) continue;

                            // advance position of the node
                            if (await _advancer.AdvancePosition(_currentLocation, newLocation, _nodeSpeed))
                            {
                                // print out the new location
                                _currentLocation = newLocation;
                                Console.WriteLine($"[LAUNCHER CONTROL] Moved Node <{nodeId!}> to new position: ({newLocation.X},{newLocation.Y}).\n");

                                // mcast the final location after end to be extra precise
                                OnNodePositionUpdate(newLocation);

                                // print a signal that the node advanced to notify launcher
                                Console.WriteLine($"adv.\n");
                            }
                            else {
                                Console.WriteLine($"Node Advancer failed to advance the node to the new position: {newLocation}.\n");
                            }
                        }
                    }
                    catch (Exception e) {
                        Console.WriteLine($"[LAUNCHER CONTROL] Error: \"{e.Message}\"\n");
                    }
                }
            });
        }


        // TEMOPRARILY update the position via mcasting to all of the engine processes
        // using the beacon emitter. this will be removed as soon as a full data packet
        // with node position reports in the heartbeats is implemented.
        private void OnNodePositionUpdate(Location newLocation) => _emitter.EmitBeacon(newLocation);


        // parse launcher position change inputs
        private bool TryParseStdInput(string input, out string? nodeId, out Location? newLocation)
        {
            try
            {
                string[] splitted = input.Split('|');
                if (splitted.Length != 2) throw new ArgumentException("Expected position was invalid");

                // input supposed to simply be 'NodeID|X,Y'
                nodeId = splitted[0];

                // split the 'X,Y' string
                string[] position = splitted[1].Split(',');
                double x = double.Parse(position[0]);
                double y = double.Parse(position[1]);

                // create new pos from the launcher change
                newLocation = new Location(x,y);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Couldn't parse StdInput: '{e.Message}'");
                newLocation = null;
                nodeId = null;
                return false;
            }
        }
    }
}
