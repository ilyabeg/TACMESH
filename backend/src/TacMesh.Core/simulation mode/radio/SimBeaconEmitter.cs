using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TacMesh.Core.assets.node;
using TacMesh.Core.graph_related;
using TacMesh.Core.interfaces.beacon_and_destination;
using TacMesh.Core.utils;

namespace TacMesh.Core.communication.radio_communication
{
    // define the beacon structure
    //
    public class SimBeacon 
    {
        [JsonInclude]
        public string NodeId { get; private set; }
        [JsonInclude]
        public int Port { get; private set; }
        [JsonInclude]
        public Location Location { get; set; }

        public SimBeacon() { }
        public SimBeacon(string nodeId, int port, Location location) 
        {
            NodeId = nodeId;
            Port = port;
            Location = location;
        }
    }


    /// <summary>
    /// Simulated beacon emitter used to send a Beacon to a remote endpoint to simulate
    /// a radio wave device broadcast to a wireless network.
    /// </summary>
    public class SimBeaconEmitter : IBeaconEmitter
    {
        private readonly SimBeacon _beacon;
        private readonly Socket _emittingSocket;
        private readonly IPEndPoint _virtualEndpoint; // the endpoint used to simulate a wireless space broadcast
        private readonly int _sourcePort; // engine process source port assigned by OS


        // inject engine info and endpoint to emit to
        public SimBeaconEmitter(string nodeId, int sourcePort, IPEndPoint virtualEndpoint)
        {
            // any free ip and port
            _emittingSocket = SocketGenerator.GenerateUdpSocket();
            _virtualEndpoint = virtualEndpoint;
            _sourcePort = sourcePort;

            // create initial beacon
            _beacon = new SimBeacon(nodeId, sourcePort, null);
        }

        /// <summary>
        /// Emit a beacon containing the node's ID and port, with a provided location
        /// </summary>
        /// <param name="location">The location of the node to emit with the beacon.</param>
        public void EmitBeacon(Location location)
        {
            try
            {
                // constant engine beacon
                _beacon.Location = location;
                string jsonBeacon = JsonSerializer.Serialize(_beacon);
                byte[] beaconBytes = Encoding.UTF8.GetBytes(jsonBeacon);

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
        public static bool TryParseBeacon(string signal, out string? senderID, out IPEndPoint? endPoint, out Location? location)
        {
            try
            {
                // try to parse the json beacon
                SimBeacon? receivedBeacon = JsonSerializer.Deserialize<SimBeacon>(signal) ?? throw new NullReferenceException();

                // pull info
                senderID = receivedBeacon.NodeId;
                location = receivedBeacon.Location;
                endPoint = new IPEndPoint(IPAddress.Loopback, receivedBeacon.Port);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"[Sim Beacon Emitter] Couldn't parse virtual destination: '{e.Message}'");
                senderID = null;
                endPoint = null;
                location = null;
                return false;
            }
        }
    }
}
