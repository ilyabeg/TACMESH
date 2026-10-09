namespace TacMesh.Core.packet_related
{
    public class DataPacket
    {
        public const int MaxPacketSize = 1500; // TEMPORARILY PUT HERE MAX PACKET SIZE

        // packet fields
        public PacketHeader PacketHeader { get; private set; }
        public string Payload { get; private set; }
        
        public DataPacket(PacketHeader packetHeader, string payload)
        {
            PacketHeader = packetHeader;
            Payload = payload;
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }
    }
}
