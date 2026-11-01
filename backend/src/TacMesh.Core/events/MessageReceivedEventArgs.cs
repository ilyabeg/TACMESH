using System.Net;

namespace TacMesh.Core.events
{
    /// <summary>
    /// Event arguments for the MessageReceived event.
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        public byte[] MessageBytes { get; }
        public IPEndPoint RemoteEndPoint { get; }

        public MessageReceivedEventArgs(byte[] messageBytes, IPEndPoint remoteEndPoint)
        {
            MessageBytes = messageBytes;
            RemoteEndPoint = remoteEndPoint;
        }
    }
}
