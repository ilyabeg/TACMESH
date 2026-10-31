using TacMesh.Core.assets.node;
using TacMesh.Core.hardware_mode;
using TacMesh.Core.interfaces;
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
        private readonly TopologyFilter _topologyFilter;
        private readonly PacketBuffer _packetBuffer;
        private readonly PacketProcessor _packetProcessor;


        // Constructor
        public Node(NodeInfo nodeInfo, NodeConfigs nodeConfigs, ITransport transporter, NeighborTable neighborTable, HeartBeater heartbeater, TopologyFilter topologyFilter)
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

            // --- Topology Filter --- 
            _topologyFilter = topologyFilter;

            // --- Packet Buffer ---
            // comes later ...

            // --- Packet Processor --- 
            // create packet processor to process received packets
            _packetProcessor = new PacketProcessor(_topologyFilter, _heartbeater, _neighborTable);

            // attach packet reading event handler
            _transporter.MessageReceived += (s, e) => _packetProcessor.ProcessPacket(e);


            // TEMPORARY TEST. THIS WILL NOT STAY HERE FOREVER.
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
