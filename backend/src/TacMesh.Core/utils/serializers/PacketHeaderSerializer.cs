using TacMesh.Core.interfaces;
using TacMesh.Core.packet;

namespace TacMesh.Core.utils.serializers
{
    /// <summary>
    /// PacketHeader Serializer class. Serializes a PacketHeader into a byte stream,
    /// and Deserializers a byte stream back into a PacketHeader.
    /// </summary>
    public class PacketHeaderSerializer : IByteSerializer<PacketHeader>
    {
        /// <summary>
        /// Method that serializes all of the header fields into a byte stream
        /// </summary>
        /// <returns>PacketHeader byte stream if successful</returns>
        public byte[] Serialize(PacketHeader header)
        {
            // byte stream exactly 76 bytes
            byte[] byteStream = new byte[PacketHeader.HEADER_SIZE];

            // byte writer to convert data into bytes and write to the stream 
            ByteWriter byteWriter = new ByteWriter(byteStream);

            byteWriter.WriteBytes(header.ProtocolVersion);
            byteWriter.WriteBytes((byte)header.MsgType);
            byteWriter.WriteBytes(header.MsgID);
            byteWriter.WriteBytes(header.SrcID, PacketHeader.LegalStringLength);
            byteWriter.WriteBytes(header.DstID, PacketHeader.LegalStringLength);
            byteWriter.WriteBytes(header.HopCount);
            byteWriter.WriteBytes(header.LogicalClock);
            byteWriter.WriteBytes(header.TTL);
            byteWriter.WriteBytes(header.Priority);
            byteWriter.WriteBytes(header.SenderCounter);

            return byteStream;
        }

        /// <summary>
        /// Method that deserializes a byte stream into a PacketHeader object
        /// </summary>
        /// <param name="header_byte_stream"></param>
        /// <returns></returns>
        public PacketHeader Deserialize(byte[] byteStream)
        {
            // reject byte streams shorter than the header size
            if (byteStream.Length != PacketHeader.HEADER_SIZE)
                throw new ArgumentOutOfRangeException($"Unable to Deserialize due to: Provided byte stream was not {PacketHeader.HEADER_SIZE} bytes");

            ByteReader reader = new ByteReader(byteStream);
            int len = PacketHeader.LegalStringLength;

            return new PacketHeader(
                p_version: reader.ReadByte(),
                msgType: (PacketType)reader.ReadByte(),
                msgID: reader.ReadGuid(),
                srcID: reader.ReadString(len),
                dstID: reader.ReadString(len),
                hopCount: reader.ReadByte(),
                logicalClock: reader.ReadLong(),
                ttl: reader.ReadLong(),
                priority: reader.ReadByte(),
                sender_counter: reader.ReadLong()
            );
        }
    }
}
