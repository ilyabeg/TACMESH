using TacMesh.Core.packet;

namespace TacMesh.Core.interfaces.routing
{
    /// <summary>
    /// Routing interface to segregate between different routing approaches and make 
    /// the system work without knowing which type of routing algorithm it uses
    /// </summary>
    public interface IRouting
    {
        void Route(DataPacket packet);
    }
}
