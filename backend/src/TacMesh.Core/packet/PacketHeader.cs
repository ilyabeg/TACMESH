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
        // THESE ARE TEMPORARY MAGIC NUMBERS AND I KNOW IT IS WRONG.
        // UNTILL I ADD THE MISSION PACKAGE OR GET IT FROM CONFIG FILE LATER,
        // THESS WILL BE DEFAULT CONFIGURATIONS STRICTLY FOR TESTING.
        public const byte PROTOCOL_VERSION = 1;
        public const long MAX_TTL = 10000000; // in seconds
        public const long MIN_TTL = 1;
        public const byte MIN_PRIORITY = 1; 
        public const byte MAX_PRIORITY = 10;
        public const byte DEFAULT_HOP = 0;
        public const byte MAX_HOP = 255;
        public const long MIN_LOGICAL_CLOCK = 0;
        public const long MAX_LOGICAL_CLOCK = long.MaxValue;
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
                if (value < DEFAULT_HOP || value > MAX_HOP)
                    throw new ArgumentException($"Error initiating Hop Count. Hop Count must be between {DEFAULT_HOP}-{MAX_HOP}");
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

                if (value < MIN_TTL || value > MAX_TTL)
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
                if (value < MIN_LOGICAL_CLOCK || value > MAX_LOGICAL_CLOCK)
                    throw new ArgumentException($"Error initializing Logical Clock. The Logical Clock must be between {MIN_LOGICAL_CLOCK}-{MAX_LOGICAL_CLOCK:e3}");
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
            if (HopCount == MAX_HOP)
                throw new InvalidOperationException($"Cannot increment Hop Count. Hop Count is already at maximum value of {MAX_HOP}");
            HopCount++;
        }


        // IMPORTANT NOTE: UPDATERS FOR SENDER COUNTER, TTL, AND LOGICAL CLOCK HAVE NOT BEEN HANDLED YET
        // AND I AM AWARE, AND WILL FIX. THIS IS BECAUSE I DIDN'T DECIDE YET HOW I AM GOING TO IMPLEMENT.
        // THIS IS AN INITIAL IMPLEMENTATION AND I DON'T PLAN OF HAVING IT LIKE THIS IN THE FINAL VERSION.


        /// <summary>
        /// Initializes a PacketHeader with default values except mandatory ones
        /// </summary>
        /// <param name="msgType"></param>
        /// <param name="srcID"></param>
        /// <param name="dstID"></param>
        /// <returns></returns>
        public static PacketHeader InitHeader(PacketType msgType, string srcID, string dstID)
        {
            // Note: TEMPORARILY use magic numbers (will be changed)
            //
            return new PacketHeader(
                p_version: 1,
                msgType: msgType,
                msgID: Guid.NewGuid(),
                srcID: srcID,
                dstID: dstID,
                hopCount: 0,
                logicalClock: 0,
                ttl: 15000, // ms
                priority: 1,
                sender_counter: 0
            );
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
