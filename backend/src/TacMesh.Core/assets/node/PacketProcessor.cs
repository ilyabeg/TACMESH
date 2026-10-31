using TacMesh.Core.database_related;
using TacMesh.Core.events;
using TacMesh.Core.hardware_mode;
using TacMesh.Core.tables.node_related;
using TacMesh.Core.utils.packet;

namespace TacMesh.Core.assets.node
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

        // constructor
        public PacketProcessor(TopologyFilter topologyFilter, HeartBeater heartbeater, NeighborTable neighborTable)
        {
            _topologyFilter = topologyFilter;
            _heartbeater = heartbeater;
            _neighborTable = neighborTable;
        }

        /// <summary>
        /// Processes the received DataPacket accordingly drops it if it is not authorized
        /// </summary>
        /// <param name="e"></param>
        public void ProcessPacket(MessageReceivedEventArgs e)
        {
            // understand the packet
            PacketHeader header = PacketReader.ReadPacket(e);

            // drop packet if it is from an un-permitted neighbor before processing
            if (!_topologyFilter.Filter(header.SrcID))
            {
                Console.WriteLine($"[Packet Processor] Dropping Packet from source \"{header.SrcID}\" due to: un-permitted neighbor");
                return;
            }

            // process packet ...            
            if (header.MsgType == PacketType.Heartbeat)
            {
                // log heartbeat
                _heartbeater.LogHeartbeat(LoggingMode.Received, header.SrcID, header.DstID);

                // add/update record of peer in neighbour table
                _neighborTable.UpdateRecord(header.SrcID, e.RemoteEndPoint);
            }
        }
    }
}
