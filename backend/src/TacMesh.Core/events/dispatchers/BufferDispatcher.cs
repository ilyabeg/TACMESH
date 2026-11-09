using TacMesh.Core.assets.node.packet;
using TacMesh.Core.assets.node.packet.buffer;
using TacMesh.Core.interfaces.routing;
using TacMesh.Core.packet;
using TacMesh.Core.tables.node;

namespace TacMesh.Core.events.dispatchers
{
    /// <summary>
    /// Buffer Dispatcher class to dispatch the buffered packets if
    /// the node discovers new neighbors
    /// </summary>
    public class BufferDispatcher
    {
        // dependencied
        private readonly NeighborTable _neighborTable;
        private readonly PacketSender _packetSender;        
        private readonly PacketBuffer _packetBuffer; 
        private readonly IRouting _routing; 
        
        
        // constructor
        public BufferDispatcher(NeighborTable neighborTable, PacketSender packetSender, PacketBuffer packetBuffer, IRouting routing)
        {
            _neighborTable = neighborTable;
            _packetSender = packetSender;
            _packetBuffer = packetBuffer;
            _routing = routing;

            // attach handler to neighbor received event to dispatch
            // the buffered messages
            //
            _neighborTable.OnNeighborDiscovered += (s, e) => DistpatchBuffer(e);
        }

        /// <summary>
        /// Dispatches the buffered messages to the new discovered neighbor
        /// </summary>
        /// <param name="newNeighbor">The neighbor identifier</param>
        private void DistpatchBuffer(string newNeighbor)
        {
            // run on background thread to free UpdateRecord() function
            // in the NeighborTable while this funtcion runs
            //
            Task.Run(() => 
            {
                // try to forward each packet from the buffer if could be 
                // routed to this new neighbor
                //
                foreach (DataPacket packet in _packetBuffer.GetCurrentPackets())
                {
                    // get the next hops from the routing method
                    //
                    List<string> nextHops = _routing.Route(packet);

                    // if new neighbor is included in the next hops, forward the packet
                    //
                    if (nextHops.Contains(newNeighbor))
                        _packetSender.ExecuteSend(packet, peerId: newNeighbor);
                }
            });            
        }
    }
}
