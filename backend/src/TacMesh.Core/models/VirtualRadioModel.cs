using System.Net;
using System.Net.Sockets;
using TacMesh.Core.socket_related;

namespace TacMesh.Core.models
{
    /// <summary>
    /// Layer inside the Node process that determines before sending a Packet if the 
    /// communication can be established with the destination node. It simulates a radio communication layer.
    /// When using physical hardware this layer will be turned off and not used.
    /// </summary>
    public class VirtualRadioModel
    {
        public const int FixedPort = 55555; // fixed discovery port number
        private IPEndPoint _discoveryEndPoint;
        private Socket _discoverySocket;


        public VirtualRadioModel()
        {
            _discoveryEndPoint = new IPEndPoint(IPAddress.Any, FixedPort);

            // let the discovery socket to be bound to an address already in use
            _discoverySocket = SocketGenerator.GenerateUdpSocket();
            _discoverySocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            SocketGenerator.BindSocket(_discoverySocket, FixedPort);
        }
    }
}
