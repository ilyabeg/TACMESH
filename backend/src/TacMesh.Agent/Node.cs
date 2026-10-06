using TacMesh.Core;
using TacMesh.Core.communication;
using TacMesh.Core.database_related;
using TacMesh.Core.events;
using TacMesh.Core.interfaces;
using TacMesh.Core.packet_related;
using TacMesh.Core.tables.node_related;

namespace TacMesh.Agent
{
    public class Node
    {
        // Node Fields
        public string NodeID { get; private set; }
        public int AssignedPort { get; private set; }

        private readonly ITransport _transporter;
        private readonly HeartBeater _heartbeater;
        private readonly NeighbourTable _neighbourTable;
        private readonly PacketBuffer _packetBuffer;        


        // Constructors
        public Node(string nodeID, ITransport transporter, NeighbourTable neighbourTable, HeartBeater heartbeater)
        {
            NodeID = nodeID;

            // start receiving background thread            
            _transporter = transporter;
            _transporter.StartReceiving();
            AssignedPort = _transporter.GetAssignedPort();
            // attach packet reading event
            _transporter.MessageReceivedEventHandler += (s, e) => OnPacketReceived(s, e);

            // start checking expiration in the background
            _neighbourTable = neighbourTable;
            _neighbourTable.StartExpirationCheck();

            // start transmitting heartbeats
            _heartbeater = heartbeater;
            _heartbeater.TransmitHeartbeat();            

            // TEMPORARY TEST. THIS WILL NOT STAY HERE FOREVER.
            Console.WriteLine($"PORT={AssignedPort}");          

            // attach Ctrl+C event handler
            Console.CancelKeyPress += (s,e) => ShutdownNode();
        }

        // ----------------------------------------------------


        // TEMPORARILY TESTS! NOT INTENTED TO STAY HERE, WILL MOVE LOGIC IN THE FUTURE
        private void OnPacketReceived(object s, MessageReceivedEventArgs e)
        {
            PacketHeader header = PacketReader.ReadPacket(e);
            if (header.MsgType == PacketType.Heartbeat)
            {
                // log heartbeat
                _heartbeater.LogHeartbeat(LoggingMode.Received, header.SrcID, header.DstID);

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

                Console.WriteLine($"Shutting down Node on PORT={AssignedPort}.\n");
                _transporter.ShutDown();
            }
        }
    }
}
