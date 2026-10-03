using TacMesh.Core;
using TacMesh.Core.communication;
using TacMesh.Core.configurations;
using TacMesh.Core.database_related;
using TacMesh.Core.events;
using TacMesh.Core.interfaces;
using TacMesh.Core.models;
using TacMesh.Core.packet_related;
using TacMesh.Core.serializers;
using TacMesh.Core.tables.node_related;

namespace TacMesh.Agent
{
    public class Node
    {
        // Node Fields
        public string NodeID { get; private set; }
        public int AssignedPort { get; private set; }

        private ITransport _transporter;
        private HeartBeater _heartbeater;
        private VirtualRadioModel _radioModel;
        private NeighbourTable _neighbourTable;
        private PacketBuffer _packetBuffer;
        private HeartbeatLogger _heartbeatLogger;


        // Constructors
        public Node(string nodeID, ITransport transporter, NeighbourTable neighbourTable, HeartbeatLogger logger)
        {
            // start receiving background thread
            NodeID = nodeID;
            _transporter = transporter;
            _transporter.StartReceiving();
            AssignedPort = _transporter.GetAssignedPort();

            // start checking expiration in the background
            _neighbourTable = neighbourTable;
            _neighbourTable.StartExpirationCheck();
            _heartbeatLogger = logger;


            // TEMPORARILY START HEARTBEATS HERE, WILL CHANGE THE PLACEMENT IN THE FUTURE
            // start transmitting heartbeats
            StartHeartbeat();


            // attach packet reading event
            _transporter.MessageReceivedEventHandler += (s,e) => OnPacketReceived(s,e);

            // TEMPORARY TEST. THIS WILL NOT STAY HERE FOREVER.
            Console.WriteLine($"PORT={AssignedPort}");          

            // attach Ctrl+C event handler
            Console.CancelKeyPress += (s,e) => ShutdownNode();
            //Console.ReadKey();
        }

        // ----------------------------------------------------


        // TEMPORARILY TESTS! NOT INTENTED TO STAY HERE, WILL MOVE LOGIC IN THE FUTURE
        private void StartHeartbeat()
        {
            // remove own node from known nodes dict before handing to the hearbeater
            SystemConfigurations.StaticNodes.Remove(NodeID);

            // start heartbeating process
            _heartbeater = new HeartBeater(                
                NodeID,
                _heartbeatLogger,
                SystemConfigurations.StaticNodes,
                new PacketHeaderSerializer(),
                _transporter
            );
            _heartbeater.TransmitHeartbeat();
        }
        private void OnPacketReceived(object s, MessageReceivedEventArgs e)
        {
            PacketHeader header = PacketReader.ReadPacket(e);
            if (header.MsgType == PacketType.Heartbeat)
            {
                // log heartbeat
                _heartbeatLogger.FormatAndLogHeartbeat(LoggingMode.Received, header.SrcID, header.DstID);

                // add/update record of peer in neighbour table
                _neighbourTable.UpdateRecord(header.SrcID, e.RemoteEndPoint);
            }
        }


        // ----------------------------------------------------

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

                Console.WriteLine($"Shutting down Node on PORT={AssignedPort}.");
                _transporter.ShutDown();
            }
        }
    }
}
