using System.Buffers.Binary;
using System.Text;

namespace TacMesh.Core.packet_related
{
    public class Deserializer
    {
        /// <summary>
        /// static method to deserialize any Packet Header byte stream into a PacketHeader object
        /// </summary>
        /// <param name="header_byte_stream"></param>
        /// <returns></returns>
        public static PacketHeader? DeserializeHeader(byte[] byteStream)
        {
            try
            {
                List<byte[]>? arrayList = CreateByteArraysList(byteStream);
                if (arrayList == null) return null;

                // --- single bits ---

                // byte number 0 of the first byte (which is also at index 0) is the protocol version
                double p_version = GetSpecificByte(arrayList, 0, 0);
                // byte number 0 of the byte at index 2 of the list (byte 17) is the hop count
                int hopCount = GetSpecificByte(arrayList, 2, 0);
                // byte number 0 of the byte at index 5 of the list (byte 34) is the packet type
                PacketType msgType = (PacketType)GetSpecificByte(arrayList, 5, 0);
                // byte number 0 of the byte at index 8 of the list (byte 67) is the priority
                int priority = GetSpecificByte(arrayList, 8, 0);

                // --- 64 bit (8 byte) numbers ---

                // the byte array from index 3 in the list is the sender counter
                long sender_counter = GetSpecificLong(arrayList, 3);
                // the byte array from index 4 in the list is the TTL
                long ttl = GetSpecificLong(arrayList, 4);
                // the byte array from index 9 in the list is the Loogical clock
                long logicalClock = GetSpecificLong(arrayList, 9);

                // --- string values ---

                // the byte array from index 1 in the list is the source ID            
                string srcID = GetSpecificString(arrayList, 1);
                // the byte array from index 6 in the list is the msg ID
                string msgID = GetSpecificString(arrayList, 6);
                // the byte array from index 7 in the list is the destination ID           
                string dstID = GetSpecificString(arrayList, 7);

                return PacketHeader.BuildPacketHeader(p_version, srcID, hopCount, sender_counter, ttl, msgType,
                    msgID, dstID, priority, logicalClock);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Couldn't Deserialize Packet Header due to: '{e.Message}'.");
                return null;
            }
        }

        // --- private deserialization helpers ---

        // each header field corresponds to it's size in bytes
        private static readonly int[] _array_size = { 1, 16, 1, 8, 8, 1, 16, 16, 1, 8 };
        private static List<byte[]>? CreateByteArraysList(byte[] byteStream)
        {
            // reject byte stream shorter than 76 bytes
            if (byteStream.Length < PacketHeader.HeaderSize) return null;

            // the starting index at which copying the array starts
            int start_index = 0, end_index = 0;

            List<byte[]> list = new List<byte[]>();

            for (int i = 0; i < _array_size.Length; i++)
            {
                // ending index of the copy array
                end_index += _array_size[i];

                // Range Operator (..): returns a new copy of the sub-array from index: start - (end-1),
                // without changing the original array
                byte[] field_bytes = byteStream[start_index..end_index];

                // next starting index starts at index + size of the last array
                start_index += _array_size[i];

                // add the corrisponding byte array of the field to the list
                list.Add(field_bytes);                
            }
            return list;
        }

        private static byte GetSpecificByte(List<byte[]> list, int array_indx, int byte_indx) => list[array_indx][byte_indx];
        private static long GetSpecificLong(List<byte[]> list, int array_indx)
        {
            byte[] bytes_array = list[array_indx];
            return BinaryPrimitives.ReadInt64BigEndian(bytes_array);
        }
        private static string GetSpecificString(List<byte[]> list, int array_indx)
        {
            byte[] bytes_array = list[array_indx];
            return Encoding.UTF8.GetString(bytes_array).TrimEnd('\0');
        }
    }
}
