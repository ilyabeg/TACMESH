namespace TacMesh.Core.interfaces
{
    public interface IPacketHeaderBuilder
    {
        public IPacketHeaderBuilder SetProtocolVersion(double p_version);
        public IPacketHeaderBuilder SetSourceID(string srcID);
        public IPacketHeaderBuilder SetHopCount(int hopCount = 0);
        public IPacketHeaderBuilder SetSenderCounter(long senderCounter);
        public IPacketHeaderBuilder SetTimeToLive(long ttl);
        public IPacketHeaderBuilder SetMessageType(PacketType type);
        public IPacketHeaderBuilder SetMessageID(string msgID);
        public IPacketHeaderBuilder SetDestinationID(string dstID);
        public IPacketHeaderBuilder SetPriority(int priority);
        public IPacketHeaderBuilder SetLogicalClock(long logicalClock);
    }
}
