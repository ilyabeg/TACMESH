using TacMesh.Core.assets.node;
using TacMesh.Core.hardware_mode;
using TacMesh.Core.interfaces;
using TacMesh.Core.tables.node_related;

namespace TacMesh.Agent
{
    public class Node
    {
        // Node Fields
        public string NodeID { get; private set; }
        public int AssignedPort { get; private set; }
        public ILocation NodeLocacation { get; private set; }

        // private fields
        private readonly ITransport _transporter;
        private readonly NeighborTable _neighborTable;
        private readonly HeartBeater _heartbeater;
        private readonly TopologyFilter _topologyFilter;
        private readonly PacketBuffer _packetBuffer;
        private readonly PacketProcessor _packetProcessor;


        // Constructor
        public Node(string nodeID, ILocation location, ITransport transporter, NeighborTable neighborTable, HeartBeater heartbeater, TopologyFilter topologyFilter)
        {
            // --- Node ID & Location --- 
            // save node id and initial location
            NodeID = nodeID;
            NodeLocacation = location;

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

            // --- Packet Processor --- 
            // create packet processor to process received packets
            _packetProcessor = new PacketProcessor(topologyFilter, heartbeater, neighborTable);

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
