namespace TacMesh.Core.interfaces
{
    public interface IPacketHeaderBuilder
    {
        public IPacketHeaderBuilder SetProtocolVersion(byte p_version);
        public IPacketHeaderBuilder SetSourceID(string srcID);
        public IPacketHeaderBuilder SetHopCount(byte hopCount = 0);
        public IPacketHeaderBuilder SetSenderCounter(long senderCounter);
        public IPacketHeaderBuilder SetTimeToLive(long ttl);
        public IPacketHeaderBuilder SetMessageType(PacketType type);
        public IPacketHeaderBuilder SetMessageID(Guid msgID);
        public IPacketHeaderBuilder SetDestinationID(string dstID);
        public IPacketHeaderBuilder SetPriority(byte priority);
        public IPacketHeaderBuilder SetLogicalClock(long logicalClock);
    }
}
