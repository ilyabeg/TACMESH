using System.Collections.Concurrent;
using System.Net;
using System.Text;
using TacMesh.Core.assets;
using TacMesh.Core.communication.radio_communication;
using TacMesh.Core.events;
using TacMesh.Core.graph_related;
using TacMesh.Core.interfaces;
using TacMesh.Core.interfaces.communication_interfaces;
using TacMesh.Core.interfaces.raster;

namespace TacMesh.Core.models
{
    // keep record of node and his location
    internal class AgentRecord
    {
        public string NodeId { get; private set; }
        public Location CurrentLocation { get; set; }

        public AgentRecord(string nodeId, Location currentLocation)
        {
            NodeId = nodeId;
            CurrentLocation = currentLocation;
        }
    }

    /// <summary>
    /// Layer inside the Node process that determines before sending a Packet if the 
    /// communication can be established with the destination node. It simulates a radio communication layer.
    /// When using physical hardware this layer will be turned off and not used.
    /// </summary>
    public class VirtualRadioModel : IRadioModel
    {       
        // Radio Receiver to provide an independed component that can receive radio signals
        // that come from the simulated "wireless" VirtualRadio space 
        private readonly IReceiver _radioReceiver;

        // ILineOfSight implementor to check the Line of Sight on the DEM
        private readonly ILineOfSight _terrianTester;


        // virtual radio model field according to the design document
        private readonly ConcurrentDictionary<IPEndPoint, AgentRecord> _agent_locations;
        private readonly DRandom _dRandom; // deterministic random generator
        private const int _DELAY_RANGE = 5; // the random delay range 
        private readonly int _MAX_RANGE; // not constant because will eventually change in runtime


        // inject max range in constructor
        public VirtualRadioModel(int maximum_range, DRandom dRandom, IReceiver radioReceiver, ILineOfSight terrianTester)
        {
            _agent_locations = new ConcurrentDictionary<IPEndPoint, AgentRecord>();
            _MAX_RANGE = maximum_range;

            _dRandom = dRandom;
            _radioReceiver = radioReceiver;
            _radioReceiver.StartReceiving(); // start locations send to virtual space
            _radioReceiver.MessageReceivedEventHandler += (s, e) => OnMessageReceived(s, e);

            _terrianTester = terrianTester;
        }

        // add each node's assined port using the beacon and initial position using system configs
        private void OnMessageReceived(object s, MessageReceivedEventArgs e)
        {
            try
            {
                // try to parse the signal
                string signal = Encoding.UTF8.GetString(e.MessageBytes);

                // temp test
                //Console.WriteLine($"[VRadio Model] {_nodeId} Received: {signal}\n");

                if (SimBeaconEmitter.TryParseBeacon(signal, out string senderID, out IPEndPoint endPoint, out Location nodeLocation))
                {
                    // add node's address and location
                    _agent_locations[endPoint!] = new AgentRecord(senderID, nodeLocation);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VIRTUAL RADIO MODEL CRASH]: '{ex.Message}'");
            }
        }

        /// <summary>
        /// returns the current knwon peers in the network.
        /// </summary>
        public Dictionary<string, IPEndPoint> GetPeers()
        {
            // list to hold only approved endpoints
            Dictionary<string, IPEndPoint> peers = new Dictionary<string, IPEndPoint>();

            // create a dict of the peers by their id
            foreach (IPEndPoint nodeAddress in _agent_locations.Keys)
                peers.TryAdd(_agent_locations[nodeAddress].NodeId, nodeAddress);
            return peers;
        }

        /// <summary>
        /// Method to evaluate a connection between two agent nodes.
        /// </summary>
        /// <param name="source">Source address</param>
        /// <param name="destination">Destination address</param>
        /// <returns>True if a connection is valid. False otherwise.</returns>
        public async Task<bool> TestConnection(IPEndPoint source, IPEndPoint destination)
        {
            // pull id's
            string srcId = _agent_locations[source].NodeId;
            string dstId = _agent_locations[destination].NodeId;

            // combine id's to make dependend seed for the randoms
            string combinedId = srcId + "-" + dstId;

            // get the locations
            Location srcLocation = _agent_locations[source].CurrentLocation;
            Location dstLocation = _agent_locations[destination].CurrentLocation;

            // distance between the points
            double distance = srcLocation.Distance(dstLocation);

            // the 3 radio model tests
            if (!TestRange(distance)) return false;
            if (!TestRelativeDistance(distance, combinedId)) return false;
            if (!_terrianTester.TestLineOfSight(srcLocation, dstLocation)) return false;

            // generate a random delay
            await GenerateDelay(combinedId);
            return true;
        }

        // --- private testing methods ---

        // Max range test
        private bool TestRange(double distance) => distance <= _MAX_RANGE;

        // Relative Distance test
        private bool TestRelativeDistance(double distance, string id)
        {
            // EXPLANATION: the relative distance of the nodes is determined by the distance RELATIVE to the
            // maximum range between the nodes. since RangeTest succeeded, distance <= _MAX_RANGE whic means
            // distance / _MAX_RANGE return a number between 0.0 and 1.0
            // The Probability of the packet "surviving" the virtual radio transmission is: (1 - Relative_Distance) * 100
            // and the survival is determined by chance: roll a random number between 1-100, if it is less than
            // or equal to the Probability, the packet "survives" the simulated transmission

            // because: distance <= MAXIMUM_RANGE, the relative distance is between 0 and 1
            double relative_distance = distance / _MAX_RANGE;
            double probability_percent = (1 - relative_distance) * 100;

            // get a random number betweem 1-100
            double num = _dRandom.DetermineRandom(id) * 100;

            // test to check the genrated random
            //Console.WriteLine($"[DEBUG] ID: {id} | Prob: {probability_percent:F1}% | Rolled: {num:F1} | Passes: {num <= probability_percent}");

            // if number is in the 'probability side'
            return num <= probability_percent;
        }

        // rolls a random generated delay to simulate latency
        private async Task GenerateDelay(string id)
        {
            // _random.Next(RANGE) return a number between 0 and 5, adding the range again
            // makes the range be between 5 and 10 ms
            int delay_ms = _dRandom.DetermineRandom(id, _DELAY_RANGE + 1) + _DELAY_RANGE;
            await Task.Delay(delay_ms);
        }
    }
}
