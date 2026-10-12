using System.Net;
using System.Net.Sockets;
using System.Text;
using TacMesh.Core.graph_related;
using TacMesh.Core.interfaces.beacon_and_destination;
using TacMesh.Core.utils;

namespace TacMesh.Core.communication.radio_communication
{
    /// <summary>
    /// Simulated beacon emitter used to send a Beacon to a remote endpoint to simulate
    /// a radio wave device broadcast to a wireless network.
    /// </summary>
    public class SimBeaconEmitter : IBeaconEmitter
    {
        // constant delay between beacons
        //private const int _beaconDelayMs = 1000;

        private readonly Socket _emittingSocket;
        private readonly IPEndPoint _virtualEndpoint; // the endpoint used to simulate a wireless space broadcast
        private readonly string _sourceID;
        private readonly int _sourcePort;


        // inject agent info and endpoint to emit to
        public SimBeaconEmitter(string sourceID, int sourcePort, IPEndPoint virtualEndpoint)
        {
            // any free ip and port
            _emittingSocket = SocketGenerator.GenerateUdpSocket();
            _virtualEndpoint = virtualEndpoint;
            _sourceID = sourceID;
            _sourcePort = sourcePort;
        }

        /// <summary>
        /// Emit a beacon containing the node's ID and port, with a provided location
        /// </summary>
        /// <param name="location">The location of the node to emit with the beacon.</param>
        public void EmitBeacon(Location location)
        {
            try
            {
                // constant agent beacon
                string beacon = $"{_sourceID}|{_sourcePort}|{location.X},{location.Y}";
                byte[] beaconBytes = Encoding.UTF8.GetBytes(beacon);

                // mcast the beacon to all radio model instances on the net
                _emittingSocket.SendTo(beaconBytes, _virtualEndpoint);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[BEACON EMITTER CRASH] '{e.Message}'");
            }
        }

        /// <summary>
        /// Tries to parse the given string signal as a beacon signal containing the SenderID and Endpoint
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="senderID"></param>
        /// <param name="endPoint"></param>
        /// <returns>True if successfult parsed the signal or False otherwise</returns>
        public static bool TryParseBeacon(string signal, out string? senderID, out IPEndPoint? endPoint, out Location? newLocaion)
        {
            try
            {
                // CURRENT signal structure: "ID|PORT|X,Y" (MAY CHANGE IN FUTURE)
                // for example: "Node-01|51001|1,1"
                string[] splitted = signal.Split('|');

                senderID = splitted[0];            // "ID|...."
                int port = int.Parse(splitted[1]); // "..|PORT"

                // split location parts
                string[] locationParts = splitted[2].Split(',');

                // parse the location
                double x = double.Parse(locationParts[0]);
                double y = double.Parse(locationParts[0]);
                newLocaion = new Location(x,y);

                // create endpoint from the parsed port
                endPoint = new IPEndPoint(IPAddress.Loopback, port);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Couldn't parse virtual destination: '{e.Message}'");
                senderID = null;
                endPoint = null;
                newLocaion = null;
                return false;
            }
        }
    }
}
