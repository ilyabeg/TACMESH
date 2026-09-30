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
        public byte[]? ByteStream { get; set; }

        // private List holding all of the byte arrays
        public List<byte[]> ByteArraysList { get; private set; }

        // Constructor
        public PacketHeader()
        {
            ByteArraysList = new List<byte[]>();
        }        

        // dump all at once. get a full header
        public static PacketHeader? BuildPacketHeader(
            double p_version, string srcID, 
            int hopCount, long sender_counter, 
            long ttl, PacketType msgType, 
            string msgID, string dstID,
            int priority, long logicalClock)
        {
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

                return builder.BuildHeader();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
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
            foreach (byte[] array in ByteArraysList)
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
