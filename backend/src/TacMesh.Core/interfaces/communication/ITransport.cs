namespace TacMesh.Core.interfaces
{
    /// <summary>
    /// Transport interface to transmit raw bytes using the transmitter and receive
    /// using the receiver. Understands concepts like: Port, IP, Socket, Bytes
    /// </summary>
    public interface ITransport : ITransmitter, IReceiver
    {
        int GetAssignedPort();
        void ShutDown();
    }
}
