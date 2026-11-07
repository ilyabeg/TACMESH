using TacMesh.Core.assets.graph;
using TacMesh.Core.assets.node.packet.validation;
using TacMesh.Core.database_related;
using TacMesh.Core.events;
using TacMesh.Core.hardware_mode;
using TacMesh.Core.interfaces;
using TacMesh.Core.packet;
using TacMesh.Core.tables.node;

namespace TacMesh.Core.assets.node.packet
{
    /// <summary>
    /// Packet processor to determine what to do and how to act upon receving a DataPacket.
    /// </summary>
    public class PacketProcessor
    {
        // dependencies
        private readonly IByteSerializer<DataPacket> _packetSerializer;
        private readonly IByteSerializer<List<string>> _linkStateSerializer;
        private readonly TopologyFilter _topologyFilter;
        private readonly HeartBeater _heartbeater;
        private readonly NeighborTable _neighborTable;
        private readonly PacketValidator _packetValidator;
        private readonly PacketSender _packetSender;
        private readonly NetworkTopology _networkTopology;
        private readonly string _sourceId;


        // constructor
        public PacketProcessor(IByteSerializer<DataPacket> packetSerializer, IByteSerializer<List<string>> linkStateSerializer, TopologyFilter topologyFilter, HeartBeater heartbeater, NeighborTable neighborTable, PacketValidator packetValidator, PacketSender packetSender, NetworkTopology networkTopology, string sourceId)
        {            
            _packetSerializer = packetSerializer;
            _linkStateSerializer = linkStateSerializer;
            _topologyFilter = topologyFilter;
            _heartbeater = heartbeater;
            _neighborTable = neighborTable;
            _packetValidator = packetValidator;
            _packetSender = packetSender;
            _networkTopology = networkTopology;
            _sourceId = sourceId;
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
        private DataPacket? ReadPacket(MessageReceivedEventArgs e)
        {
            try
            {
                // deserialize pacjet bytes
                DataPacket packet = _packetSerializer.Deserialize(e.MessageBytes);
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

                // pull the node info from the heartbeat payload
                //
                string nodeId = packet.PacketHeader.SrcID;
                Location nodeLocation = Location.FromBytes(packet.Payload);

                // add/update record of peer in neighbour table
                _neighborTable.UpdateRecord(nodeId, nodeLocation, e.RemoteEndPoint);

                // update specific node's location in the net topology
                _networkTopology.UpdateNodeLocation(nodeId, nodeLocation);

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
                // deserialize the location before the link state (first 16 bytes)
                //
                byte[] locationBytes = packet.Payload.AsSpan().Slice(start: 0, ByteCount._16bytes).ToArray();
                Location nodeLocation = Location.FromBytes(locationBytes);

                // deserialize the link state after the location in the payload
                //
                byte[] linkStateBytes = packet.Payload.AsSpan().Slice(start: ByteCount._16bytes).ToArray();
                List<string> linkState = _linkStateSerializer.Deserialize(linkStateBytes);
                
                string nodeId = packet.PacketHeader.SrcID; // the node identifier

                // update the node's link state and location in the net topology
                //
                _networkTopology.UpdateLinkState(nodeId, linkState);
                _networkTopology.UpdateNodeLocation(nodeId, nodeLocation);

                // increment the hop count, retransmit the packet to spread the link state,
                // and exclude the neighbor which this message was sent from
                //
                packet.PacketHeader.IncrementHopCount();
                _packetSender.SendPacket(packet, exluciveAddress: e.RemoteEndPoint);

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

                // if the packet isn't intended for this node, let the PacketSender handle it
                //
                // SELF NOTE: what happens if a Link State ot Heartbeat not inteded for THIS node gets processed
                //
                if (!packet.PacketHeader.DstID.Equals(_sourceId))
                {
                    // To do ...
                }

                return true;
            }
            return false;
        }
    }
}
