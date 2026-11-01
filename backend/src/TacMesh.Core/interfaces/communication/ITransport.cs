namespace TacMesh.Core.interfaces
{
    public interface ITransport : ITransmitter, IReceiver
    {
        int GetAssignedPort();
        void ShutDown();
    }
}
