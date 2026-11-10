using System.Net;
using TacMesh.Core.assets.node.packet;
using TacMesh.Core.assets.node.packet.buffer;
using TacMesh.Core.interfaces.routing;
using TacMesh.Core.packet;

namespace TacMesh.Core.events.dispatchers
{
    /// <summary>
    /// Packet Dispatcher class used to wrap the PacketSender and dispatch packets through it.
    /// Understands concepts like: PacketBuffer, the Routing interface
    /// </summary>
    public class PacketDispatcher
    {
        // dependencies
        private readonly IRouting _routing;
        private readonly PacketSender _packetSender;
        private readonly PacketBuffer _packetBuffer;

        // constructor
        public PacketDispatcher(IRouting routing, PacketSender packetSender, PacketBuffer packetBuffer)
        {
            _routing = routing;
            _packetSender = packetSender;
            _packetBuffer = packetBuffer;
        }

        /// <summary>
        /// Dispatches the packet to the next hops via the routing interface.
        /// </summary>
        /// <param name="packet">The packet to dispatch</param>
        /// <param name="excludeAddress">The address to exclude from the dispatch</param>
        public void DispatchPacket(DataPacket packet, IPEndPoint? excludeAddress = null)
        {
            // return, if the packet was already processed and stored in the buffer.
            // this stops from retransmitting the packet infinitely on the network
            //
            if (_packetBuffer.Contains(packet)) return;

            // allways store the message in the buffer
            //
            _packetBuffer.Add(packet);
            
            List<string> nextHops = _routing.Route(packet); // get the next hops

            // forward the packet to all of the next hops directly
            //
            foreach (string nextHop in nextHops)
                _packetSender.ExecuteSend(packet, exluciveAddress: excludeAddress, peerId: nextHop);
        }
    }
}
