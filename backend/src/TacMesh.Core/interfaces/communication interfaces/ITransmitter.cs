using System.Net;
using TacMesh.Core.custom_events;

namespace TacMesh.Core.interfaces
{
    public interface ITransmitter
    {
        // public crash event handler to handle listening crashes
        public event EventHandler<CrashEventArgs> CrashedEventHandler;

        // interface method
        public void Transmit(byte[] data, IPEndPoint destination);
    }
}
