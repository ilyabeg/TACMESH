using TacMesh.Core.assets.node;
using TacMesh.Core.assets.node.packet;
using TacMesh.Core.interfaces;
using TacMesh.Core.interfaces.communication;
using TacMesh.Core.tables.node_related;

namespace TacMesh.Engine
{
    public class Node
    {
        // Node Fields
        public NodeInfo NodeInfo { get; private set; }
        public NodeConfigs NodeConfigs { get; private set; } // hold configs ref to access the up to date settings
        public int AssignedPort { get; private set; }

        // private fields
        private readonly ITransport _transporter;
        private readonly NeighborTable _neighborTable;
        private readonly HeartBeater _heartbeater;
        private readonly PacketProcessor _packetProcessor;
        private readonly IInputListener _inputListener;


        // Constructor
        public Node(NodeInfo nodeInfo, NodeConfigs nodeConfigs, ITransport transporter, NeighborTable neighborTable, HeartBeater heartbeater, PacketProcessor packetProcessor, IInputListener inputListener)
        {
            // --- Node Info & Settings --- 
            NodeInfo = nodeInfo;
            NodeConfigs = nodeConfigs;

            // --- Transporter --- 
            // start receiving background thread            
            _transporter = transporter;
            _transporter.StartReceiving();
            AssignedPort = _transporter.GetAssignedPort();

            // --- Neighbor Table --- 
            // start checking expiration in the background
            _neighborTable = neighborTable;
            _neighborTable.StartExpirationCheck();

            // --- Heart Beater --- 
            // start transmitting heartbeats
            _heartbeater = heartbeater;
            _heartbeater.TransmitHeartbeat();

            // --- Packet Processor --- 
            // attach packet reading event handler
            _packetProcessor = packetProcessor;            
            _transporter.MessageReceived += (s, e) => _packetProcessor.ProcessPacket(e);

            // --- Input Listener ---
            _inputListener = inputListener;
            _inputListener.ListenForInput();

            // --- Node Initialization Signal ---
            Console.WriteLine($"PORT={AssignedPort}");          

            // attach Ctrl+C event handler
            Console.CancelKeyPress += (s,e) => ShutdownNode();
        }
        

        // flag to determine if the Node is already shutting down
        private bool _shutdown_flag = false;
        private readonly object _shutdown_lock = new object();

        /// <summary>
        /// Shuts down the Node gracefully.
        /// </summary>
        private void ShutdownNode()
        {
            lock (_shutdown_lock)
            {
                if (_shutdown_flag) return; // another thread already shutting down the Node
                _shutdown_flag = true; // claim shutdown

                Console.WriteLine($"Shutting down Node on PORT={AssignedPort}.\n");
                _transporter.ShutDown();
            }
        }
    }
}
