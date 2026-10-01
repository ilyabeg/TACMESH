using System.Net;
using TacMesh.Core.communication;
using TacMesh.Core.configurations;
using TacMesh.Core.interfaces;
using TacMesh.Core.models;
using TacMesh.Core.packet_related;
using TacMesh.Core.serializers;

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
        private List<IPEndPoint> _neighbourNodes;
        private PacketBuffer _packetBuffer;


        // Constructors
        public Node(ITransport transporter, string nodeID)
        {
            // start receiving background thread
            _transporter = transporter;
            _transporter.StartReceiving();
            AssignedPort = _transporter.GetAssignedPort();
            NodeID = nodeID;


            // TEMPORARILY START HEARTBEATS HERE, WILL CHANGE THE PLACEMENT IN THE FUTURE
            // start transmitting heartbeats
            StartHeartbeat();


            // attach packet reading event
            _transporter.MessageReceivedEventHandler += (s,e) => PacketReader.ReadPacket(e);

            // TEMPORARY TEST. THIS WILL NOT STAY HERE FOREVER.
            Console.WriteLine($"~~~ PORT OPENED ON = {AssignedPort} for {NodeID} ~~~\n");            

            // attach Ctrl+C event handler
            Console.CancelKeyPress += (s,e) => ShutdownNode();
            //Console.ReadKey();
        }

        // ----------------------------------------------------

        // TEMPORARILY TEST HEARTBEATS HERE, WILL ALSO MAYBE THIS CHANGE PLACEMENT IN THE FUTURE
        private void StartHeartbeat()
        {
            // remove own node from known nodes dict before handing to the hearbeater
            SystemConfigurations.StaticNodes.Remove(NodeID);

            // start heartbeating process
            _heartbeater = new HeartBeater(
                NodeID,
                SystemConfigurations.StaticNodes,
                new PacketHeaderSerializer(),
                _transporter
            );
            _heartbeater.TransmitHeartbeat();
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
