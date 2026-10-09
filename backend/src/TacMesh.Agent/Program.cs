using System.Net;
using System.Net.Sockets;
using System.Text;
using TacMesh.Core.clocks;
using TacMesh.Core.communication;
using TacMesh.Core.communication.radio_communication;
using TacMesh.Core.communication.simulation_communication;
using TacMesh.Core.database_related;
using TacMesh.Core.graph_related;
using TacMesh.Core.interfaces;
using TacMesh.Core.interfaces.beacon___destination;
using TacMesh.Core.interfaces.communication_interfaces;
using TacMesh.Core.interfaces.raster;
using TacMesh.Core.map_related;
using TacMesh.Core.models;
using TacMesh.Core.serializers;
using TacMesh.Core.socket_related;
using TacMesh.Core.tables.node_related;
using TacMesh.Core.utils.configurations;

namespace TacMesh.Agent
{
    public class Program
    {
        // arguments length should be exactly 2: NodeID, scenario file path
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

                // arguments
                string nodeId = args[0];
                string scenarioFile = args[1];

                // load scenario into memory
                SystemConfigurations.LoadConfigurations(scenarioFile);                

                // --- create dependencies ---

                Socket radio_socket = GetRadioSocket();
                IReceiver radioReceiver = new SimReceiver(radio_socket);
                ILineOfSight terrainLoS = new TerrainLineOfSight();

                // TEMPORARILY inject max range as magic number for the test, WILL BE CHANGED!
                IRadioModel radioModel = new VirtualRadioModel(maximum_range: 100, radioReceiver, terrainLoS);

                Socket agent_socket = GetAgentSocket();
                ITransport simTransport = new SimTransport(agent_socket, radioModel);

                // start transmitting beacon to virtual radio model
                int assignedPort = ((IPEndPoint)agent_socket.LocalEndPoint).Port;
                EmitBeaconToRadioModel(nodeId, assignedPort);

                HeartBeater heartBeater = GetHeartBeater(nodeId, simTransport, radioModel);
                NeighbourTable neighbourTable = new NeighbourTable(new SystemClock());
                Location initial_location = SystemConfigurations.NodePositions[nodeId];

                // --- inject all dependencies --- 

                Node node = new Node(nodeId, initial_location, simTransport, neighbourTable, heartBeater);
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[AGENT ERROR] {e}");
            }
        }

        // TEMPORARILY EMIT ASSIGNED PORT BEACON TO RADIO MODEL HERE, THIS LOGIC WILL NOT STAY HERE FOREVER
        static void EmitBeaconToRadioModel(string nodeId, int assignedPort)
        {
            try
            {
                string positionReport = $"{nodeId}|{assignedPort}";
                byte[] beacon = Encoding.UTF8.GetBytes(positionReport);

                // open emitter and inject mcast group endpoint
                IBeaconEmitter emitter = new SimBeaconEmitter(SystemConfigurations.McastEndPoint);
                emitter.EmitBeacon(beacon);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[BEACON EMITTER ERROR]: '{e.Message}'");
            }                   
        }

        static Socket GetAgentSocket()
        {
            // create and bind socket to a random free port assigned by the OS and the localhoast ip
            Socket s = SocketGenerator.GenerateUdpSocket();
            SocketGenerator.BindSocket(s, IPAddress.Loopback);
            return s;
        }

        static Socket GetRadioSocket() 
            => SocketGenerator.GenerateMcastListenerSocket(SystemConfigurations.McastGroupIp, SystemConfigurations.McastPort);

        static HeartBeater GetHeartBeater(string nodeId, ITransport transport, IRadioModel radioModel) 
            => new HeartBeater(nodeId, new HeartbeatLogger(), new PacketHeaderSerializer(), transport, radioModel);




        static void ListenForStdInput()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    string input = Console.ReadLine();
                    Console.WriteLine($"[RECEIVED FROM StdINPUT STREAM]: '{input}'");
                }
            });
        }
    }
}
