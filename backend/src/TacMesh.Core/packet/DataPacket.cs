using System.Collections;
using System.Text;
using TacMesh.Core.utils.design;

namespace TacMesh.Core.packet
{
    public class DataPacket
    {
        public const int MAX_PACKET_SIZE = 1500; // TEMPORARILY PUT HERE MAX PACKET SIZE

        // packet fields
        public PacketHeader PacketHeader { get; private set; }
        public byte[] Payload { get; private set; }
        
        // constructor
        public DataPacket(PacketHeader packetHeader, byte[] payload)
        {
            PacketHeader = packetHeader;
            Payload = payload;
        }

        // format the packet
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();

            // title area
            str.AppendLine("╔════════════════════════════════════════════════════════════════════════╗");

            string title = $" DATA PACKET";
            str.AppendLine($"║{title.PadRight(ConsoleDesign._72_padRight)}║");

            str.AppendLine("╚════════════════════════════════════════════════════════════════════════╝");

            if (PacketHeader != null)
            {
                str.Append(PacketHeader.ToString());
            }

            // payload area
            int payloadBytes = Payload.Length;

            str.AppendLine("┌────────────────────────────────────────────────────────────────────────┐");
            string payloadTitle = $" PAYLOAD (Size: {payloadBytes} Bytes)";
            str.AppendLine($"│{payloadTitle.PadRight(ConsoleDesign._72_padRight)}│");
            str.AppendLine("├────────────────────────────────────────────────────────────────────────┤");

            if (payloadBytes == 0)
            {
                str.AppendLine($"│ {"[EMPTY]".PadRight(ConsoleDesign._70_padRight)} │");
            }
            else
            {
                // split payload string into lines of max 70 characters 
                int maxLineLen = 70;
                IEnumerable payloadChunks = Payload.Chunk(maxLineLen);

                foreach (var chunk in payloadChunks)
                {                    
                    str.AppendLine($"│ {chunk.ToString()!.PadRight(maxLineLen)} │");
                }
            }

            str.AppendLine("└────────────────────────────────────────────────────────────────────────┘");

            return str.ToString();
        }

        /// <summary>
        /// Overriden Equals() to treat different packet references with the same 
        /// MsgID Guid as the same exact packets
        /// </summary>
        /// <param name="obj"></param>
        /// <returns>If this packet is equal to the obj packet</returns>
        public override bool Equals(object? obj)
        {
            if (obj is DataPacket other) {
                return PacketHeader.MsgID == other.PacketHeader.MsgID;
            }
            return false;
        }

        /// <summary>
        /// Overriden GetHashCode() for comparing
        /// </summary>
        /// <returns>The hashcode of the packet message id</returns>
        public override int GetHashCode() => PacketHeader.MsgID.GetHashCode();
    }
}
