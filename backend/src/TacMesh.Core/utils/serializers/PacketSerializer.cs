using TacMesh.Core.interfaces;
using TacMesh.Core.packet;

namespace TacMesh.Core.utils.serializers
{
    public class PacketSerializer: IByteSerializer<DataPacket>
    {
        // header serializer
        private static readonly PacketHeaderSerializer _headerSerializer = new();

        /// <summary>
        /// Serializes the provided data packet into a byte array
        /// </summary>
        /// <param name="dataPacket"></param>
        /// <returns>The serialized bytes of the DataPacket</returns>
        public byte[] Serialize(DataPacket dataPacket)
        {
            // serialize the header first
            //
            byte[] headerBytes = _headerSerializer.Serialize(dataPacket.PacketHeader);

            // serialize the payload
            //
            // Note: TEMPORARILY input the raw bytes. later, when the 
            // cryptography algorithms will be added, the payload will be encrypted 
            //            

            // digitally sign the packet
            //
            // TO DO ...

            // concatenate the byte arrays into 1 continuous byte array
            // by using the spread operator
            //
            return [.. headerBytes, .. dataPacket.Payload];
        }

        /// <summary>
        /// Deserializes the provided byte array into a data packet
        /// </summary>
        /// <param name="packetBytes"></param>
        /// <returns>The deserialized DataPacket</returns>
        public DataPacket Deserialize(byte[] packetBytes)
        {
            // deserialize the first 76 header bytes first
            //
            byte[] headerBytes = packetBytes.AsSpan().Slice(start: 0, PacketHeader.HEADER_SIZE).ToArray();
            PacketHeader header = _headerSerializer.Deserialize(headerBytes);

            // deserialize the payload
            //
            // NOTE: currently deserializing simply with utf8 encoding untill cryptography 
            // algorithms are added in the future. plus when a signature is implemented the 
            // payload will have to be pulled by it's length from the total bytes array
            //
            int payloadLength = packetBytes.Length - PacketHeader.HEADER_SIZE;
            byte[] payloadBytes = packetBytes.AsSpan().Slice(start: PacketHeader.HEADER_SIZE, payloadLength).ToArray();

            // authenticate the signature with the mission package per the proposal
            //
            // TO DO ...

            // return the full data packet
            //
            return new DataPacket(header, payloadBytes);
        }
    }
}
