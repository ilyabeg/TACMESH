using System.Net;
using System.Net.Sockets;
using TacMesh.Core.assets;
using TacMesh.Core.assets.clocks;
using TacMesh.Core.communication.radio_communication;
using TacMesh.Core.communication.simulation_communication;
using TacMesh.Core.database_related;
using TacMesh.Core.graph_related;
using TacMesh.Core.interfaces;
using TacMesh.Core.interfaces.beacon___destination;
using TacMesh.Core.interfaces.communication_interfaces;
using TacMesh.Core.interfaces.raster;
using TacMesh.Core.launcher;
using TacMesh.Core.map_related;
using TacMesh.Core.models;
using TacMesh.Core.serializers;
using TacMesh.Core.tables.node_related;
using TacMesh.Core.utils;

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

                // --- arguments ---
                string nodeId = args[0];
                string scenarioFile = args[1];

                // load scenario into memory
                SystemConfigs.LoadConfigurations(scenarioFile);

                // --- create dependencies ---

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
                NeighbourTable neighbourTable = new NeighbourTable(new SystemClock());
                Location initial_location = SystemConfigs.NodePositions[nodeId];

                // beacon emitter
                IBeaconEmitter beaconEmitter = new SimBeaconEmitter(nodeId, assignedPort, SystemConfigs.McastEndPoint);
                beaconEmitter.EmitBeacon();

                // --- inject all dependencies --- 

                Node node = new Node(nodeId, initial_location, simTransport, neighbourTable, heartBeater);

                // listen for launcher stdinput
                LauncherControl launcherControl = new LauncherControl((VirtualRadioModel)radioModel, nodeId);
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
