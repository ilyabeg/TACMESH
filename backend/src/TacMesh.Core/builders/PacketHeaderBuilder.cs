using System.Buffers.Binary;
using System.Text;
using TacMesh.Core.interfaces;
using TacMesh.Core.packet_related;

namespace TacMesh.Core.builders
{
    /// <summary>
    /// The PacketHeaderBuilder class implements the 'Builder' Design Pattern for a couple of good reasons:
    /// 1) Supplying easy, flexible and readable building of a PacketHeader while containing the exact header format perfectly.
    /// 2) Handles data integrity and error checking to make sure the input data is correctly formated. If not: throws an
    ///    Exception with a fitting message describing the problem.
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

            //// string values don't need Big Endian order because each letter is a single byte
            //byte[] arr = Encoding.UTF8.GetBytes(srcID);

            //int buffer_size = 16; // field's size in bytes
            //byte[] buffer = new byte[buffer_size];

            //// copy to buffer that adds padding if shorter than 16
            //arr.CopyTo(buffer, 0);
            //_header.B_SrcID = buffer;
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

            //int buffer_size = 8; // long size in bytes
            //byte[] buffer = new byte[buffer_size];
            //BinaryPrimitives.TryWriteInt64BigEndian(buffer, senderCounter);

            //// convert to byte array
            //_header.B_SenderCounter = buffer;            
        }

        public IPacketHeaderBuilder SetTimeToLive(long ttl)
        {            
            _header.TTL = ttl;
            return this;

            //int buffer_size = 8; // long size in bytes
            //byte[] buffer = new byte[buffer_size];
            //BinaryPrimitives.TryWriteInt64BigEndian(buffer, ttl);

            //// convert to byte array
            //_header.B_TTL = buffer;            
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

            //// string values don't need Big Endian order because each letter is a single byte
            //byte[] arr = Encoding.UTF8.GetBytes(msgID);

            //int buffer_size = 16; // field's size in bytes
            //byte[] buffer = new byte[buffer_size];

            //// copy to buffer that adds padding if shorter than 16
            //arr.CopyTo(buffer, 0);
            //_header.B_MsgID = buffer;
        }

        public IPacketHeaderBuilder SetDestinationID(string dstID)
        {            
            _header.DstID = dstID;
            return this;

            //// string values don't need Big Endian order because each letter is a single byte
            //byte[] arr = Encoding.UTF8.GetBytes(dstID);

            //int buffer_size = 16; // field's size in bytes
            //byte[] buffer = new byte[buffer_size];

            //// copy to buffer that adds padding if shorter than 16
            //arr.CopyTo(buffer, 0);
            //_header.B_DstID = buffer;
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

            //int buffer_size = 8; // long size in bytes
            //byte[] buffer = new byte[buffer_size]; 
            //BinaryPrimitives.TryWriteInt64BigEndian(buffer, logicalClock);

            //// convert to byte array
            //_header.B_LogicalClock = buffer;            
        }

        // build
        public PacketHeader BuildHeader() => _header;
    }
}
