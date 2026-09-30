namespace TacMesh.Core.interfaces
{
    /// <summary>
    /// Serializer interface in case I build other types of Serializers in the future.
    /// </summary>
    /// <typeparam name="TOutput">The output expected out of the serialization</typeparam>
    /// <typeparam name="TInput">The input expected to the serialization</typeparam>
    public interface ISerializer<TOutput, TInput>
    {
        public TOutput Serialize(TInput input);
        public TInput Deserialize(TOutput input);
    }
}
