using TacMesh.Core.assets.clocks;
using TacMesh.Core.assets.node;
using TacMesh.Core.assets.node.packet;
using TacMesh.Core.interfaces;
using TacMesh.Core.packet;
using TacMesh.Core.tables.node;

namespace TacMesh.Core.assets.graph
{
    public class LinkStateDispatcher
    {
        // dependencies
        private readonly string _sourceId; // the identifier of the source node
        private readonly NodeConfigs _nodeConfigs; // to access location
        private readonly PacketSender _packetSender; // to send the link state packets
        private readonly NeighborTable _neighborTable; // to listen for link state updates
        private readonly LamportClock _lamportClock; // to access current logical clock
        private readonly SenderCounter _senderCounter; // to attach the sender counter to the packets
        private readonly IByteSerializer<List<string>> _linkStateSerializer; // to serialize the link state
        private readonly NetworkTopology _networkTopology; // to update the own node's link state


        // constructor
        public LinkStateDispatcher(string source, NodeConfigs nodeConfigs, PacketSender packetSender, NeighborTable neighborTable, LamportClock lamportClock, SenderCounter senderCounter, IByteSerializer<List<string>> linkStateSerializer, NetworkTopology networkTopology)
        {
            _sourceId = source;
            _nodeConfigs = nodeConfigs;
            _packetSender = packetSender;
            _neighborTable = neighborTable;
            _lamportClock = lamportClock;
            _senderCounter = senderCounter;
            _linkStateSerializer = linkStateSerializer;
            _networkTopology = networkTopology;

            // attach link state update event handler
            _neighborTable.OnLinkStateChanged += (s,e) => DispatchLinkState(e);
        }

        /// <summary>
        /// Dispatches the link state of the node to every remote neighbor 
        /// which is currently in range
        /// </summary>
        /// <param name="linkState">The link state to dispatch</param>
        private void DispatchLinkState(List<string> linkState)
        {
            // add the own node's link state in the topology before dispatching to remote nodes
            //
            _networkTopology.UpdateLinkState(_sourceId, linkState);
            _networkTopology.UpdateNodeLocation(_sourceId, _nodeConfigs.Location);

            long logicalClockValue = _lamportClock.GetNext(); // inject the current clock value to each packet

            // build and dispatch the packet using the packet sender
            //
            DataPacket linkStatePacket = BuildLinkStatePacket(linkState, logicalClockValue);
            _packetSender.SendPacket(linkStatePacket);
        }

        private DataPacket BuildLinkStatePacket(List<string> linkState, long logicalClockValue)
        {
            // link state packets must contain the node's location aswell 
            // for remote nodes to set the location in their local graph
            //
            byte[] locationBytes = _nodeConfigs.Location.ToBytes();

            // serialize the link state to a byte array
            //
            byte[] linkStateBytes = _linkStateSerializer.Serialize(linkState);

            // concat the bytes to one payload using spread operator
            //
            byte[] packetPayload = [.. locationBytes, .. linkStateBytes];

            // init the packet header
            //
            PacketHeader packetHeader = new PacketHeader(
                msgType: PacketType.LinkState,
                srcID: _sourceId,
                dstID: PacketHeader.ALL_SPECIFIER,
                msgID: Guid.NewGuid(),
                logicalClock: logicalClockValue,
                senderCounter: _senderCounter.GetCurrent()
            );

            return new DataPacket(packetHeader, packetPayload);
        }
    }
}
