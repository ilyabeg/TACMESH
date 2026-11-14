using System.Collections.Concurrent;
using TacMesh.Core.assets.clocks;
using TacMesh.Core.assets.node.packet;
using TacMesh.Core.interfaces;
using TacMesh.Core.packet;

namespace TacMesh.Core.assets.routing.prophet
{
    /// <summary>
    /// Handler class to handle PRoPHET rouing protocol routing exchange messages
    /// </summary>
    public class ProphetExchangeHandler
    {
        // dependencies
        private readonly IByteSerializer<ConcurrentDictionary<string, double>> _serializer;
        private readonly PacketSender _packetSender;
        private readonly LamportClock _lamportClock;
        private readonly SenderCounter _senderCounter;
        private readonly string _sourceNode;

        // constructor
        public ProphetExchangeHandler(IByteSerializer<ConcurrentDictionary<string, double>> serializer, PacketSender packetSender, LamportClock lamportClock, SenderCounter senderCounter, string sourceNode)
        {
            _serializer = serializer;
            _packetSender = packetSender;
            _lamportClock = lamportClock;
            _senderCounter = senderCounter;
            _sourceNode = sourceNode;
        }

        // --- Probability Vector exchange methods ---

        /// <summary>
        /// Dispatcher method that transmits the probability vector to a specific neighbor
        /// </summary>
        /// <param name="neighbor">The specific neighbor</param>
        /// <param name="prVector">The current probability vector</param>
        public void DispatchVectorExchange(string neighbor, ConcurrentDictionary<string, double> prVector)
        {
            // serialize the current local probability vector state
            //
            byte[] currentVector = _serializer.Serialize(prVector);

            // build the actual packet
            DataPacket routingExchange = BuildExchangeVector(neighbor, currentVector);

            // transmit the packet to the neighbor
            _packetSender.SendPacket(routingExchange);
        }

        /// <summary>
        /// Translator method to get the deserialized probability vector from the byte array
        /// </summary>
        /// <param name="vectorBytes">The received byte array of the vector</param>
        /// <returns>The deserialized probability vector</returns>
        public ConcurrentDictionary<string, double> TranslateVector(byte[] vectorBytes) => _serializer.Deserialize(vectorBytes);

        /// <summary>
        /// Builds a probability vector exchange packet to a specific node
        /// </summary>
        /// <param name="destination">The node's identifier</param>
        /// <param name="payload">The serialized probability vector as payload</param>
        /// <returns>The built vector exchange packet</returns>
        private DataPacket BuildExchangeVector(string destination, byte[] payload)
        {
            // build the header
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
