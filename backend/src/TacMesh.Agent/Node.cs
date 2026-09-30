using System.Net;
using System.Net.Sockets;
using System.Text;
using TacMesh.Core;
using TacMesh.Core.packet_related;
using TacMesh.Core.serializing_related;

namespace TacMesh.Agent
{
    public class Node
    {
        #region magic numbers
        private readonly IPAddress _localhoast = IPAddress.Loopback; // localhost ip address (127.0.0.1)
        private readonly int _randomPort = 0; // let the OS decide the port
        private IPEndPoint _userEndPoint;        

        private readonly IPAddress _discoveryIP = IPAddress.Any;
        private readonly int _discoveryPort = 55555; // fixed discovery port number
        private IPEndPoint _discoveryEndPoint;
        #endregion

        #region multicast group values
        private readonly IPAddress _mcastAddress = IPAddress.Parse("239.0.0.1");
        // MulticastOption is a class that provides the IPAddress values used to join or drop an IPv4 multicast group
        private MulticastOption _mcastOption;
        private IPEndPoint _mcastEndPoint;
        #endregion

        #region end user socket and discovery socket
        public Socket UserSocket { get; private set; }
        private Socket _discoverySocket;
        public int AssignedPort { get; private set; } // the port which will be assigned eventually
        #endregion

        #region Node Fields
        public string NodeID { get; private set; }
        #endregion

        // temporary message 
        private readonly byte[] _discover_message;

        // Serializers
        private readonly PacketHeaderSerializer _header_serializer;


        // Constructors
        public Node(string nodeID)
        {
            try
            {
                NodeID = nodeID;
                _mcastEndPoint = new IPEndPoint(_mcastAddress, _discoveryPort);
                _header_serializer = new PacketHeaderSerializer();
                CreateSockets();
                BindSockets();
                AddToMCastGroup();
                RunListeners();
                // attach Ctrl+C event handler
                Console.CancelKeyPress += (sender, e) => ShutdownNode();

                AssignedPort = ((IPEndPoint)UserSocket.LocalEndPoint).Port;
                Console.WriteLine($"PORT={AssignedPort}");

                _discover_message = Encoding.UTF8.GetBytes($"DISCOVER#{AssignedPort}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error Constructing Node: '{e.Message}'");
                ShutdownNode();
            }
        }

        // flag to determine if the Node is already shutting down
        private bool _shutdown_flag = false;
        private readonly object _shutdown_lock = new object();
        /// <summary>
        /// Shuts down the User and Discovery Sockets gracefully. Executing Socket.Close() internally executes
        /// Socket.Dispose() (check Socket.Close() definition), so handling disposing of the Socket is unnecessary.
        /// 
        /// NOTE: By using the null-conditional operator (?) I ensure no exception is thrown if a null Socket exists.
        /// </summary>
        private void ShutdownNode()
        {
            lock (_shutdown_lock)
            {
                if (_shutdown_flag) return; // another thread already shutting down the Node
                _shutdown_flag = true; // claim shutdown

                // drop mcast group and close the sockets
                Console.WriteLine($"Shutting down Node on PORT={AssignedPort}.");
                _discoverySocket?.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.DropMembership, _mcastOption);
                _discoverySocket?.Close();
                UserSocket?.Close();
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

        /// <summary>
        /// Adds the Discovery Socket to the multicast group that sits on the IPV4 Address: 239.0.0.1.
        /// </summary>
        private void AddToMCastGroup()
        {
            // add discovery socket to multicast group on ip 239.0.0.1
            _mcastOption = new MulticastOption(_mcastAddress);
            _discoverySocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, _mcastOption);
        }

        /// <summary>
        /// Runs the listening loops in background threads using Tasks.
        /// </summary>
        private void RunListeners()
        {
            Task.Run(() => Messenger.ListenForBroadcast(_discoverySocket, AssignedPort, SaveReceivedMessage, ShutdownNode));
            Task.Run(() => Messenger.ListenForUnicast(UserSocket, SaveReceivedMessage, ShutdownNode));
        }

        public void Test()
        {
            Console.WriteLine($"Sending message from instance with port {AssignedPort}");
            Messenger.SendTo(_discoverySocket, _mcastEndPoint, _discover_message);
        }
        
        /// <summary>
        /// Callback function used for pushing received messages from the background threads to the Agent.
        /// </summary>
        /// <param name="message">The bytes that were received.</param>
        /// <param name="remoteEP">The remote EndPoint from which the message was received.</param>
        private void SaveReceivedMessage(byte[] message, IPEndPoint remoteEP)
        {
            // ignore self messages
            if (remoteEP.Port == AssignedPort) return;

            // TEMP TEST: try header transmition
            if (message.Length >= PacketHeader.HeaderSize)
            {
                PacketHeader? received_header = _header_serializer.Deserialize(message);
                if (received_header == null) return;

                Console.WriteLine($"{AssignedPort} Received Packet Header from PORT: {remoteEP.Port}");
                Console.WriteLine(received_header);
            }
            else
            {
                Console.WriteLine($"{AssignedPort} Received message from PORT: {remoteEP.Port}");

                // transmit a header to remote user
                PacketHeader header_test = PacketHeader.BuildPacketHeader(1, NodeID, 0, 0, 0, PacketType.Heartbeat, $"1234567890123456", "TMP-DST", 1, 0);

                byte[] header_bytes = _header_serializer.Serialize(header_test);

                Messenger.SendTo(UserSocket, remoteEP, header_bytes);
            }
        }
    }
}
