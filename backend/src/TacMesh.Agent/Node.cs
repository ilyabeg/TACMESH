using System.Net;
using System.Net.Sockets;
using System.Text;
using TacMesh.Core;

namespace TacMesh.Agent
{
    internal class Node
    {
        // magic numbers
        private readonly IPAddress _localhoast = IPAddress.Loopback; // localhost ip address (127.0.0.1)
        private readonly int _randomPort = 0; // let the OS decide the port
        private IPEndPoint _userEndPoint;
        private int _assignedPort; // the port which will be assigned eventually

        private readonly IPAddress _discoveryIP = IPAddress.Any;
        private readonly int _discoveryPort = 55555; // fixed discovery port number
        private IPEndPoint _discoveryEndPoint;

        // multicast group values
        private readonly IPAddress _mcastAddress = IPAddress.Parse("239.0.0.1");
        private MulticastOption _mcastOption;
        private IPEndPoint _mcastEndPoint;

        // end user socket
        public Socket UserSocket { get; private set; }
        private Socket _discoverySocket;

        // temporary message 
        private readonly byte[] _message;


        // Constructors
        public Node()
        {
            try
            {
                _mcastEndPoint = new IPEndPoint(_mcastAddress, _discoveryPort);
                CreateSockets();
                BindSockets();
                AddToMCastGroup();
                Task.Run(() => Messenger.ListenForBroadcast(_discoverySocket!, SaveReceivedMessage));
                Task.Run(() => Messenger.ListenForUnicast(UserSocket!));

                _assignedPort = ((IPEndPoint)UserSocket.LocalEndPoint).Port;
                _message = Encoding.UTF8.GetBytes($"DISCOVER#{_assignedPort}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
        }

        private void CreateSockets()
        {
            // let the discovery socket to be bound to an address already in use
            _discoverySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _discoverySocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // initiate the user socket with UDP protocol
            UserSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        private void BindSockets()
        {
            // bind socket to any ip and the fixed port
            _discoveryEndPoint = new IPEndPoint(_discoveryIP, _discoveryPort);
            _discoverySocket.Bind(_discoveryEndPoint);

            // bind user to local host ip and a random port
            _userEndPoint = new IPEndPoint(_localhoast, _randomPort);
            UserSocket.Bind(_userEndPoint);
        }

        private void AddToMCastGroup()
        {
            // add discovery socket to multicast group on ip 239.0.0.1
            _mcastOption = new MulticastOption(_mcastAddress);
            _discoverySocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, _mcastOption);
        }

        public void Test1()
        {
            Console.WriteLine($"Sending message from instance with port {_assignedPort}");
            Messenger.SendTo(_discoverySocket, _mcastEndPoint, _message);
        }
        
        private void SaveReceivedMessage(byte[] message, IPEndPoint remoteEP)
        {
            // ignore self messages
            if (remoteEP.Port == _assignedPort) return;
            Messenger.SendTo(UserSocket, remoteEP, message);
        }
    }
}
