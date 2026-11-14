using TacMesh.Core.assets.clocks;
using TacMesh.Core.assets.node.packet;
using TacMesh.Core.assets.node.packet.buffer;
using TacMesh.Core.interfaces;
using TacMesh.Core.packet;

namespace TacMesh.Core.assets.routing.epidemic
{
    /// <summary>
    /// Handler class to handle Epidemic rouing protocol routing exchange messages
    /// </summary>
    public class EpidemicExchangeHandler
    {
        // dependencies
        private readonly IByteSerializer<List<Guid>> _serializer;
        private readonly PacketBuffer _packetBuffer;
        private readonly PacketSender _packetSender;
        private readonly LamportClock _lamportClock;
        private readonly SenderCounter _senderCounter;
        private readonly string _sourceNode;

        // constructor
        public EpidemicExchangeHandler(IByteSerializer<List<Guid>> serializer, PacketBuffer packetBuffer, PacketSender packetSender, LamportClock lamportClock, SenderCounter senderCounter, string sourceNode)
        {
            _serializer = serializer;
            _packetBuffer = packetBuffer;
            _packetSender = packetSender;
            _lamportClock = lamportClock;
            _senderCounter = senderCounter;
            _senderCounter = senderCounter;
            _sourceNode = sourceNode;
        }

        // --- Summary Vector dispatch logic ---

        /// <summary>
        /// Dispatches the node's current summary vector to the specific neighbor
        /// </summary>
        /// <param name="neighbor">The neighbor to send the sum vector to</param>
        public void DispatchSumVector(string neighbor)
        {
            // serialize the current summary vector
            //
            List<Guid> currPacketIdentifiers = _packetBuffer.GetCurrentPackets();
            byte[] currVector = _serializer.Serialize(currPacketIdentifiers);

            // get the sum vector packet and send to the neighbor
            //
            DataPacket vectorExchange = BuildSumVectorPacket(neighbor, currVector);
            _packetSender.SendPacket(vectorExchange);
        }

        private DataPacket BuildSumVectorPacket(string destination, byte[] payload)
        {
            // build the packet header
            //
            PacketHeader header = new PacketHeader(
                msgType: PacketType.RoutingExchange,
                srcID: _sourceNode,
                dstID: destination,
                msgID: Guid.NewGuid(),
                logicalClock: _lamportClock.GetCurrent(),
                senderCounter: _senderCounter.GetCurrent()
            );
            return new DataPacket(header, payload);
        }
    }
}
