using System.Text.Json;
using TacMesh.Core.assets.node;
using TacMesh.Core.assets.node.packet;
using TacMesh.Core.interfaces.beacon_and_destination;
using TacMesh.Core.interfaces.communication;
using TacMesh.Core.packet;
using TacMesh.Core.packet_related;

namespace TacMesh.Core.hardware_mode.communication
{
    /// <summary>
    /// Input listener class to control the IPC messages received from the User
    /// at simulation mode
    /// </summary>
    public class UserControl : IInputListener
    {        
        // dependencies
        private readonly PacketSender _packetSender;

        // maybe temporary
        private readonly NodeConfigs _nodeConfigs;
        private readonly IBeaconEmitter _beaconEmitter;


        // Constructor 
        public UserControl(PacketSender packetSender, NodeConfigs nodeConfigs, IBeaconEmitter beaconEmitter) 
        {
            _packetSender = packetSender;
            _nodeConfigs = nodeConfigs;
            _beaconEmitter = beaconEmitter;
        }


        public void ListenForInput()
        {
            // create a dedicated thread for listening for input instead of using a thread from 
            // the thread pool via a Task, because a Console.ReadLine() is a synchronous operation
            // and will block the thread from working in the meantime of waiting for input
            //
            Thread listener = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        // parse input pushed from the user frontend
                        string input = Console.ReadLine();

                        if (input.Equals("."))
                        {
                            // emit a beacon to all radio models on the net
                            _beaconEmitter.EmitBeacon(_nodeConfigs.Location);
                        }

                        TryParseStdInput(input);

                        // test received input
                        //Console.WriteLine($"[USER CONTROL] received: \"{input}\"");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"[USER CONTROL] Error: \"{e.Message}\"\n");
                    }
                }
            });
            listener.IsBackground = true; // set as bg thread to make it die when the main thread dies
            listener.Start();
        }

        // --- parsing methods ---

        private void TryParseStdInput(string input)
        {
            if (TryParseUserMessage(input)) return;
        }

        // try to parse as a user message json
        private bool TryParseUserMessage(string input)
        {
            try
            {
                UserMessage? userMessage = JsonSerializer.Deserialize<UserMessage>(input) ?? throw new Exception();

                // build and send the packet to the destination
                //
                DataPacket packet = CreateSourcePacket(userMessage);
                _packetSender.SendPacket(packet);
                return true;
            }
            catch { return false; }
        }

        private DataPacket CreateSourcePacket(UserMessage userMessage)
        {
            return new DataPacket(
                PacketHeader.InitHeader(PacketType.UserMessage, userMessage.Source, userMessage.Destination), 
                userMessage.Message
            );
        }
    }
}
