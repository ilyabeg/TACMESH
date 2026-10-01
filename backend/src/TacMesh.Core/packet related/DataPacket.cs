namespace TacMesh.Core.packet_related
{
    public class DataPacket
    {
        public const int MaxPacketSize = 1500; // TEMPORARILY PUT HERE MAX PACKET SIZE
        public PacketHeader PacketHeader { get; private set; }
    }
}
