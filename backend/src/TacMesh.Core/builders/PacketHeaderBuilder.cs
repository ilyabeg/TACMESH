using TacMesh.Core.interfaces;

namespace TacMesh.Core.builders
{
    /// <summary>
    /// The PacketHeaderBuilder class implements the 'Builder' Design Pattern to supplying easy, 
    /// flexible and readable building of a PacketHeader while containing the exact header format perfectly.    
    /// </summary>
    public class PacketHeaderBuilder : IPacketHeaderBuilder
    {        
        #region private header fields for creation
        private byte _p_version;
        private PacketType _msgType;
        private Guid _msgID;
        private string _srcID;
        private string _dstID;
        private byte _hopCount;
        private long _logicalClock;
        private long _ttl;
        private byte _priority;
        private long _sender_counter;
        #endregion

        // building methods
        public IPacketHeaderBuilder SetProtocolVersion(byte p_version)
        {
            _p_version = p_version;
            return this;
        }
        public IPacketHeaderBuilder SetMessageType(PacketType type)
        {
            _msgType = type;
            return this;
        }
        public IPacketHeaderBuilder SetMessageID(Guid msgID)
        {
            _msgID = msgID;
            return this;
        }
        public IPacketHeaderBuilder SetSourceID(string srcID)
        {            
            _srcID = srcID;
            return this;
        }
        public IPacketHeaderBuilder SetDestinationID(string dstID)
        {
            _dstID = dstID;
            return this;
        }
        public IPacketHeaderBuilder SetHopCount(byte hopCount = 0) // if hop count was not provided, the default is 0
        {           
            _hopCount = hopCount;            
            return this;
        }
        public IPacketHeaderBuilder SetLogicalClock(long logicalClock)
        {         
            _logicalClock = logicalClock;
            return this;           
        }
        public IPacketHeaderBuilder SetTimeToLive(long ttl)
        {
            _ttl = ttl;
            return this;
        }
        public IPacketHeaderBuilder SetPriority(byte priority)
        {
            _priority = priority;
            return this;
        }
        public IPacketHeaderBuilder SetSenderCounter(long senderCounter)
        {
            _sender_counter = senderCounter;
            return this;
        }

        // build
        public PacketHeader BuildHeader() => new PacketHeader(
            _p_version,
            _msgType,
            _msgID,
            _srcID,
            _dstID,
            _hopCount,
            _logicalClock,
            _ttl,
            _priority,
            _sender_counter
        );
    }
}
