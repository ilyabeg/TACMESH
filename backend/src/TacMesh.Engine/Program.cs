using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using TacMesh.Core.assets;
using TacMesh.Core.assets.clocks;
using TacMesh.Core.assets.graph;
using TacMesh.Core.assets.node;
using TacMesh.Core.assets.node.packet;
using TacMesh.Core.assets.node.packet.buffer;
using TacMesh.Core.assets.node.packet.validation;
using TacMesh.Core.assets.routing.epidemic;
using TacMesh.Core.assets.routing.prophet;
using TacMesh.Core.communication.radio_communication;
using TacMesh.Core.communication.simulation_communication;
using TacMesh.Core.database_related;
using TacMesh.Core.events.dispatchers;
using TacMesh.Core.hardware_mode;
using TacMesh.Core.interfaces;
using TacMesh.Core.interfaces.beacon_and_destination;
using TacMesh.Core.interfaces.clock_interfaces;
using TacMesh.Core.interfaces.communication;
using TacMesh.Core.interfaces.communication_interfaces;
using TacMesh.Core.interfaces.raster;
using TacMesh.Core.interfaces.routing;
using TacMesh.Core.launcher;
using TacMesh.Core.map_related;
using TacMesh.Core.models;
using TacMesh.Core.packet;
using TacMesh.Core.tables.node;
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
                    Console.WriteLine("ERROR: Engine Process arguments did not match. Terminating process...\n");
                    return;
                }

                // set the output encoding to UTF-8 encoding
                Console.OutputEncoding = System.Text.Encoding.UTF8;

                // --- arguments ---

                // save node info
                string nodeInfoJson = args[0];
                NodeInfo nodeInfo = JsonSerializer.Deserialize<NodeInfo>(nodeInfoJson)!;

                // save node config settings to access the engine's specific node settings anywhere               
                string configsJson = args[1];
                NodeConfigs nodeConfigs = JsonSerializer.Deserialize<NodeConfigs>(configsJson)!;


                // --- create all dependencies ---

                // Engine socket & port
                Socket engine_socket = GetEngineSocket();
                int assignedPort = ((IPEndPoint)engine_socket.LocalEndPoint!).Port;

                // Beacon Emitter
                IBeaconEmitter beaconEmitter = new SimBeaconEmitter(nodeInfo.NodeId, assignedPort, SysConfigs.McastEndPoint);

                // RadioModel
                Socket radio_socket = GetRadioSocket();
                IReceiver radioReceiver = new SimReceiver(radio_socket);
                ILineOfSight terrainLoS = new TerrainLineOfSight();
                DRandom dRandom = new DRandom();
                IRadioModel radioModel = new VirtualRadioModel(nodeConfigs, dRandom, radioReceiver, terrainLoS);

                // Transport
                ITransport simTransport = new SimTransport(engine_socket, radioModel);

                // Clock and Lamport Logical Clock
                IClock systemClock = new SystemClock();
                LamportClock lamportClock = new LamportClock();

                // Seralizers
                IByteSerializer<DataPacket> packetSerializer = new PacketSerializer();
                IByteSerializer<List<string>> linkStateSerializer = new LinkStateSerializer();
                IByteSerializer<ConcurrentDictionary<string, double>> prophetSerializer = new ProphetExchangeSerializer();

                // Sender Counter state
                SenderCounter senderCounter = new SenderCounter();

                // Heartbeater & NeighbourTable
                HeartBeater heartbeater = new HeartBeater(nodeInfo.NodeId, nodeConfigs, new HeartbeatLogger(), simTransport, radioModel, lamportClock, packetSerializer, senderCounter);
                NeighborTable neighborTable = new NeighborTable(systemClock);

                // Packet Buffer, Packet Sender
                PacketBuffer packetBuffer = new PacketBuffer(systemClock);
                PacketSender packetSender = new PacketSender(packetSerializer, neighborTable, simTransport);

                // Routing Exchange Handlers
                ProphetExchangeHandler prophetExchangeHandler = new ProphetExchangeHandler(prophetSerializer, packetSender, lamportClock, senderCounter, nodeInfo.NodeId);
                EpidemicExchangeHandler epidemicExchangeHandler = new EpidemicExchangeHandler(packetBuffer, packetSender, lamportClock, senderCounter, nodeInfo.NodeId);

                // Routing Algorithm
                //IRouting routing = new EpidemicRouting(neighborTable, packetBuffer, epidemicExchangeHandler);
                IRouting routing = new ProphetRouting(nodeInfo.NodeId, neighborTable, prophetExchangeHandler);

                // Topology Filter & Network Topology
                TopologyFilter topologyFilter = new TopologyFilter(nodeConfigs);
                NetworkTopology networkTopology = new NetworkTopology();

                // Packet Validator, Packet Dispatcher, Packet Processor
                PacketValidator packetValidator = new PacketValidator(lamportClock);
                PacketDispatcher packetDispatcher = new PacketDispatcher(routing, packetSender, packetBuffer);
                PacketProcessor packetProcessor = new PacketProcessor(packetSerializer, linkStateSerializer, topologyFilter, heartbeater, neighborTable, packetValidator, packetDispatcher, packetSender, networkTopology, routing, nodeInfo.NodeId);

                // Link State & Buffer Dispatchers
                LinkStateDispatcher linkStateDispatcher = new LinkStateDispatcher(nodeInfo.NodeId, nodeConfigs, packetSender, neighborTable, lamportClock, senderCounter, linkStateSerializer, networkTopology);
                BufferDispatcher bufferDispatcher = new BufferDispatcher(neighborTable, packetSender, packetBuffer, routing);

                // Input Listener
                IInputListener inputControl = new LauncherControl(nodeInfo.NodeId, nodeConfigs, beaconEmitter);
                //IInputListener inputControl = new UserControl(packetSender, nodeConfigs, beaconEmitter, lamportClock);                


                // --- inject all dependencies --- 
                Node node = new Node(nodeInfo, nodeConfigs, simTransport, neighborTable, heartbeater, packetProcessor, inputControl);


                // Release the Semaphore to allow the next engine to be initialized in the launcher
                string semName = "TacMesh-Global-Sem";

                if (Semaphore.TryOpenExisting(semName, out Semaphore? semaphore))
                {
                    using (semaphore)
                    {
                        // un-freeze the launcher
                        semaphore.Release();
                    }
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
    }
}
