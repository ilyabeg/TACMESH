using System.Net;

namespace TacMesh.Core.interfaces.communication_interfaces
{
    public interface IRadioModel
    {
        /// <summary>
        /// Method to evaluate a connection between two engine nodes
        /// </summary>
        /// <returns>True if a connection is valid. False otherwise.</returns>
        Task<bool> TestConnection(IPEndPoint source, IPEndPoint destination);

        /// <summary>
        /// returns the current knwon peers in the network.
        /// </summary>
        Dictionary<string, IPEndPoint> GetPeers();
    }
}
