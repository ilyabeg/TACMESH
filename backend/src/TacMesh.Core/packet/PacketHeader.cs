using System.Text;
using TacMesh.Core.utils.design;

namespace TacMesh.Core.packet
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
        public const int HEADER_SIZE = 76; // bytes

        #region TEMPORARY MAGIC NUMBERS
        public const int DEFAULT_MIN_VALUE = 0;
        public const byte PROTOCOL_VERSION = 1;
        public const long DEFAULT_TTL = 5000; // representing ms (default 5 sec)
        public const byte MIN_PRIORITY = 1; 
        public const byte MAX_PRIORITY = 10;
        public const byte MAX_HOP = 255;
        #endregion

        // current legal string length 16 bytes
        // CAN BE CHANGED AT ANY TIME OR BE PROVIDED FROM AN OTHER SOURCE,
        // SUCH AS THE CONFIG FILE OR MISSION PACKAGE.
        public const int LEGAL_STRING_LENGTH = 16;

        // a constant destination identifier that specifies that a packet 
        // with this dest id is ment for anybody
        //
        public const string ALL_SPECIFIER = "@all";


        // Header fields in order of transmision
        #region Header Fields
        public byte ProtocolVersion // 1 byte
        { 
            get; 
            private set
            {
                if (value != PROTOCOL_VERSION)
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

                if (string.Equals(value, ALL_SPECIFIER))
                    throw new ArgumentException("Error initiating Source ID. Invalid ID.");

                // check if actual byte length and not the amount of characters
                int byteCount = Encoding.UTF8.GetByteCount(value);

                if (byteCount > LEGAL_STRING_LENGTH)
                    throw new ArgumentException($"Error initiating Source ID. The ID must be {LEGAL_STRING_LENGTH} bytes or less");
                
                field = value;
            }
        }

        public byte HopCount // 1 byte
        {
            get;
            private set
            {
                if (value < DEFAULT_MIN_VALUE || value > MAX_HOP)
                    throw new ArgumentException($"Error initiating Hop Count. Hop Count must be between {DEFAULT_MIN_VALUE}-{MAX_HOP}");
                field = value;
            }
        }

        public long SenderCounter // 8 byte
        {
            get;
            private set
            {                
                if (value < DEFAULT_MIN_VALUE || value > long.MaxValue)
                    throw new ArgumentException("Error initiating Sender Counter.");
                field = value;
            }
        }

        public long TTL // 8 byte
        {
            get;
            private set
            {
                if (value < DEFAULT_MIN_VALUE || value > long.MaxValue)
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

                if (byteCount > LEGAL_STRING_LENGTH)
                    throw new ArgumentException($"Error initiating Destination ID. The ID must be {LEGAL_STRING_LENGTH} bytes or less");

                field = value;
            }
        }

        public byte Priority // 1 byte
        {
            get;
            private set
            {
                if (value < MIN_PRIORITY || value > MAX_PRIORITY)
                    throw new ArgumentException($"Error initializing message priority. The priority must be between {MIN_PRIORITY}-{MAX_PRIORITY}");
                field = value;
            }
        }

        public long LogicalClock // 8 byte
        {
            get;
            private set
            {
                if (value < DEFAULT_MIN_VALUE || value > long.MaxValue)
                    throw new ArgumentException($"Error initializing Logical Clock. The Logical Clock must be between {DEFAULT_MIN_VALUE}-{long.MaxValue:e3}");
                field = value;
            }
        }
        #endregion

        // Constructor build the packet header with the specified fields if they are included. non included
        // fields default to the constant default values
        //
        public PacketHeader(
            PacketType msgType,
            string srcID, 
            string dstID,
            Guid msgID,
            byte priority = MIN_PRIORITY,
            long logicalClock = DEFAULT_MIN_VALUE,
            long ttl = DEFAULT_TTL,
            long senderCounter = DEFAULT_MIN_VALUE,
            byte p_version = PROTOCOL_VERSION,
            byte hopCount = DEFAULT_MIN_VALUE)
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
            SenderCounter = senderCounter;
        }

        /// <summary>
        /// Used to increment the hop count when forwarding messages.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void IncrementHopCount()
        {
            if (HopCount == MAX_HOP)
                throw new InvalidOperationException($"Cannot increment Hop Count. Hop Count is already at maximum value of {MAX_HOP}");
            HopCount++;
        }

        /// <summary>
        /// Returns a string representation of the Packet Header in an organized block
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();

            str.AppendLine("┌────────────────────────────────────────────────────────────────────────┐");

            string title = $" PACKET HEADER (Size: {HEADER_SIZE} Bytes)";
            str.AppendLine($"│{title.PadRight(ConsoleDesign._72_padRight)}│");

            str.AppendLine("├────────────────────┬──────────┬────────────────────────────────────────┤");
            str.AppendLine("│ Field              │ Size     │ Value                                  │");
            str.AppendLine("├────────────────────┼──────────┼────────────────────────────────────────┤");

            str.AppendLine(FormatHeaderRow("Protocol Version", "1 B", ProtocolVersion.ToString()));
            str.AppendLine(FormatHeaderRow("Message Type", "1 B", MsgType.ToString()));
            str.AppendLine(FormatHeaderRow("Message ID", "16 B", MsgID.ToString()));
            str.AppendLine(FormatHeaderRow("Source ID", "16 B", SrcID));
            str.AppendLine(FormatHeaderRow("Destination ID", "16 B", DstID));
            str.AppendLine(FormatHeaderRow("Hop Count", "1 B", HopCount.ToString()));
            str.AppendLine(FormatHeaderRow("Logical Clock", "8 B", LogicalClock.ToString()));
            str.AppendLine(FormatHeaderRow("TTL", "8 B", $"{TTL}s"));
            str.AppendLine(FormatHeaderRow("Priority", "1 B", Priority.ToString()));
            str.AppendLine(FormatHeaderRow("Sender Counter", "8 B", SenderCounter.ToString()));

            str.AppendLine("└────────────────────┴──────────┴────────────────────────────────────────┘");

            // TO DO: visually see all bytes

            return str.ToString();
        }

        // format a header row specificaly by the field
        private string FormatHeaderRow(string fieldName, string size, string value)
        {
            return $"│ {fieldName,ConsoleDesign._18_padRight} │ {size,ConsoleDesign._8_padRight} │ {value,ConsoleDesign._38_padRight} │";
        }
    }
}
