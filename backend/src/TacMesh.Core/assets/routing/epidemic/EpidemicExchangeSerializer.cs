using TacMesh.Core.interfaces;

namespace TacMesh.Core.assets.routing.epidemic
{
    /// <summary>
    /// Epidemic routing protocol's routing exchange packet serializer.
    /// Epidemic pairwise exchange phase requires the nodes to exchange 
    /// a list of their internal stored packet identifiers from the buffer
    /// </summary>
    public class EpidemicExchangeSerializer : IByteSerializer<List<Guid>>
    {        
        public byte[] Serialize(List<Guid> input)
        {
            throw new NotImplementedException();
        }

        public List<Guid> Deserialize(byte[] input)
        {
            throw new NotImplementedException();
        }
    }
}
