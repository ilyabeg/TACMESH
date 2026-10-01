namespace TacMesh.Core.interfaces
{
    /// <summary>
    /// Non generic byte serializer interface to provide byte constants
    /// </summary>
    public interface IByteSerializer
    {
        // constant byte sizes
        public const int _1byte = 1;
        public const int _8bytes = 8;
        public const int _16bytes = 16;
    }

    /// <summary>
    /// Serializer interface for serializing and deserializing data into and from a byte array. 
    /// This interface defines the methods for converting an input of type T to an output
    /// of type byte[] and vice versa.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize/deserialize</typeparam>
    public interface IByteSerializer<T> : ISerializer<byte[], T>, IByteSerializer
    {
        // does not provide any additional methods, but is a more specific interface for byte serialization        
    }
}
