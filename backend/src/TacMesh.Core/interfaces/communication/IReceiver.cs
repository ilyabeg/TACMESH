using TacMesh.Core.custom_events;
using TacMesh.Core.events;

namespace TacMesh.Core.interfaces
{
    public interface IReceiver
    {
        // public crash event handler to handle crashes
        public event EventHandler<CrashEventArgs> Crashed;
        // public message received event handler to handle incoming messages
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        // interface method
        public void StartReceiving();
    }
}
