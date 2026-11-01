using TacMesh.Core.graph_related;

namespace TacMesh.Core.interfaces.beacon_and_destination
{
    /// <summary>
    /// A beacon emitter is a small hardware or software device that a Node to constantly
    /// broadcast its presence or location to the environment, whether it's physical or in a simulation
    /// </summary>
    public interface IBeaconEmitter
    {
        void EmitBeacon(Location location);
    }
}
