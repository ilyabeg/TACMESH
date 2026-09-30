using System.Buffers.Binary;
using System.Text;

namespace TacMesh.Core.serializing_related
{
    /// <summary>
    /// The Byte Reader class is a helper class used to deserialize any type of data into from
    /// byte representation. By following the SOLID Principle O/C, I leave this class OPEN for 
    /// extension, and CLOSED for modification.
    /// </summary>
    public class ByteReader
    {
        // Memory of type byte to keep a "clone" of the actual byte stream that needs to be translated.        
        private Memory<byte> _byteMemory;

        // byte sizes
        private readonly int _1byte = 1;
        private readonly int _8byte = 8;

        // fixed index 0
        private readonly int _index0 = 0;

        // Inject the byte buffer in construction
        public ByteReader(byte[] buffer)
        {
            // now the Memory holds the bytes
            _byteMemory = buffer;
        }

        // --- Write Overloads ---
        public byte ReadByte()
        {
            // use Span to not create new memory and to read the data from the Memory object.
            byte field = _byteMemory.Span[_index0];

            // slice the written data: shrinks the Memory object by 1 byte
            _byteMemory = _byteMemory.Slice(_1byte);

            return field;
        }

        public long ReadLong()
        {
            // write the 8 bytes with BigEndian order
            long field = BinaryPrimitives.ReadInt64BigEndian(_byteMemory.Span);

            // slice the written data: shrinks the Memory object by 8 bytes
            _byteMemory = _byteMemory.Slice(_8byte);

            return field;
        }

        public string ReadString(int length)
        {
            // slice a chunk of 'length' bytes out of the Memory object
            Span<byte> _byteSpan = _byteMemory.Span.Slice(_index0, length);

            // it is not possible to slice a read only Span, so extract the length without \0 bytes
            // the 1st occurence if the null byte, is the length of the actual string.
            // for example: "ABCD\0\0\0\0\0\0...", first null byte is at index 4, and the string length is 4
            // so we get the slice of the first 4 indexes
            byte nullByte = 0;
            int indexOfNull = _byteSpan.IndexOf(nullByte); // return index of the first occurance

            // it is not possible to slice a read only Span, so extract the length without \0 bytes
            if (indexOfNull != -1)
                _byteSpan = _byteSpan.Slice(_index0, indexOfNull);

            // write the string into that chunk
            string field = Encoding.UTF8.GetString(_byteSpan);

            // slice the written data: shrinks the Memory object by 'length' bytes
            _byteMemory = _byteMemory.Slice(length);

            return field;
        }
    }
}
