namespace TacMesh.Core.interfaces
{
    /// <summary>
    /// Generic Serializer interface for serializing and deserializing data between two types. 
    /// This interface defines the methods for converting an input of type TInput to an output
    /// of type TOutput and vice versa.
    /// </summary>
    /// <typeparam name="TOutput">The output expected out of the serialization</typeparam>
    /// <typeparam name="TInput">The input expected to the serialization</typeparam>
    public interface ISerializer<TOutput, TInput>
    {
        public TOutput Serialize(TInput input);
        public TInput Deserialize(TOutput input);
    }
}
