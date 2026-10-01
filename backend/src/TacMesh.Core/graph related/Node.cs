using System.Net;
using System.Text;
using TacMesh.Core.events;
using TacMesh.Core.interfaces;
using TacMesh.Core.models;
using TacMesh.Core.packet_related;
using TacMesh.Core.serializers;

namespace TacMesh.Core
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

            _transporter.MessageReceivedEventHandler += (sender, e) => OnMsgReceived(e);

            NodeID = nodeID;
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
        /// Shuts down the User and Socket gracefully. Executing Socket.Close() internally executes
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

                Console.WriteLine($"Shutting down Node on PORT={AssignedPort}.");
                _transporter.ShutDown();
            }
        }

        // TESTING SECTION ONLY. IGNORE THE FACT THAT THIS IS HORRIBLE WRITTING, IT IS STRICTLY A TEST
        private int test_role;
        private int count;
        public void Test(int role)
        {
            count = 0;

            // 0 - מאזין
            if (role == 0)
            {
                Console.WriteLine($"PORT={AssignedPort}\n");
                test_role = role;
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("Enter port: ");
                int port = int.Parse(Console.ReadLine());
                Console.WriteLine("Enter message: ");
                string msg = Console.ReadLine();

                byte[] bytes = Encoding.UTF8.GetBytes(msg);

                IPEndPoint ep = new IPEndPoint(IPAddress.Loopback, port);
                _transporter.Transmit(bytes, ep);
                test_role = role;

                Console.ReadKey();
            }
        }
        private void OnMsgReceived(MessageReceivedEventArgs e)
        {
            if (test_role == 0)
            {
                // first message, echo back
                if (count == 0)
                {
                    // echo the message back to the sender
                    _transporter.Transmit(e.MessageBytes, e.RemoteEndPoint);
                    count++;
                }
                else
                    // if the second time getting message, only print out
                    Print(e);
            }
            else
            {
                // if the second time getting message, only print out without asking to write new message
                if (count == 1)
                    Print(e);
                else
                {
                    // first time getting message, print out
                    Print(e);

                    Console.WriteLine("Enter port: ");
                    int port = int.Parse(Console.ReadLine());
                    Console.WriteLine("Enter message: ");
                    string msg = Console.ReadLine();

                    byte[] bytes = Encoding.UTF8.GetBytes(msg);

                    IPEndPoint ep = new IPEndPoint(IPAddress.Loopback, port);
                    _transporter.Transmit(bytes, ep);
                    count++;
                }
            }
        }

        private void Print(MessageReceivedEventArgs e)
        {
            string received = Encoding.UTF8.GetString(e.MessageBytes);
            Console.WriteLine($"\nRecieved: '{received}' from PORT={e.RemoteEndPoint.Port}\n");
        }
    }
}
