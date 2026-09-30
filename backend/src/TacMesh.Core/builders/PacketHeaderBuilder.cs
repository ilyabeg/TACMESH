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
    ///    
    /// NOTES: 1. Fix _maxTTL in future: currently a mgic number, needs to be global from a different source, such as the Core.
    ///        2. Same goes for the LatestVersion
    /// </summary>
    public class PacketHeaderBuilder : IPacketHeaderBuilder
    {
        // temporarily put the fixed magic numbers here 
        public double LatestVersion { get; private set; } = 1.0;
        private readonly long _maxTTL = 10000000; // in seconds
        private readonly int _16bytes = 16; // 16 bytes fixed size

        // private working header
        private readonly PacketHeader _header;

        // constructor
        public PacketHeaderBuilder(PacketHeader header) => _header = header;        

        // building methods
        public IPacketHeaderBuilder SetProtocolVersion(double p_version)
        {
            if (p_version != LatestVersion)
                throw new ArgumentException($"Error initiating Protocol Version. Protocol Version must be up to date (v{LatestVersion:F1})");

            // save as primitive and as bytes
            _header.ProtocolVersion = p_version;
            return this;
        }
        public IPacketHeaderBuilder SetSourceID(string srcID)
        {
            if (srcID.IsWhiteSpace() || srcID.Length > _16bytes)
                throw new ArgumentException("Error initiating Source ID. The ID must be under 16 characters and contain text");

            // save as primitive and as bytes
            _header.SrcID = srcID;

            // string values don't need Big Endian order because each letter is a single byte
            byte[] arr = Encoding.UTF8.GetBytes(srcID);

            int buffer_size = 16; // field's size in bytes
            byte[] buffer = new byte[buffer_size];

            // copy to buffer that adds padding if shorter than 16
            arr.CopyTo(buffer, 0);
            _header.B_SrcID = buffer;

            return this;
        }       
        public IPacketHeaderBuilder SetHopCount(int hopCount = 0) // if hop count was not provided, the default is 0
        {
            if (hopCount < 0 || hopCount > 255)
                throw new ArgumentException("Error initiating Hop Count. Hop Count must be between 0-255");

            // save as primitive and as bytes
            _header.HopCount = hopCount;
            _header.B_HopCount = (byte)hopCount;

            return this;
        }
        public IPacketHeaderBuilder SetSenderCounter(long senderCounter)
        {
            // save as primitive and as bytes
            _header.SenderCounter = senderCounter;

            int buffer_size = 8; // long size in bytes
            byte[] buffer = new byte[buffer_size];
            BinaryPrimitives.TryWriteInt64BigEndian(buffer, senderCounter);

            // convert to byte array
            _header.B_SenderCounter = buffer;

            return this;
        }
        public IPacketHeaderBuilder SetTimeToLive(long ttl)
        {
            if (ttl < 1 || ttl > _maxTTL) // <----------------------- FIX TEMP TLL IN FUTURE
                throw new ArgumentException("Error initiating TTL");

            // save as primitive and as bytes
            _header.TTL = ttl;

            int buffer_size = 8; // long size in bytes
            byte[] buffer = new byte[buffer_size];
            BinaryPrimitives.TryWriteInt64BigEndian(buffer, ttl);

            // convert to byte array
            _header.B_TTL = buffer;

            return this;
        }
        public IPacketHeaderBuilder SetMessageType(PacketType type)
        {
            if (type < PacketType.Heartbeat || type > PacketType.UserMessage)
                throw new ArgumentException("Error initiating Packet Type: Type not recognised");

            // save as primitive and as bytes
            _header.MsgType = type;
            _header.B_MsgType = (byte)type;

            return this;
        }
        public IPacketHeaderBuilder SetMessageID(string msgID)
        {
            if (msgID.IsWhiteSpace() || msgID.Length > _16bytes)
                throw new ArgumentException("Error initiating Message ID. The ID must be unique, under 16 characters and contain text");

            // save as primitive and as bytes
            _header.MsgID = msgID;

            // string values don't need Big Endian order because each letter is a single byte
            byte[] arr = Encoding.UTF8.GetBytes(msgID);

            int buffer_size = 16; // field's size in bytes
            byte[] buffer = new byte[buffer_size];

            // copy to buffer that adds padding if shorter than 16
            arr.CopyTo(buffer, 0);
            _header.B_MsgID = buffer;

            return this;
        }
        public IPacketHeaderBuilder SetDestinationID(string dstID)
        {
            if (dstID.IsWhiteSpace() || dstID.Length > _16bytes)
                throw new ArgumentException("Error initiating Destination ID. The ID must be under 16 characters and contain text");

            // save as primitive and as bytes
            _header.DstID = dstID;

            // string values don't need Big Endian order because each letter is a single byte
            byte[] arr = Encoding.UTF8.GetBytes(dstID);

            int buffer_size = 16; // field's size in bytes
            byte[] buffer = new byte[buffer_size];

            // copy to buffer that adds padding if shorter than 16
            arr.CopyTo(buffer, 0);
            _header.B_DstID = buffer;

            return this;
        }
        public IPacketHeaderBuilder SetPriority(int priority)
        {
            if (priority < 1 || priority > 10)
                throw new ArgumentException("Error initializing message priority. The priority must be between 1-10");

            // save as primitive and as bytes
            _header.Priority = priority;
            _header.B_Priority = (byte)priority;

            return this;
        }
        public IPacketHeaderBuilder SetLogicalClock(long logicalClock)
        {
            if (logicalClock < 0 || logicalClock > long.MaxValue)
                throw new ArgumentException($"Error initializing Logical Clock. The Logical Clock must be between 0- {long.MaxValue:e3}");

            // save as primitive and as bytes
            _header.LogicalClock = logicalClock;

            int buffer_size = 8; // long size in bytes
            byte[] buffer = new byte[buffer_size]; 
            BinaryPrimitives.TryWriteInt64BigEndian(buffer, logicalClock);

            // convert to byte array
            _header.B_LogicalClock = buffer;

            return this;
        }

        // build
        public PacketHeader BuildHeader()
        {
            _header.ByteArraysList.Clear();

            if (AreHeaderFieldsEmpty())
                throw new ArgumentNullException("Error building the Packet Header. One or more fields are empty.");

            // add all field in the correct order to the array list            
            _header.ByteArraysList.Add(new byte[] { _header.B_ProtocolVersion });
            _header.ByteArraysList.Add(_header.B_SrcID);
            _header.ByteArraysList.Add(new byte[] { _header.B_HopCount });
            _header.ByteArraysList.Add(_header.B_SenderCounter);
            _header.ByteArraysList.Add(_header.B_TTL);
            _header.ByteArraysList.Add(new byte[] { _header.B_MsgType });
            _header.ByteArraysList.Add(_header.B_MsgID);
            _header.ByteArraysList.Add(_header.B_DstID);
            _header.ByteArraysList.Add(new byte[] { _header.B_Priority });
            _header.ByteArraysList.Add(_header.B_LogicalClock);
            _header.ByteStream = Serializer.SerializeHeader(_header);

            if (_header.ByteStream == null)
                throw new Exception("Error. Couldn't serialize Packet Header.");

            return _header;
        }

        // types byte cant be null so no point checking
        private bool AreHeaderFieldsEmpty() => (_header.SrcID == null || _header.MsgID == null 
            || _header.DstID == null || _header.B_SenderCounter == null || _header.B_TTL == null 
            || _header.B_LogicalClock == null);
    }
}
