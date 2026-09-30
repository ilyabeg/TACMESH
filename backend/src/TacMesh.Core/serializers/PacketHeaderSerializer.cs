using TacMesh.Core.builders;
using TacMesh.Core.interfaces;
using TacMesh.Core.packet_related;

namespace TacMesh.Core.serializing_related
{
    /// <summary>
    /// PacketHeader Serializer class. Serializes a PacketHeader into a byte stream,
    /// and Deserializers a byte stream back into a PacketHeader.
    /// </summary>
    public class PacketHeaderSerializer : ISerializer<byte[], PacketHeader>
    {
        /// <summary>
        /// Method that serializes all of the header fields into a byte stream
        /// </summary>
        /// <returns>PacketHeader byte stream if successful</returns>
        public byte[] Serialize(PacketHeader header)
        {
            // byte stream exactly 76 bytes
            byte[] byteStream = new byte[PacketHeader.HeaderSize];

            // byte writer to convert data into bytes and write to the stream 
            ByteWriter byteWriter = new ByteWriter(byteStream);

            byteWriter.WriteBytes(header.ProtocolVersion);
            byteWriter.WriteBytes((byte)header.MsgType);
            byteWriter.WriteBytes(header.MsgID, PacketHeader.LegalStringLength);
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
            if (byteStream.Length < PacketHeader.HeaderSize)
                throw new ArgumentOutOfRangeException($"Unable to Deserialize due to: Provided byte stream was shorter than {PacketHeader.HeaderSize} bytes");

            PacketHeaderBuilder builder = new PacketHeaderBuilder(new PacketHeader());
            ByteReader reader = new ByteReader(byteStream);

            int len = PacketHeader.LegalStringLength;

            builder.SetProtocolVersion(reader.ReadByte())
                .SetMessageType((PacketType)reader.ReadByte())
                .SetMessageID(reader.ReadString(len))
                .SetSourceID(reader.ReadString(len))
                .SetDestinationID(reader.ReadString(len))
                .SetHopCount(reader.ReadByte())
                .SetLogicalClock(reader.ReadLong())
                .SetTimeToLive(reader.ReadLong())
                .SetPriority(reader.ReadByte())
                .SetSenderCounter(reader.ReadLong());

            return builder.BuildHeader();
        }
    }
}
