using System.Text;
using TacMesh.Core.builders;

namespace TacMesh.Core
{
    // type of the packet
    public enum PacketType : byte
    {
        Heartbeat,
        LinkState,
        UserMessage
    }

    /// <summary>
    /// The Packet Header class contains all of the Header fields as byte and primitive types representations.
    /// </summary>
    public class PacketHeader
    {
        // fixed global header size
        public const int HeaderSize = 76; // bytes

        #region TEMPORARY MAGIC NUMBERS
        // THESE ARE TEMPORARY MAGIC NUMBERS AND I KNOW IT IS WRONG.
        // UNTILL I ADD THE MISSION PACKAGE OR GET IT FROM CONFIG FILE LATER,
        // THESS WILL BE DEFAULT CONFIGURATIONS STRICTLY FOR TESTING.
        private readonly byte _currentProtocolVersion = 1;
        private readonly long _maxTTL = 10000000; // in seconds
        private readonly long _minTTL = 1;
        private readonly byte _minPriority = 1; 
        private readonly byte _maxPriority = 10;
        private readonly byte _minHopCount = 0;
        private readonly byte _maxHopCount = 255;
        private readonly long _minLogicalClock = 0;
        private readonly long _maxLogicalClock = long.MaxValue;
        #endregion

        // current legal string length 16 bytes
        // CAN BE CHANGED AT ANY TIME OR BE PROVIDED FROM AN OTHER SOURCE,
        // SUCH AS THE CONFIG FILE OR MISSION PACKAGE.
        public const int LegalStringLength = 16;


        // Header fields in order of transmision
        #region Header Fields
        public byte ProtocolVersion // 1 byte
        { 
            get; 
            set
            {
                if (value != _currentProtocolVersion)
                    throw new ArgumentException("Unknown protocol version");
                field = value;
            }
        }

        public string SrcID // 16 byte
        { 
            get;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Error initiating Source ID. The ID must contain text");
                
                // check if actual byte length and not the amount of characters
                int byteCount = Encoding.UTF8.GetByteCount(value);

                if (byteCount > LegalStringLength)
                    throw new ArgumentException($"Error initiating Source ID. The ID must be {LegalStringLength} bytes or less");
                
                field = value;
            }
        }

        public byte HopCount // 1 byte
        {
            get;
            set
            {
                if (value < _minHopCount || value > _maxHopCount)
                    throw new ArgumentException($"Error initiating Hop Count. Hop Count must be between {_minHopCount}-{_maxHopCount}");
                field = value;
            }
        }

        public long SenderCounter // 8 byte
        {
            get;
            set
            {
                // THERE IS NO CHECK FOR THIS FIELDS, AND I KNOW THIS IS WRONG.
                // I JUST DIDN'T GET TO LEARN YET WHAT THIS FIELD DOES EXACTLY, THUS I
                // DON'T HAVE A CHECK YET. THIS IS STRICTLY TEMPORARY AND WILL BE FIXED
                // IN THE FUTURE.

                //if (value ...)
                //    throw new ArgumentException("Error initiating Sender Counter.");
                field = value;
            }
        }

        public long TTL // 8 byte
        {
            get;
            set
            {
                // AGAIN, MAX TTL IS YET TO BE DESIDED AND THIS MAGIC NUMBER IS STRICTLY TEMPORARY.

                if (value < _minTTL || value > _maxTTL)
                    throw new ArgumentException("Error initiating TTL");
                field = value;
            }
        }

        public PacketType MsgType // 1 byte
        {
            get;
            set
            {
                if (value < PacketType.Heartbeat || value > PacketType.UserMessage)
                    throw new ArgumentException("Error initiating Packet Type: Unkown Packet Type");
                field = value;
            }
        }

        public string MsgID // 16 byte
        {
            get;
            set
            {
                // VERY IMPORTANT NOTE: THERES IS NO CHECK FOR A UNIQUE ID BECAUSE I HAVEN'T DECIDED 
                // YET ON HOW I AM GOING TO MAKE A UNIQUE ID FOR EVERY MESSAGE. FOR NOW I ONLY CHECK THE 
                // LENGTH, AND THIS IS STRICTLY TEMPORARY FOR TESTING REASONS ONLY. I KNOW THIS IS 
                // WRONG AND I DO NOT INTEND OF LEAVING IT THIS WAY IN THE FINISHED PRODUCT.

                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Error initiating Message ID. The ID must contain text");

                // check the actual byte length and not the amount of characters
                int byteCount = Encoding.UTF8.GetByteCount(value);

                if (byteCount != LegalStringLength)
                    throw new ArgumentException($"Error initiating Message ID. The ID must be exactly {LegalStringLength} bytes");

                field = value;
            }
        }

        public string DstID // 16 byte
        {
            get;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Error initiating Destination ID. The ID must contain text");

                // check if actual byte length and not the amount of characters
                int byteCount = Encoding.UTF8.GetByteCount(value);

                if (byteCount > LegalStringLength)
                    throw new ArgumentException($"Error initiating Destination ID. The ID must be {LegalStringLength} bytes or less");

                field = value;
            }
        }

        public byte Priority // 1 byte
        {
            get;
            set
            {
                // AGAIN, THESE ARE STRICTLY ONLY TEMPORARY VALUES AND WILL BE CHANGED.

                if (value < _minPriority || value > _maxPriority)
                    throw new ArgumentException($"Error initializing message priority. The priority must be between {_minPriority}-{_maxPriority}");
                field = value;
            }
        }

        public long LogicalClock // 8 byte
        {
            get;
            set
            {
                if (value < _minLogicalClock || value > _maxLogicalClock)
                    throw new ArgumentException($"Error initializing Logical Clock. The Logical Clock must be between {_minLogicalClock}-{_maxLogicalClock:e3}");
                field = value;
            }
        }
        #endregion

        // dump all at once. get a full header
        public static PacketHeader BuildPacketHeader(
            byte p_version, PacketType msgType,
            string msgID, string srcID, string dstID,
            byte hopCount, long logicalClock,
            long ttl, byte priority, long sender_counter)
        {
            PacketHeaderBuilder builder = new PacketHeaderBuilder(new PacketHeader());

            builder.SetProtocolVersion(p_version)
            .SetMessageType(msgType)
            .SetMessageID(msgID)
            .SetSourceID(srcID)
            .SetDestinationID(dstID)
            .SetHopCount(hopCount)
            .SetLogicalClock(logicalClock)
            .SetTimeToLive(ttl)
            .SetPriority(priority)
            .SetSenderCounter(sender_counter);

            return builder.BuildHeader();
        }

        /// <summary>
        /// Prints out Packet Header in an orginized block
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();

            str.AppendLine("---------------------------------");
            str.AppendLine("         HEADER FIELDS:");
            str.AppendLine("---------------------------------\n");
            str.AppendLine($"Protocol Version: {ProtocolVersion}");
            str.AppendLine($"Message Type:     {MsgType}");
            str.AppendLine($"MsgID:            {MsgID}");
            str.AppendLine($"SrcID:            {SrcID}");
            str.AppendLine($"DstID:            {DstID}");
            str.AppendLine($"Hop Count:        {HopCount}");
            str.AppendLine($"Logical Clock:    {LogicalClock}");
            str.AppendLine($"TTL:              {TTL}");
            str.AppendLine($"Priority:         {Priority}");
            str.AppendLine($"Sender Count:     {SenderCounter}");

            // TO DO: visually see all bytes

            return str.ToString();
        }
    }
}
