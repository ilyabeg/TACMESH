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
                // TEMPORARILY inject max range as magic number for the test, WILL BE CHANGED!
                VirtualRadioModel radioModel = new VirtualRadioModel(maximum_range: 100, radioReceiver);

                Socket agent_socket = GetAgentSocket();
                ITransport simTransport = new SimTransport(agent_socket, radioModel);

                // start transmitting beacon to virtual radio model
                int assignedPort = ((IPEndPoint)agent_socket.LocalEndPoint).Port;
                StartEmittingBeacon(nodeId, assignedPort);

                HeartBeater heartBeater = GetHeartBeater(nodeId, simTransport, radioModel);
                NeighbourTable neighbourTable = new NeighbourTable(new SystemClock());

                // --- inject all dependencies --- 

                Node node = new Node(nodeId, simTransport, neighbourTable, heartBeater);
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[AGENT ERROR] {e}");
            }
        }

        // TEMPORARILY START TRANSMITING THE MOCK POSITIONS FROM THE SCENARIO FILE HERE,
        // THIS IS NOT INTENDED TO STAY HERE, I KNOW IT IS INCORRECT TO ADD THIS LOGIC 
        // TO THE AGENT PROJECT, BUT FOR TEMPORARY TESTING REASONS I TEST THIS HERE.
        static void StartEmittingBeacon(string nodeId, int assignedPort)
        {
            Task.Run(async () =>
            {
                try
                {
                    // THE POSITION IS CURRENTLY STATIC. IT WILL CHANGE IN THE FUTURE AND 
                    // WILL NOT BE SENT LIKE THIS. THE LOCATIONS WILL BE SENT VIA THE HEARTBEAT PACKET
                    // PAYLOAD BUT BECAUSE I CURRENTLY DON'T HAVE A FULL DATAPACKET OBJECT I SEND THE
                    // LOCATIONS TO THE RADIO MODEL AND CHECK THE RANGE AND ALL THE OTHER TESTS USING
                    // THIS TEMPORARILY STATIC LOCATION FOR EVERY NODE.
                    GraphPoint position = SystemConfigurations.NodePositions[nodeId];
                    string positionReport = $"{nodeId}|{assignedPort}|{position.X}|{position.Y}";
                    byte[] beacon = Encoding.UTF8.GetBytes(positionReport);

                    // open emitter and inject mcast group endpoint
                    IBeaconEmitter emitter = new SimBeaconEmitter(SystemConfigurations.McastEndPoint);

                    while (true)
                    {
                        emitter.EmitBeacon(beacon);

                        // TEMPORARILY MATCH DELAY TO HEARTBEAT DELAY
                        await Task.Delay(SystemConfigurations.HeartbeatDelayMs);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
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

        static HeartBeater GetHeartBeater(string nodeId, ITransport transport, IDestinationProvider provider) 
            => new HeartBeater(nodeId, new HeartbeatLogger(), new PacketHeaderSerializer(), transport, provider);
    }
}
