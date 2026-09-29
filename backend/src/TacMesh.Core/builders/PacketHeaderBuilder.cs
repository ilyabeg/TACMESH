using System.Buffers.Binary;
using System.Text;
using TacMesh.Core.interfaces;

namespace TacMesh.Core.builders
{
    public class PacketHeaderBuilder : IPacketHeaderBuilder
    {
        // temporarily put the fixed magic numbers here 
        public double LatestVersion { get; private set; } = 1.0;
        private long _maxTTL = 10000000; // in seconds

        // private working header
        private PacketHeader _header;

        // constructor
        public PacketHeaderBuilder(PacketHeader header) => _header = header;        

        // building methods
        public IPacketHeaderBuilder SetProtocolVersion(double p_version)
        {
            if (p_version != LatestVersion)
                throw new Exception($"Error initiating Protocol Version. Protocol Version must be up to date (v{LatestVersion:F1}).");

            // save as primitive and as bytes
            _header.ProtocolVersion = p_version;
            _header.B_ProtocolVersion = (byte)p_version;

            return this;
        }
        public IPacketHeaderBuilder SetSourceID(string srcID)
        {
            // save as primitive and as bytes
            _header.SrcID = srcID; // FIX THIS LATER (temp not checking if ok)

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
            if (ttl < 1 || ttl > _maxTTL)
                throw new Exception("Error initiating TTL.");

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
            // save as primitive and as bytes
            _header.MsgType = type;
            _header.B_MsgType = (byte)type;

            return this;
        }
        public IPacketHeaderBuilder SetMessageID(string msgID)
        {
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
                throw new Exception("Error initializing message priority. The priority must be between 1-10.");

            // save as primitive and as bytes
            _header.Priority = priority;
            _header.B_Priority = (byte)priority;

            return this;
        }
        public IPacketHeaderBuilder SetLogicalClock(long logicalClock)
        {
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
        public PacketHeader BuildPacketHeader()
        {
            // add all field in the correct order to the array list            
            _header.ArrayList.Add(new byte[] { _header.B_ProtocolVersion });
            _header.ArrayList.Add(_header.B_SrcID);
            _header.ArrayList.Add(new byte[] { _header.B_HopCount });
            _header.ArrayList.Add(_header.B_SenderCounter);
            _header.ArrayList.Add(_header.B_TTL);
            _header.ArrayList.Add(new byte[] { _header.B_MsgType });
            _header.ArrayList.Add(_header.B_MsgID);
            _header.ArrayList.Add(_header.B_DstID);
            _header.ArrayList.Add(new byte[] { _header.B_Priority });
            _header.ArrayList.Add(_header.B_LogicalClock);

            _header.SerializeFields();
            return _header;
        }
    }
}
