using System.Buffers.Binary;
using System.Text;

namespace TacMesh.Core.packet_related
{
    /// <summary>
    /// The Byte writer class is a helper class used to serialize any type of data into it's
    /// byte representation. By following the SOLID Principle O/C, I leave this class OPEN for 
    /// extension, and CLOSED for modification.
    /// </summary>
    public class ByteWriter
    {
        // Memory of type byte to keep a "clone" of the actual byte stream that needs to built.        
        private Memory<byte> _byteMemory;

        // byte sizes
        private readonly int _1byte = 1;
        private readonly int _8byte = 8;

        // fixed index 0
        private readonly int _index0 = 0;

        // Inject the empty buffer in construction
        public ByteWriter(byte[] buffer)
        {
            // now the Memory holds the bytes
            _byteMemory = buffer;
        }

        // --- Write Overloads ---
        public void WriteBytes(byte data)
        {
            // use Span to not create new memory and to write the data into the Memory object.
            // writing to the Span, writes both to the Memory object and the original array
            _byteMemory.Span[_index0] = data;

            // slice the written data: shrinks the Memory object by 1 byte
            _byteMemory = _byteMemory.Slice(_1byte);
        }

        public void WriteBytes(long data)
        {
            // write the 8 bytes with BigEndian order
            BinaryPrimitives.WriteInt64BigEndian(_byteMemory.Span, data);

            // slice the written data: shrinks the Memory object by 8 bytes
            _byteMemory = _byteMemory.Slice(_8byte);
        }

        public void WriteBytes(string data, int length)
        {
            // slice a chunk of 'length' bytes out of the Memory object
            Span<byte> _byteSpan = _byteMemory.Span.Slice(_index0, length);

            // write the string into that chunk
            Encoding.UTF8.GetBytes(data, _byteSpan);

            // slice the written data: shrinks the Memory object by 'length' bytes
            _byteMemory = _byteMemory.Slice(length);
        }
    }
}
