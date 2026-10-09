using System.Collections.Concurrent;
using System.Net;
using System.Text;
using TacMesh.Core.events;
using TacMesh.Core.graph_related;
using TacMesh.Core.interfaces;
using TacMesh.Core.interfaces.communication_interfaces;
using TacMesh.Core.interfaces.raster;

namespace TacMesh.Core.models
{
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
        private readonly ConcurrentDictionary<string, IPEndPoint> _agent_locations;
        private readonly Random _random_generator;
        private const int _SEED = 13021995; // fixed seed to generate the same Randoms to repeat tests
        private const int _DELAY_RANGE = 5; // the random delay range 
        private readonly int _MAX_RANGE; // not constant because will eventually change in runtime


        // inject max range in constructor
        public VirtualRadioModel(int maximum_range, IReceiver radioReceiver, ILineOfSight terrianTester)
        {
            _agent_locations = new ConcurrentDictionary<string, IPEndPoint>();
            _random_generator = new Random(_SEED);            
            _MAX_RANGE = maximum_range;

            _radioReceiver = radioReceiver;
            _radioReceiver.StartReceiving(); // start locations send to virtual space
            _radioReceiver.MessageReceivedEventHandler += (s, e) => OnMessageReceived(s, e);

            _terrianTester = terrianTester;
        }

        // update location report based on the sender
        private void OnMessageReceived(object s, MessageReceivedEventArgs e)
        {
            try
            {
                // try to parse the signal
                string signal = Encoding.UTF8.GetString(e.MessageBytes);
                if (!TryParseSignal(signal, out string senderID, out IPEndPoint endPoint)) return;

                // add or update the node's address
                _agent_locations[senderID!] = endPoint!;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VIRTUAL RADIO MODEL CRASH]: '{ex.Message}'");
            }
        }

        /// <summary>
        /// Tries to parse the given string signal as a beacon signal containing the SenderID and Endpoint
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="senderID"></param>
        /// <param name="endPoint"></param>
        /// <returns>True if successfult parsed the signal or False otherwise</returns>
        private bool TryParseSignal(string signal, out string? senderID, out IPEndPoint? endPoint)
        {
            try
            {
                // CURRENT signal structure: "ID|PORT" (MAY CHANGE IN FUTURE)
                // for example: "Node-01|51001"
                string[] splitted = signal.Split('|');

                senderID = splitted[0];            // "ID|...."
                int port = int.Parse(splitted[1]); // "..|PORT"

                // create endpoint from the parsed port
                endPoint = new IPEndPoint(IPAddress.Loopback, port);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Couldn't parse virtual destination: '{e.Message}'");
                senderID = null;
                endPoint = null;
                return false;
            }
        }


        /// <summary>
        /// Method to check connection between two agent nodes.
        /// </summary>
        /// <param name="source">Source location</param>
        /// <param name="destination">Destination location</param>
        /// <returns>True if a connection is valid. False otherwise.</returns>
        public async Task<bool> TestConnection(Location source, Location destination)
        {
            // distance between the points
            double distance = source.Distance(destination);

            // the 3 radio model tests
            if (!TestRange(distance)) return false;
            if (!TestRelativeDistance(distance)) return false;
            if (!_terrianTester.TestLineOfSight(source, destination)) return false;

            // if all successful, generate delay
            await GenerateDelay();
            return true;
        }

        /// <summary>
        /// Method that provides the current neighbours the node can theoretically reach
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<string, IPEndPoint> GetNetworkDevices() => _agent_locations;


        // --- private testing methods ---

        // Max range test
        private bool TestRange(double distance) => distance <= _MAX_RANGE;


        // constants for upper/lower bound for random number
        private const int _upperBound = 101;
        private const int _lowerBound = 1;

        // Relative Distance test
        private bool TestRelativeDistance(double distance)
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
            // returns random between lower bound (including) and upper bound (excluding)
            int num = _random_generator.Next(_lowerBound, _upperBound);

            // if number is in the 'probability side'
            return num <= probability_percent;
        }

        // rolls a random generated delay to simulate latency
        private async Task GenerateDelay()
        {
            // _random.Next(RANGE) return a number between 0 and 5, adding the range again
            // makes the range be between 5 and 10 ms
            int delay_ms = _random_generator.Next(_DELAY_RANGE+1) + _DELAY_RANGE;
            await Task.Delay(delay_ms);
        }
    }
}
