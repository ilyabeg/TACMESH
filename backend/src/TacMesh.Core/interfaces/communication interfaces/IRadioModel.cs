using System.Net;

namespace TacMesh.Core.interfaces.communication_interfaces
{
    public interface IRadioModel
    {
        /// <summary>
        /// Method to check connection between two agent nodes
        /// </summary>
        /// <returns>True if a connection is valid. False otherwise.</returns>
        Task<bool> TestConnection(IPEndPoint source, IPEndPoint destination);
    }
}
