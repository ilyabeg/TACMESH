using TacMesh.Core.packet;

namespace TacMesh.Core.interfaces.routing
{
    /// <summary>
    /// Routing interface to segregate between different routing approaches and make 
    /// the system work without knowing which type of routing algorithm it uses
    /// </summary>
    public interface IRouting
    {
        /// <summary>
        /// Router method to determine the next hops for the DataPacket
        /// </summary>
        /// <param name="packet">The packet to route through to the next hops</param>
        /// <returns>A list containing the identifiers of the next hops</returns>
        List<string> Route(DataPacket packet);
    }
}
