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
        // fixed global header size
        public static readonly int HeaderSize = 76; // bytes

        // Header fields in order of transmision (as byte and primitive representations)
        #region Header Fields
        public byte ProtocolVersion { get; private set; } // 1 byte
        public string SrcID { get; private set; }  // 16 byte
        public int HopCount { get; private set; } // 1 byte
        public long SenderCounter { get; private set; } // 8 byte
        public long TTL { get; private set; } // 8 byte
        public PacketType MsgType { get; private set; } // 1 byte
        public string MsgID { get; private set; } // 16 byte
        public string DstID { get; private set; } // 16 byte
        public int Priority { get; private set; }  // 1 byte
        public long LogicalClock { get; private set; }  // 8 byte
        #endregion

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
            str.AppendLine($"Protocol Version: {ProtocolVersion:F1}");
            str.AppendLine($"SrcID:            {SrcID}");
            str.AppendLine($"Hop Count:        {HopCount}");
            str.AppendLine($"Sender Count:     {SenderCounter}");
            str.AppendLine($"TTL:              {TTL}");
            str.AppendLine($"Message Type:     {MsgType}");
            str.AppendLine($"MsgID:            {MsgID}");
            str.AppendLine($"DstID:            {DstID}");
            str.AppendLine($"Priority:         {Priority}");
            str.AppendLine($"Logical Clock:    {LogicalClock}");

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
