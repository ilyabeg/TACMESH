using System.Net;
using System.Net.Sockets;
using TacMesh.Core.clocks;
using TacMesh.Core.communication.simulation_communication;
using TacMesh.Core.configurations;
using TacMesh.Core.database_related;
using TacMesh.Core.socket_related;
using TacMesh.Core.tables.node_related;

namespace TacMesh.Agent
{
    public class Program
    {
        // arguments length should be exactly 2: NodeID, config file path
        const int arguments_length = 2;

        static void Main(string[] args)
        {
            try
            {
                if (args.Length != arguments_length)
                {
                    Console.WriteLine("ERROR: Agent Process arguments did not match. Terminating process...");
                    return;
                }


                // TEMPORARY TEST
                //Console.WriteLine("Enter node id:");            


                // the arguments
                string nodeId = args[0];
                string configFile = args[1]; // @"..\..\..\config.json"

                // load configurations before starting node process
                SystemConfigurations.LoadConfigurations(configFile, nodeId);

                // TEMPORARILY SET EACH NODES STATIC PORT NUMBER
                int portNum = SystemConfigurations.StaticNodes[nodeId].Port; // .Port because it is IPEndPoint

                // create all dependencies
                Socket socket = GetSocket(portNum);
                SimTransport simTransport = GetSimTransport(socket);
                NeighbourTable neighbourTable = GetNeighbourTable();
                HeartbeatLogger logger = new HeartbeatLogger();

                Node node = new Node(nodeId, simTransport, neighbourTable, logger);
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[AGENT ERROR] {e}");
            }
        }

        static Socket GetSocket(int port)
        {
            // create and bind socket to a random free port assigned by the OS and the localhoast ip
            Socket s = SocketGenerator.GenerateUdpSocket();
            SocketGenerator.BindSocket(s, IPAddress.Loopback, port);
            return s;
        }
        static SimTransport GetSimTransport(Socket s) => new SimTransport(s);
        static NeighbourTable GetNeighbourTable() => new NeighbourTable(new SystemClock());
    }
}
