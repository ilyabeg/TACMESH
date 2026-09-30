using TacMesh.Core.interfaces;

namespace TacMesh.Core.builders
{
    /// <summary>
    /// The PacketHeaderBuilder class implements the 'Builder' Design Pattern to supplying easy, 
    /// flexible and readable building of a PacketHeader while containing the exact header format perfectly.    
    /// </summary>
    public class PacketHeaderBuilder : IPacketHeaderBuilder
    {
        // private working header
        private readonly PacketHeader _header;

        // constructor
        public PacketHeaderBuilder(PacketHeader header) => _header = header;        

        // building methods
        public IPacketHeaderBuilder SetProtocolVersion(byte p_version)
        {
            _header.ProtocolVersion = p_version;
            return this;
        }

        public IPacketHeaderBuilder SetSourceID(string srcID)
        {            
            _header.SrcID = srcID;
            return this;
        }       

        public IPacketHeaderBuilder SetHopCount(byte hopCount = 0) // if hop count was not provided, the default is 0
        {           
            _header.HopCount = hopCount;            
            return this;
        }

        public IPacketHeaderBuilder SetSenderCounter(long senderCounter)
        {
            _header.SenderCounter = senderCounter;
            return this;          
        }

        public IPacketHeaderBuilder SetTimeToLive(long ttl)
        {            
            _header.TTL = ttl;
            return this;          
        }

        public IPacketHeaderBuilder SetMessageType(PacketType type)
        {            
            _header.MsgType = type;
            return this;
        }

        public IPacketHeaderBuilder SetMessageID(string msgID)
        {
            _header.MsgID = msgID;
            return this;
        }

        public IPacketHeaderBuilder SetDestinationID(string dstID)
        {            
            _header.DstID = dstID;
            return this;
        }

        public IPacketHeaderBuilder SetPriority(byte priority)
        {
            _header.Priority = priority;
            return this;
        }

        public IPacketHeaderBuilder SetLogicalClock(long logicalClock)
        {         
            // save as primitive and as bytes
            _header.LogicalClock = logicalClock;
            return this;           
        }

        // build
        public PacketHeader BuildHeader() => _header;
    }
}
