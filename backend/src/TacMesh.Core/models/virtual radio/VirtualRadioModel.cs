using System.Collections.Concurrent;
using System.Net;
using System.Text;
using TacMesh.Core.events;
using TacMesh.Core.graph_related;
using TacMesh.Core.interfaces;
using TacMesh.Core.interfaces.communication_interfaces;

namespace TacMesh.Core.models
{

    // TEMPORARY RADIO MODEL LOCATION REPORT CLASS TO HOLD LOCATION REPORT FROM AGENTS
    public class TempLocationReport : IDestination
    {
        public string NodeId { get; private set; }
        public IPEndPoint Address { get; private set; }
        public GraphPoint LocationPoint { get; private set; }
        public TempLocationReport(string nodeId, IPEndPoint address, GraphPoint locationPoint)
        {
            NodeId = nodeId;
            Address = address;
            LocationPoint = locationPoint;
        }

        /// <summary>
        /// static method to create a location report out of a parsed string
        /// </summary>
        /// <param name="signal"></param>
        /// <returns>The result of the parsing with the matching location report value</returns>
        public static (bool Result, TempLocationReport? LocationReport) ParseSignal(string signal)
        {
            try
            {
                // CURRENT signal structure: "ID|PORT|X|Y" (MAY CHANGE IN FUTURE)
                // for example: "Node-01|51001|10|10"
                string[] splitted = signal.Split('|');

                string nodeId = splitted[0];       // "ID|....|.|."
                int port = int.Parse(splitted[1]); // "..|PORT|.|."
                int x = int.Parse(splitted[2]);    // "..|....|X|."
                int y = int.Parse(splitted[3]);    // "..|....|.|Y"

                // create endpoint and graph point from the parsed string
                IPEndPoint address = new IPEndPoint(IPAddress.Loopback, port);
                GraphPoint location = new GraphPoint(x,y);

                return (true, new TempLocationReport(nodeId, address, location));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Received invalid beacon signal: '{e.Message}'");
                return (false, null);
            }
        }
    }

    /// <summary>
    /// Layer inside the Node process that determines before sending a Packet if the 
    /// communication can be established with the destination node. It simulates a radio communication layer.
    /// When using physical hardware this layer will be turned off and not used.
    /// </summary>
    public class VirtualRadioModel : IRadioModel, IDestinationProvider
    {       
        // Radio Receiver to provide an independed component that can receive radio signals
        // that come from the simulated "wireless" VirtualRadio space 
        private readonly IReceiver _radioReceiver;
        
        // virtual radio model field according to the design document
        private readonly ConcurrentDictionary<IPEndPoint, IDestination> _agent_locations;
        private readonly Random _random_generator;
        private const int _SEED = 13021995; // fixed seed to generate the same Randoms to repeat tests
        private const int _DELAY_RANGE = 5; // the random delay range 
        private readonly int _MAX_RANGE; // not constant because will eventually change in runtime


        // inject max range in constructor
        public VirtualRadioModel(int maximum_range, IReceiver radioReceiver)
        {
            _agent_locations = new ConcurrentDictionary<IPEndPoint, IDestination>();
            _random_generator = new Random(_SEED);            
            _MAX_RANGE = maximum_range;

            _radioReceiver = radioReceiver;
            _radioReceiver.StartReceiving(); // start locations send to virtual space

            _radioReceiver.MessageReceivedEventHandler += (s,e) => OnMessageReceived(s,e);
        }

        // update location report based on the sender
        private void OnMessageReceived(object s, MessageReceivedEventArgs e)
        {
            try
            {
                string message = Encoding.UTF8.GetString(e.MessageBytes);

                // try to parse the signal
                var parsingResult = TempLocationReport.ParseSignal(message);

                // if the signal couldn't be parsed, ignore it
                if (!parsingResult.Result) return;

                TempLocationReport locationReport = parsingResult.LocationReport;

                // add or update the node's the new report
                _agent_locations[locationReport.Address] = locationReport;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[VIRTUAL RADIO MODEL CRASH]: '{ex.Message}'");
            }
        }


        /// <summary>
        /// Method to check connection between two agent nodes
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns>True if a connection is valid. False otherwise.</returns>
        public async Task<bool> TestConnection(IPEndPoint source, IPEndPoint destination)
        {
            var test1_result = TestRange(source, destination);
            if (!test1_result.Result) 
                return false;

            if (!TestRelativeDistance(test1_result.Distance)) 
                return false;

            if (!TestLineOfSight(source, destination)) 
                return false;

            await GenerateDelay();
            return true;
        }

        /// <summary>
        /// Method that provides the current neighbours the node can theoretically reach
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<IPEndPoint, IDestination> ProvideDestinations() => _agent_locations;


        // --- private testing methods ---

        // Max range test
        private (bool Result, double Distance) TestRange(IPEndPoint source, IPEndPoint destination)
        {
            GraphPoint point1 = _agent_locations[source].LocationPoint;
            GraphPoint point2 = _agent_locations[destination].LocationPoint;

            double distance = point1.Distance(point2);
            return distance > _MAX_RANGE ? (false, double.MinValue) : (true, distance);
        }

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

        // valid Line of Sight test
        private bool TestLineOfSight(IPEndPoint source, IPEndPoint destination)
        {
            // TEMPORARILY ALWAYS TRUE UNTILL ACTUALY IMPLEMENTED LINE OF SIGHT ALGORITHM.
            return true;
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
