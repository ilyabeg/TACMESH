using System.Net;
using System.Text;
using TacMesh.Core.events;
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
        private VirtualRadioModel _radioModel;
        private List<IPEndPoint> _neighbourNodes;
        private PacketBuffer _packetBuffer;

        // Serializers
        private readonly PacketHeaderSerializer _header_serializer;


        // Constructors
        public Node(ITransport transporter, string nodeID)
        {
            _transporter = transporter;
            _transporter.StartReceiving();
            AssignedPort = _transporter.GetAssignedPort(); 

            NodeID = nodeID;
            // TEMPORARY TEST. THIS WILL NOT STAY HERE FOREVER.
            Console.WriteLine($"PORT={AssignedPort} on {NodeID}");
            Console.ReadKey();

            _header_serializer = new PacketHeaderSerializer();
            _radioModel = new VirtualRadioModel();
            _neighbourNodes = new List<IPEndPoint>();

            // attach Ctrl+C event handler
            Console.CancelKeyPress += (sender, e) => ShutdownNode();            
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

                Console.WriteLine($"Shutting down Node on PORT={AssignedPort}.");
                _transporter.ShutDown();
            }
        }

        // TMPORARY PRINTING METHOD, I AM AWARE THAT THIS IS LOGIC THAT GOES INTO THE CORE
        private void Print(MessageReceivedEventArgs e)
        {
            string received = Encoding.UTF8.GetString(e.MessageBytes);
            Console.WriteLine($"\nRecieved: '{received}' from PORT={e.RemoteEndPoint.Port}\n");
        }
    }
}
