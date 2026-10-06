using System.Collections.Concurrent;
using System.Net;

namespace TacMesh.Core.interfaces
{
    public interface IDestinationProvider
    {
        ConcurrentDictionary<IPEndPoint, IDestination> ProvideDestinations();
    }
}
