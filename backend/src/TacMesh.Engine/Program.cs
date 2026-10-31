using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using TacMesh.Core.assets;
using TacMesh.Core.assets.clocks;
using TacMesh.Core.assets.node;
using TacMesh.Core.communication.radio_communication;
using TacMesh.Core.communication.simulation_communication;
using TacMesh.Core.database_related;
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

namespace TacMesh.Engine
{
    public class Program
    {
        // arguments count
        const int arguments_length = 2;

        static void Main(string[] args)
        {
            try
            {
                if (args.Length != arguments_length)
                {
                    Console.WriteLine("ERROR: Engine Process arguments did not match. Terminating process...");
                    return;
                }
                // set the output encoding to UTF-8 encoding
                Console.OutputEncoding = System.Text.Encoding.UTF8;

                // --- arguments ---

                // save node info
                string nodeInfoJson = args[0];
                NodeInfo? nodeInfo = DeserializeNodeInfo(nodeInfoJson);
                if (nodeInfo == null) return;

                // save node config settings to access the engine's specific node settings anywhere               
                string configsJson = args[1];
                NodeConfigs? nodeConfigs = DeserializeConfigSettings(configsJson);
                if (nodeConfigs == null) return;


                // --- create all dependencies ---

                // Engine socket & port
                Socket engine_socket = GetEngineSocket();
                int assignedPort = ((IPEndPoint)engine_socket.LocalEndPoint).Port;

                // IRadioModel
                Socket radio_socket = GetRadioSocket();
                IReceiver radioReceiver = new SimReceiver(radio_socket);
                ILineOfSight terrainLoS = new TerrainLineOfSight();
                DRandom dRandom = new DRandom();

                // TEMPORARILY inject max range as magic number for the test, WILL BE CHANGED!
                IRadioModel radioModel = new VirtualRadioModel(nodeConfigs, dRandom, radioReceiver, terrainLoS);

                // ITransport
                ITransport simTransport = new SimTransport(engine_socket, radioModel);

                // Heartbeats, NeighbourTable
                HeartBeater heartBeater = GetHeartBeater(nodeInfo.NodeId, simTransport, radioModel);
                NeighborTable neighbourTable = new NeighborTable(new SystemClock());

                // Beacon Emitter
                IBeaconEmitter beaconEmitter = new SimBeaconEmitter(nodeInfo.NodeId, assignedPort, SysConfigs.McastEndPoint);
                LauncherControl launcherControl = new LauncherControl(nodeInfo.NodeId, nodeConfigs, beaconEmitter);

                // Topology Filter
                TopologyFilter topologyFilter = new TopologyFilter(nodeConfigs);


                // --- inject all dependencies --- 

                Node node = new Node(nodeInfo, nodeConfigs, simTransport, neighbourTable, heartBeater, topologyFilter);


                // listen for launcher stdinput
                launcherControl.ListenForStdInput();


                // Release the Semaphore to allow the next engine to be initialized in the launcher
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
                Console.WriteLine($"[ENGINE ERROR] {e}");
            }
        }

        static Socket GetEngineSocket()
        {
            // create and bind socket to a random free port assigned by the OS and the localhoast ip
            Socket s = SocketGenerator.GenerateUdpSocket();
            SocketGenerator.BindSocket(s, IPAddress.Loopback);
            return s;
        }

        static Socket GetRadioSocket() 
            => SocketGenerator.GenerateMcastListenerSocket(SysConfigs.McastGroupIp, SysConfigs.McastPort);

        static HeartBeater GetHeartBeater(string nodeId, ITransport transport, IRadioModel radioModel) 
            => new HeartBeater(nodeId, new HeartbeatLogger(), new PacketHeaderSerializer(), transport, radioModel);

        static NodeInfo? DeserializeNodeInfo(string json)
        {
            try {
                return JsonSerializer.Deserialize<NodeInfo>(json);
            }
            catch (Exception e) {
                Console.WriteLine($"[SYS CONFIGS] Error deserializing the node info: \"{e.Message}\"");
                return null;
            }
        }

        static NodeConfigs? DeserializeConfigSettings(string json)
        {
            try {
                return JsonSerializer.Deserialize<NodeConfigs>(json);
            }
            catch (Exception e) {
                Console.WriteLine($"[SYS CONFIGS] Error deserializing the node configs: \"{e.Message}\"");
                return null;
            }
        }
    }
}
