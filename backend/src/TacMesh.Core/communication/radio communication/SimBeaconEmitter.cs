using System.Net;
using System.Net.Sockets;
using TacMesh.Core.interfaces.beacon___destination;
using TacMesh.Core.socket_related;

namespace TacMesh.Core.communication.radio_communication
{
    /// <summary>
    /// Simulated beacon emitter used to send a Beacon to a remote endpoint to simulate
    /// a radio wave device broadcast to a wireless network.
    /// </summary>
    public class SimBeaconEmitter : IBeaconEmitter
    {
        private readonly Socket _emittingSocket;
        private IPEndPoint _virtualEndpoint; // the endpoint used to simulate a wireless space broadcast

        public SimBeaconEmitter(IPEndPoint virtualEndpoint)
        {
            // any free ip and port
            _emittingSocket = SocketGenerator.GenerateUdpSocket();
            _virtualEndpoint = virtualEndpoint;
        }

        /// <summary>
        /// Start emitting beacon to
        /// </summary>
        /// <param name="beacon"></param>
        /// <param name="remoteEndpoint"></param>
        public void EmitBeacon(byte[] beacon) => _emittingSocket.SendTo(beacon, _virtualEndpoint);
    }
}
