using TacMesh.Core.assets.node.packet.buffer;
using TacMesh.Core.interfaces.routing;
using TacMesh.Core.packet;
using TacMesh.Core.tables.node;

namespace TacMesh.Core.assets.routing.epidemic
{
    /// <summary>
    /// Epidemic routing algorithm method. Forwards stored datapackets to all of the current
    /// neighbors of the node, to spread buffered packets like a virus across the network.
    /// </summary>
    public class EpidemicRouting : IRouting
    {
        // dependencies
        private readonly NeighborTable _neighborTable;        
        private readonly PacketBuffer _packetBuffer;
        private readonly EpidemicExchangeHandler _exchangeHandler;


        // constructor
        public EpidemicRouting(NeighborTable neighborTable, PacketBuffer packetBuffer, EpidemicExchangeHandler exchangeHandler)
        {
            _neighborTable = neighborTable;
            _packetBuffer = packetBuffer;
            _exchangeHandler = exchangeHandler;

            // attach event handler for discovering new neighbors
            _neighborTable.OnNeighborDiscovered += (s, e) => _exchangeHandler.DispatchSumVector(e);
        }
        
        // --- IRouting interface methods ---

        /// <summary>
        /// Router method to determine the next hops for the DataPacket
        /// </summary>
        /// <param name="packet">The packet to route through to the next hops</param>
        /// <returns>A list containing the identifiers of the next hops</returns>
        public List<string> Route(DataPacket packet)
        {
            // Epidemic routing allways returns all of the current neighbors as the 
            // next hops to spread the packet in the network like a virus.
            //
            return _neighborTable.GetCurrentNeighbors();
        }

        /// <summary>
        /// Processor for Epidemic routing exchange packet payloads. processes incoming
        /// remote buffered packet identifiers packet exchanges
        /// </summary>
        /// <param name="senderId">The identifier of the paacket sender.</param>
        /// <param name="exchangePayload">The routing exchange payload bytes.</param>
        public void ProcessRoutingExchange(string senderId, byte[] exchangePayload)
        {
            
        }        
    }
}
