using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using TacMesh.Core.assets;
using TacMesh.Core.assets.clocks;
using TacMesh.Core.assets.node;
using TacMesh.Core.communication.radio_communication;
using TacMesh.Core.communication.simulation_communication;
using TacMesh.Core.database_related;
using TacMesh.Core.graph_related;
using TacMesh.Core.hardware_mode;
using TacMesh.Core.interfaces;
using TacMesh.Core.interfaces.beacon_and_destination;
using TacMesh.Core.interfaces.communication_interfaces;
using TacMesh.Core.interfaces.raster;
using TacMesh.Core.launcher;
using TacMesh.Core.map_related;
using TacMesh.Core.models;
using TacMesh.Core.tables.node_related;
using TacMesh.Core.utils;
using TacMesh.Core.utils.serializers;

namespace TacMesh.Agent
{
    public class Program
    {
        // arguments count
        const int arguments_length = 4;

        static void Main(string[] args)
        {
            try
            {
                if (args.Length != arguments_length)
                {
                    Console.WriteLine("ERROR: Agent Process arguments did not match. Terminating process...");
                    return;
                }

                // --- arguments ---

                string nodeId = args[0];
                Location initialLocation = JsonSerializer.Deserialize<Location>(args[1])!;
                //Console.WriteLine($"{nodeId} initial location: {initialLocation}");
                double initialSpeed = double.Parse(args[2]);
                List<string> permittedNodes = JsonSerializer.Deserialize<List<string>>(args[3])!;

                // --- create all dependencies ---

                // Agent socket & port
                Socket agent_socket = GetAgentSocket();
                int assignedPort = ((IPEndPoint)agent_socket.LocalEndPoint).Port;

                // IRadioModel
                Socket radio_socket = GetRadioSocket();
                IReceiver radioReceiver = new SimReceiver(radio_socket);
                ILineOfSight terrainLoS = new TerrainLineOfSight();
                DRandom dRandom = new DRandom();

                // TEMPORARILY inject max range as magic number for the test, WILL BE CHANGED!
                IRadioModel radioModel = new VirtualRadioModel(maximum_range: 10, dRandom, radioReceiver, terrainLoS);

                // ITransport
                ITransport simTransport = new SimTransport(agent_socket, radioModel);

                // Heartbeats, NeighbourTable
                HeartBeater heartBeater = GetHeartBeater(nodeId, simTransport, radioModel);
                NeighborTable neighbourTable = new NeighborTable(new SystemClock());

                // Beacon Emitter
                IBeaconEmitter beaconEmitter = new SimBeaconEmitter(nodeId, assignedPort, SystemConfigs.McastEndPoint);
                LauncherControl launcherControl = new LauncherControl((VirtualRadioModel)radioModel, nodeId, initialLocation, initialSpeed, beaconEmitter);

                // Topology Filter
                TopologyFilter topologyFilter = new TopologyFilter(permittedNodes); // <- inject permitted


                // --- inject all dependencies --- 

                Node node = new Node(nodeId, initialLocation, simTransport, neighbourTable, heartBeater, topologyFilter);


                // listen for launcher stdinput
                launcherControl.ListenForStdInput();


                // Release the Semaphore to allow the next agent to be initialized in the launcher
                string semName = "TacMesh-Global-Sem";
                using (Semaphore semaphore = Semaphore.OpenExisting(semName))
                {
                    // un-freeze the launcher
                    semaphore.Release();
                }

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[AGENT ERROR] {e}");
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
            => SocketGenerator.GenerateMcastListenerSocket(SystemConfigs.McastGroupIp, SystemConfigs.McastPort);

        static HeartBeater GetHeartBeater(string nodeId, ITransport transport, IRadioModel radioModel) 
            => new HeartBeater(nodeId, new HeartbeatLogger(), new PacketHeaderSerializer(), transport, radioModel);
    }
}
