using System.Net;

namespace TacMesh.Core.interfaces.beacon___destination
{
    /// <summary>
    /// A beacon emitter is a small hardware or software device that a Node to constantly
    /// broadcast its presence or location to the environment, whether it's physical or in a simulation
    /// </summary>
    public interface IBeaconEmitter
    {
        void EmitBeacon(byte[] beacon);
    }
}
