using System.Buffers.Binary;
using System.Text;
using TacMesh.Core.builders;

namespace TacMesh.Core
{
    // type of the packet
    public enum PacketType
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
        // global header size
        public int HeaderSize { get; set; } = 76;

        // Header fields in order of transmision (as byte and primitive representations)
        public byte B_ProtocolVersion { get; set; } // 1 byte
        public double ProtocolVersion { get; set; } 

        public byte[] B_SrcID { get; set; } // 16 byte
        public string SrcID { get; set; } 

        public byte B_HopCount { get; set; } // 1 byte        
        public int HopCount { get; set; }       

        public byte[] B_SenderCounter { get; set; } // 8 byte
        public long SenderCounter { get; set; }

        public byte[] B_TTL { get; set; } // 8 byte
        public long TTL { get; set; }

        public byte B_MsgType { get; set; } // 1 byte
        public PacketType MsgType { get; set; }

        public byte[] B_MsgID { get; set; } // 16 byte
        public string MsgID { get; set; }

        public byte[] B_DstID { get; set; } // 16 byte
        public string DstID { get; set; }

        public byte B_Priority { get; set; } // 1 byte
        public int Priority { get; set; }

        public byte[] B_LogicalClock { get; set; } // 8 byte
        public long LogicalClock { get; set; }

        // working byte stream of all the fields (76 bytes in total)
        private readonly int _headerSize = 76;
        public byte[] ByteStream { get; private set; }

        // private List holding all of the byte arrays
        public List<byte[]> ArrayList { get; private set; }

        // Constructor
        public PacketHeader()
        {
            ArrayList = new List<byte[]>();
            ByteStream = new byte[_headerSize];
        }

        /// <summary>
        /// Method that serializes all of the header fields into a byte stream
        /// </summary>
        /// <returns>True if Serialization process was successful. Flase if Serialization process was un-successful</returns>
        public bool SerializeFields()
        {
            try
            {
                int offset = 0;
                foreach (byte[] array in ArrayList)
                {
                    array.CopyTo(ByteStream, offset); // copy each array to the stream from the starting position at offset
                    offset += array.Length;

                    // stop early if fields are larger than 76 bytes
                    if (offset > _headerSize) throw new Exception("Header size Overflow");
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Couldn't Serialize Header due to: '{e.Message}'.");
                return false;
            }
        }

        /// <summary>
        /// static method to deserialize any Packet Header byte stream
        /// </summary>
        /// <param name="header_byte_stream"></param>
        /// <returns></returns>
        public static PacketHeader Deserialize(List<byte[]> arrayList)
        {
            // single bits

            // bit number 0 of the first byte (which is also at index 0) is the protocol version
            int array_indx = 0, bit_indx = 0;
            double p_version = arrayList[array_indx][bit_indx];

            // bit number 0 of the byte at index 2 of the list (byte 17) is the hop count
            array_indx = 2;
            int hopCount = arrayList[array_indx][bit_indx];

            // bit number 0 of the byte at index 5 of the list (byte 34) is the packet type
            array_indx = 5;
            PacketType msgType = (PacketType)arrayList[array_indx][bit_indx];

            // bit number 0 of the byte at index 8 of the list (byte 67) is the priority
            array_indx = 8;
            int priority = arrayList[array_indx][bit_indx];

            // 64 bit (8 byte) numbers

            // the byte array from index 3 in the list is the sender counter
            array_indx = 3;
            byte[] b_senderCounter = arrayList[array_indx];
            long sender_counter = BinaryPrimitives.ReadInt64BigEndian(b_senderCounter);

            // the byte array from index 4 in the list is the TTL
            array_indx = 4;
            byte[] b_ttl = arrayList[array_indx];
            long ttl = BinaryPrimitives.ReadInt64BigEndian(b_ttl);

            // the byte array from index 9 in the list is the Loogical clock
            array_indx = 9;
            byte[] b_logicalClock = arrayList[array_indx];
            long logicalClock = BinaryPrimitives.ReadInt64BigEndian(b_logicalClock);

            // string values

            // the byte array from index 1 in the list is the source ID
            array_indx = 1;
            byte[] b_srcID = arrayList[array_indx];
            string srcID = Encoding.UTF8.GetString(b_srcID).TrimEnd('\0');

            // the byte array from index 6 in the list is the msg ID
            array_indx = 6;
            byte[] b_msgID = arrayList[array_indx];
            string msgID = Encoding.UTF8.GetString(b_msgID).TrimEnd('\0');

            // the byte array from index 7 in the list is the destination ID
            array_indx = 7;
            byte[] b_dstID = arrayList[array_indx];
            string dstID = Encoding.UTF8.GetString(b_dstID).TrimEnd('\0');

            // building the actual header
            PacketHeaderBuilder builder = new PacketHeaderBuilder(new PacketHeader());

            try
            {
                builder.SetProtocolVersion(p_version)
                .SetSourceID(srcID)
                .SetHopCount(hopCount)
                .SetSenderCounter(sender_counter)
                .SetTimeToLive(ttl)
                .SetMessageType(msgType)
                .SetMessageID(msgID)
                .SetDestinationID(dstID)
                .SetPriority(priority)
                .SetLogicalClock(logicalClock);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception while building deserialized header: '{e.Message}'");
            }

            return builder.BuildPacketHeader();
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
            str.AppendLine($"Protocol Version: {ProtocolVersion:F1} (1 bytes)");
            str.AppendLine($"SrcID:            {SrcID} ({B_SrcID.Length} bytes)");
            str.AppendLine($"Hop Count:        {HopCount} (1 bytes)");
            str.AppendLine($"Sender Count:     {SenderCounter} ({B_SenderCounter.Length} bytes)");
            str.AppendLine($"TTL:              {TTL} ({B_TTL.Length} bytes)");
            str.AppendLine($"Message Type:     {MsgType} (1 bytes)");
            str.AppendLine($"MsgID:            {MsgID} ({B_MsgID.Length} bytes)");
            str.AppendLine($"DstID:            {DstID} ({B_DstID.Length} bytes)");
            str.AppendLine($"Priority:         {Priority} (1 bytes)");
            str.AppendLine($"Logical Clock:    {LogicalClock} ({B_LogicalClock.Length} bytes)");

            // visually see all bytes
            foreach (byte[] array in ArrayList)
            {
                Console.WriteLine("BYTES " + array.Length + ":");
                foreach (byte b in array)
                {
                    Console.Write(b + " ");
                }
                Console.WriteLine("\n");
            }

            return str.ToString();
        }
    }
}
