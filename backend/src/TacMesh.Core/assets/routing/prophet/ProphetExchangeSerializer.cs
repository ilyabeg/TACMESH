using System.Collections.Concurrent;
using TacMesh.Core.interfaces;
using TacMesh.Core.utils.serializers;

namespace TacMesh.Core.assets.routing.prophet
{
    /// <summary>
    /// PRoPHET routing protocol's routing exchange packet serializer.
    /// PRoPHET predictability vector exchange phase requires the nodes to
    /// exchange their internal predictability vectors upon discovering new neighbors
    /// </summary>
    public class ProphetExchangeSerializer : IByteSerializer<ConcurrentDictionary<string, double>>
    {
        // IMPORTANT NOTE: serialization includes starting with 4 bytes of the actual 
        // probability vector length. each entry of a NodeID and its delivery
        // probability consists of 16 bytes for the node identifier and ** 1 BYTE **
        // for its delivery probability! the reason for this being that if you
        // were to serialize each double into 8 bytes, the probability vector exchanges
        // would overflow and be extremely large.
        //
        // example: probability vector exchange of 100 nodes would have been ( 16 bytes for
        // the identifier + 8 more bytes for the double probability ) times 100 = 2,400 bytes!!
        //
        // the solution is to simply translate each probability (which is: 0 <= probability <= 1)
        // to a single BYTE representation. P = 1.0 -> P_Byte = 255, P = 0.0 -> P_Byte = 0
        //
        // the exact formula is: Pr_Byte = (Byte)Round(Pr * 255). and vice versa: Pr = Pr_Byte / 255
        //
        // example:
        // Pr = 0.75 -> Pr_Byte = (Byte) Round(0.75 * 255) = (Byte) Round(191.25) = 191
        // Pr_Byte = 191 -> Pr = 191 / 255 = 0.7490 (difference from original is less than 0.001)
        //
        // this solution transmits 1 byte instead of 8 meaning 1/8 of the original data (12.5%),
        // which means 87.5% of the network bandwith is saved, with a maximum error of ~0.00196!
        //
        // error calculation:
        // the step size is the integer steps between each infinite double decimal: Step_Size = 1/255 = ~0.00392
        // any double decimal number is at most 0.5 away from an integer number. meaning the max error is:
        // Max_Error = Step_Size / 2 = 0.00392 / 2 = ~0.00196
        //
        // which means the maximum possible error is guaranteed to be less than +/- 0.002.
        

        /// <summary>
        /// Serializes the probability vector into bytes
        /// </summary>
        /// <param name="probabilityVector">The probability vector</param>
        /// <returns>The serialized probability vector as bytes</returns>
        public byte[] Serialize(ConcurrentDictionary<string, double> probabilityVector)
        {
            // current vector snapshot in case it changes during runtime
            //
            KeyValuePair<string, double>[] currVector = probabilityVector.ToArray();
            int length = currVector.Length; // determine the payload length (first 4 bytes)

            // buffer to hold all of the routing exchange bytes
            // first 4 bytes = vector length
            // then: for every kvp in the vector - 16 bytes of string id + 1 byte for the probability
            //
            byte[] buffer = new byte[ByteCount._4bytes + (ByteCount._16bytes + ByteCount._1byte) * length];

            ByteWriter byteWriter = new ByteWriter(buffer); // byte writer to write all the data

            // serialize the length of the dict before appending the
            // actual dict serialization
            //
            byteWriter.WriteBytes(length);

            foreach (KeyValuePair<string, double> kvp in currVector)
            {
                string nodeId = kvp.Key;
                double Pr = kvp.Value;
                
                byte Pr_Byte = (byte) Math.Round(Pr * 255); // get the corresponding byte for the probability

                // write the information to the byte writer
                //
                byteWriter.WriteBytes(nodeId, ByteCount._16bytes);
                byteWriter.WriteBytes(Pr_Byte);
            }

            // return the final result
            //
            return buffer;
        }

        /// <summary>
        /// Deserializes the probability vector bytes into a dictionary
        /// </summary>
        /// <param name="vectorBytes">The probability vector bytes</param>
        /// <returns>The deserialized probability vector bytes as a concurrent dictionary</returns>
        public ConcurrentDictionary<string, double> Deserialize(byte[] vectorBytes)
        {
            ByteReader byteReader = new ByteReader(vectorBytes); // byte reader to read all the data

            // deserialize the length of the dict before deserializing the
            // actual information
            //
            int length = byteReader.ReadInt();

            // deserialize each node id and it's delivery probability and 
            // append to the probability vector
            //
            ConcurrentDictionary<string, double> probabilityVector = new();

            for (int i = 0; i < length; i++)
            {
                // read the node id and it's probability in order
                //
                string nodeId = byteReader.ReadString(ByteCount._16bytes);
                byte Pr_Byte = byteReader.ReadByte();

                double Pr = Pr_Byte / 255.0; // get the corrisponding probability from the byte

                // add the info to the dictionary
                //
                probabilityVector.TryAdd(nodeId, Pr);
            }

            // return the final result
            //
            return probabilityVector;
        }
    }
}
