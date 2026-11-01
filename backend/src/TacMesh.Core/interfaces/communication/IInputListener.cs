namespace TacMesh.Core.interfaces.communication
{
    /// <summary>
    /// Interface used to segregate between classes that are responsible for 
    /// listening for standard inputs, for different reasons
    /// </summary>
    public interface IInputListener
    {
        void ListenForInput();
    }
}
