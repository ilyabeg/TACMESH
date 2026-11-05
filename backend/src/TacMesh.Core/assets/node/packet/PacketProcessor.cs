using TacMesh.Core.assets.clocks;
using TacMesh.Core.assets.node.packet.validation;
using TacMesh.Core.database_related;
using TacMesh.Core.events;
using TacMesh.Core.hardware_mode;
using TacMesh.Core.packet;
using TacMesh.Core.packet_related;
using TacMesh.Core.tables.node_related;
using TacMesh.Core.utils.serializers;

namespace TacMesh.Core.assets.node.packet
{
    /// <summary>
    /// Packet processor to determine what to do and how to act upon receving a DataPacket.
    /// </summary>
    public class PacketProcessor
    {
        // dependencies
        private readonly TopologyFilter _topologyFilter;
        private readonly HeartBeater _heartbeater;
        private readonly NeighborTable _neighborTable;
        private readonly PacketValidator _packetValidator;


        // constructor
        public PacketProcessor(TopologyFilter topologyFilter, HeartBeater heartbeater, NeighborTable neighborTable, PacketValidator packetValidator)
        {
            _topologyFilter = topologyFilter;
            _heartbeater = heartbeater;
            _neighborTable = neighborTable;
            _packetValidator = packetValidator;
        }

        /// <summary>
        /// Processes the received DataPacket accordingly drops it if it is not authorized
        /// </summary>
        /// <param name="e">Message Received Event arguments</param>
        public void ProcessPacket(MessageReceivedEventArgs e)
        {
            // interprate the packet
            DataPacket? packet = ReadPacket(e);

            // validate packet before processing
            //
            if (packet != null && _packetValidator.Validate(packet))
            {
                // drop packet if it is from an un-permitted neighbor before processing
                //
                if (!_topologyFilter.Filter(packet.PacketHeader.SrcID))
                {
                    Console.WriteLine($"[Packet Processor] Dropping Packet from source \"{packet.PacketHeader.SrcID}\" due to: un-permitted neighbor");
                    return;
                }

                // process packet by type ...
                // 
                if (ProcessHeartbeat(packet, e)) return;
                if (ProcessUserMessage(packet, e)) return;
                if (ProcessLinkState(packet, e)) return;
            }
            // if packet invalid, it is dropped...
        }

        /// <summary>
        /// Reads the received message bytes and tries to parse it as a DataPacket
        /// </summary>
        /// <param name="e">Message Received Event Args containing the received bytes</param>
        /// <returns>a DataPacket object if deserialized successfuly or null otherwise.</returns>
        public static DataPacket? ReadPacket(MessageReceivedEventArgs e)
        {
            try
            {
                // deserialize pacjet bytes
                DataPacket packet = PacketSerializer.Deserialize(e.MessageBytes);
                return packet;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Packet Processor] Couldn't interprate the DataPacket due to: \"{ex.Message}\"");
                return null;
            }
        }


        // --- Processing the different Packet Types ---

        /// <summary>
        /// tries to process the packet as a heartbeat packet
        /// </summary>
        /// <returns>true if successfuly processed as a heartbeat packet</returns>
        private bool ProcessHeartbeat(DataPacket packet, MessageReceivedEventArgs e)
        {
            if (packet.PacketHeader.MsgType == PacketType.Heartbeat && _packetValidator.ValidateLogicalClock(packet))
            {
                // log heartbeat
                _heartbeater.LogHeartbeat(LoggingMode.Received, packet.PacketHeader.SrcID, packet.PacketHeader.DstID);

                // add/update record of peer in neighbour table
                _neighborTable.UpdateRecord(packet.PacketHeader.SrcID, e.RemoteEndPoint);
                return true;
            }
            return false;
        }

        /// <summary>
        /// tries to process the packet as a link state packet
        /// </summary>
        /// <returns>true if successfuly processed as a link state packet</returns>
        private bool ProcessLinkState(DataPacket packet, MessageReceivedEventArgs e)
        {
            if (packet.PacketHeader.MsgType == PacketType.LinkState && _packetValidator.ValidateLogicalClock(packet))
            {
                // To Do ...


                // TEMP TEST
                Console.WriteLine($"[PACKET PROCESSOR] Link state packet from source {packet.PacketHeader.SrcID} was valid. Processing now ...");


                return true;
            }
            return false;
        }

        /// <summary>
        /// tries to process the packet as a user message packet
        /// </summary>
        /// <returns>true if successfuly processed as a user message packet</returns>
        private bool ProcessUserMessage(DataPacket packet, MessageReceivedEventArgs e)
        {
            if (packet.PacketHeader.MsgType == PacketType.UserMessage)
            {
                // TEST print out packet
                Console.WriteLine($"[PACKET PROCESSOR] Received User Message: \n{packet}");

                // if the packet isn't intended for this node, invoke OnForward event to let the
                // PacketSender handle it
                //
                // SELF NOTE: what happens if a Link State ot Heartbeat not inteded for THIS node gets processed
                //
                
                // To do ...

                return true;
            }
            return false;
        }
    }
}
