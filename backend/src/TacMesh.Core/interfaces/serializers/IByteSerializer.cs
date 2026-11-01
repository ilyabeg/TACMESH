namespace TacMesh.Core.interfaces
{
    public struct ByteCount
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
    public interface IByteSerializer<T> : ISerializer<byte[], T>
    {
        // does not provide any additional methods, but is a more specific interface for byte serialization        
    }
}
