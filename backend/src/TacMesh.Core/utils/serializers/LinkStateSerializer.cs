using TacMesh.Core.interfaces;
using TacMesh.Core.packet;

namespace TacMesh.Core.utils.serializers
{
    public class LinkStateSerializer : IByteSerializer<List<string>>
    {
        /// <summary>
        /// Serializes the provided link state into a byte array
        /// </summary>
        /// <param name="input">The provided link state to serialize</param>
        /// <returns>The link state as a byte array</returns>
        public byte[] Serialize(List<string> linkState)
        {
            int length = linkState.Count; // determine the length of the link state list (4 bytes)

            // buffer to hold all the data
            // (4 bytes for the length of the list + the length * the size of a legal string)
            // 
            byte[] byteBuffer = new byte[ByteCount._4bytes + length * PacketHeader.LEGAL_STRING_LENGTH];
            
            ByteWriter byteWriter = new ByteWriter(byteBuffer); // byte writter to write all the data

            // serialize the length of the list before appending the
            // actual list serialization
            //
            byteWriter.WriteBytes(length);

            foreach (string neighbor in linkState)
            {
                // each neighbor id must be exactly the legal bytes length.
                // the packet header bounds each node id to a specific byte length, so
                // no need to worry if it's bigger than the strict length because it can't be
                //
                byteWriter.WriteBytes(neighbor, PacketHeader.LEGAL_STRING_LENGTH);
            }

            // return the final result
            //
            return byteBuffer;
        }

        /// <summary>
        /// Deserializes the provided link state from a byte array
        /// </summary>
        /// <param name="input">The provided link state to deserialize</param>
        /// <returns>The deserialized link state</returns>
        public List<string> Deserialize(byte[] linkStateBytes)
        {
            ByteReader byteReader = new ByteReader(linkStateBytes); // byte writter to read all the data

            // determine the length of the link state list (first 4 bytes)
            //
            int length = byteReader.ReadInt();

            // now deserialize each neighbor id and append to the 
            // result list
            //
            List<string> linkState = new List<string>();

            for (int i = 0; i < length; i++)
            {
                // each neighbor id must be exactly the legal bytes length
                //
                string neighbor = byteReader.ReadString(PacketHeader.LEGAL_STRING_LENGTH);
                linkState.Add(neighbor);
            }

            // return the final result
            //
            return linkState;
        }
    }
}
