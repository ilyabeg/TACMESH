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
            private set
            {
                if (value != _currentProtocolVersion)
                    throw new ArgumentException("Unknown protocol version");
                field = value;
            }
        }

        public string SrcID // 16 byte
        { 
            get;
            private set
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
            private set
            {
                if (value < _minHopCount || value > _maxHopCount)
                    throw new ArgumentException($"Error initiating Hop Count. Hop Count must be between {_minHopCount}-{_maxHopCount}");
                field = value;
            }
        }

        public long SenderCounter // 8 byte
        {
            get;
            private set
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
            private set
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
            private set
            {
                if (value < PacketType.Heartbeat || value > PacketType.UserMessage)
                    throw new ArgumentException("Error initiating Packet Type: Unkown Packet Type");
                field = value;
            }
        }

        public Guid MsgID // 16 byte
        {
            get;
            private set
            {
                if (value == Guid.Empty)
                    throw new ArgumentException("Error initiating Message ID. The ID must be a valid GUID");
                field = value;
            }
        }

        public string DstID // 16 byte
        {
            get;
            private set
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
            private set
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
            private set
            {
                if (value < _minLogicalClock || value > _maxLogicalClock)
                    throw new ArgumentException($"Error initializing Logical Clock. The Logical Clock must be between {_minLogicalClock}-{_maxLogicalClock:e3}");
                field = value;
            }
        }
        #endregion

        // Constructor NEEDS all fields at once, no way to make header with a missing field
        public PacketHeader(
            byte p_version, PacketType msgType,
            Guid msgID, string srcID, string dstID,
            byte hopCount, long logicalClock,
            long ttl, byte priority, long sender_counter)
        {
            ProtocolVersion = p_version;
            MsgType = msgType;
            MsgID = msgID;
            SrcID = srcID;
            DstID = dstID;
            HopCount = hopCount;
            LogicalClock = logicalClock;
            TTL = ttl;
            Priority = priority;
            SenderCounter = sender_counter;
        }

        /// <summary>
        /// Used to increment the hop count when forwarding messages.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void IncrementHopCount()
        {
            if (HopCount == _maxHopCount)
                throw new InvalidOperationException($"Cannot increment Hop Count. Hop Count is already at maximum value of {_maxHopCount}");
            HopCount++;
        }


        // IMPORTANT NODE: UPDATERS FOR SENDER COUNTER, TTL, AND LOGICAL CLOCK HAVE NOT BEEN HANDLED YET
        // AND I AM AWARE, AND WILL FIX. THIS IS BECAUSE I DIDN'T DECIDE YET HOW I AM GOING TO IMPLEMENT.
        // THIS IS AN INITIAL IMPLEMENTATION AND I DON'T PLAN OF HAVING IT LIKE THIS IN THE FINAL VERSION.


        /// <summary>
        /// Returns a string representation of the Packet Header in an organized block
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
