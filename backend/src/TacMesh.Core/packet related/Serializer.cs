namespace TacMesh.Core.packet_related
{
    public class Serializer
    {
        /// <summary>
        /// Method that serializes all of the header fields into a byte stream
        /// </summary>
        /// <returns>PacketHeader byte stream if successful, null otherwise</returns>
        public static byte[]? SerializeHeader(PacketHeader header)
        {
            try
            {
                // byte stream exactly 76 bytes
                byte[] byteStream = new byte[PacketHeader.HeaderSize];

                int offset = 0;
                foreach (byte[] array in header.ByteArraysList)
                {
                    array.CopyTo(byteStream, offset); // copy each array to the stream from the starting position at offset
                    offset += array.Length;

                    // stop early if fields are larger than 76 bytes
                    if (offset > PacketHeader.HeaderSize) throw new Exception("Header size Overflow");
                }
                return byteStream;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Couldn't Serialize Header due to: '{e.Message}'.");
                return null;
            }
        }
    }
}
