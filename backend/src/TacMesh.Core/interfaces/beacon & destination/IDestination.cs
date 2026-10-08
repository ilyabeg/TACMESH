using System.Net;
using TacMesh.Core.graph_related;

namespace TacMesh.Core.interfaces
{
    public interface IDestination
    {
        public string NodeId { get; }
        public IPEndPoint Address { get; }
        public Location LocationPoint { get; }
    }
}
