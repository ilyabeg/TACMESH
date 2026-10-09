using System.Collections.Concurrent;
using System.Net;
using TacMesh.Core.graph_related;

namespace TacMesh.Core.interfaces.communication_interfaces
{
    public interface IRadioModel
    {
        /// <summary>
        /// Method to check connection between two agent nodes
        /// </summary>
        /// <returns>True if a connection is valid. False otherwise.</returns>
        Task<bool> TestConnection(Location source, Location destination);

        /// <summary>
        /// Returns all the devices on the network.
        /// </summary>
        /// <returns></returns>
        ConcurrentDictionary<string, IPEndPoint> GetNetworkDevices();
    }
}
