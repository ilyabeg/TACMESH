using System.Collections;
using TacMesh.Core.utils.serializers;

namespace TacMesh.Core.assets.routing.epidemic
{
    /// <summary>
    /// Bloom Filter to handle the Epidemic protocol pairwise packet exchange
    /// </summary>
    public class BloomFilter
    {
        // IMPORTANT NOTE: the Bloom Filter is very good at saving network bandwidth by compressing
        // the list of Guids into constant 256 byte packets.
        //
        // without the filter, the Epidemic pairwise exchange packet would be 16 bytes of Guid * the
        // amount of packets. for example:
        //
        // 80 packets * 16 bytes each ID = 1280 bytes!
        //
        // Packet Amount (N) | Guid Size (N * 16B) | Bloom Filter Size | Bandwidth saved
        // ------------------+---------------------+-------------------+-----------------
        // 50 packets        | 800 Bytes           | 256 Bytes         | 68.0% saved
        // 100 packets       | 1,600 Bytes         | 256 Bytes         | 84.0% saved
        // 150 packets       | 2,400 Bytes         | 256 Bytes         | 89.0% saved
        //
        // saved precentage (100 packets) = (1600 - 256) / 1600 = 1344 / 1600 = 84%
        //
        // The Bloom Filter Risk (False Positive):
        //
        // the only risk with the bloom filter is that packet ids may overlap on specific bits.
        // some packet ids may mark bits 10, 20, 30 as TRUE. and some other ID-X hash's may fall
        // on those same bits. 
        // when a node looks at those three bit positions, all three are 1 and mistakenly conclude
        // that packet with ID-X is present in that filter, even if it isn't.
        //
        // the consequence of this error is completely harmless. if a false positive occurs on packet
        // X, Node B assumes Node A has it, so Node B does not transfer packet X during that specific
        // contact opportunity. packet is NOT lost because Epidemic Routing replicates packets across
        // multiple paths, another node will deliver packet X to Node A, or Node B will deliver it
        // during another encounter when the buffer state has changed.
        // it simply slightly delays delivery for that packet, but prevents 100% of buffer overflows
        //
        // the probability of a False Positive formula:
        //
        // p =~ (1 − e^(-[k * n]/m))^k ; m = total bits, n = number of packets, k = number of hash functions
        //
        // exmple: the probability of a false positive happening for 100 packets in the buffer, 256 byte
        // filter, and 3 hashes is: ~0.00253 = ~0.25%. that's 1 in 400 packets. more packets = greater Prob'
        //
        // Conclution: taking a risk of a packet arriving in a small delay, or through a different path,
        // once in a while, for such a huge and efficient advantage is completely justifiable, especialy when
        // this risk almost doesn't affect or comprimise the execution and performance of the algorithm, at all.


        /// <summary>
        /// The number of cycles/iterations of hash computing for each message id
        /// </summary>
        private const int K = 3;

        /// <summary>
        /// 256 byte array = 2048 bits
        /// </summary>
        private const int FilterSize = 256;

        /// <summary>
        /// The amount of bits in the filter size
        /// </summary>
        private const int BitLength = FilterSize * 8;


        /// <summary>
        /// Computes a bloom filter for all the current node's buffered packets by flipping
        /// the corresponding bits of the hashed packet ids in the filter byte array
        /// </summary>
        /// <param name="packetIdentifiers">The packet identifiers</param>
        /// <returns>Returns the computed bloom filter from the packet ids</returns>
        public static byte[] ComputeFilter(List<Guid> packetIdentifiers)
        {
            // the final filter result
            byte[] filter = new byte[FilterSize];

            BitArray bitArray = new BitArray(filter); // create bit array from the filter bytes

            // compute hash on each packet id and flip corresponding bit in the
            // bit array. repeat K times
            // O(N) - iterates over N packet ids
            //
            foreach (Guid packetID in packetIdentifiers)
            {
                // get the base and step hashes from the packet id
                //
                var (baseHash, stepHash) = GetBaseAndStepHashes(packetID);

                // O(K) - K iterations = O(1) because K is simply 3
                //
                for (int i = 0; i < K; i++)
                {
                    // compute hash index of the packet
                    //
                    int hashIndex = ComputeHashIndex(baseHash, stepHash, hashCycle: i);

                    // flip corresponding bit at the computed index
                    //
                    bitArray[hashIndex] = true; // true = 1
                }
            }
            // copy computed filter bits back to byte array from index 0
            //
            bitArray.CopyTo(filter, index: 0);
            return filter;
        }

        /// <summary>
        /// Determines the missing packets from the byte 
        /// </summary>
        /// <param name="filter">The received remote bloom filter</param>
        /// <param name="packetIdentifiers">The current packet identifiers</param>
        /// <returns></returns>
        public static List<Guid> DetermineMissingPackets(byte[] filter, List<Guid> packetIdentifiers)
        {
            // the final missing packets result
            List<Guid> missingPackets = new List<Guid>();

            BitArray bitArray = new BitArray(filter); // create bit array from the filter bytes

            // iterate through all packets, compute their hash, and
            // determine if it's missing if any corresponding hashed
            // bits are false (0)
            //
            foreach (Guid packetID in packetIdentifiers)
            {
                // get the base and step hashes from the packet id
                //
                var (baseHash, stepHash) = GetBaseAndStepHashes(packetID);

                // O(K) - K iterations = O(1) because K is simply 3
                //
                for (int i = 0; i < K; i++)
                {
                    // compute hash index of the packet
                    //
                    int hashIndex = ComputeHashIndex(baseHash, stepHash, hashCycle: i);

                    // corresponding bit at the computed index is off (0) = missing packet
                    //
                    if (!bitArray[hashIndex])
                    {
                        // add packet to missing list and continue to next packet id
                        //
                        missingPackets.Add(packetID);
                        break;
                    }                        
                }
            }
            // return the final result
            return missingPackets;
        }

        // --- Kirsch & Mitzenmacher hashing ---

        /// <summary>
        /// Computes the corresponding index of the hashed packet id in the filter bit array
        /// </summary>
        /// <param name="packetID">The packet identifier</param>
        /// <returns>The corresponding index of the hashed packet id</returns>
        private static int ComputeHashIndex(int baseHash, int stepHash, int hashCycle)
        {
            // convert calculation to un-signed int to keep as positive
            //
            return (int)((uint)(baseHash + hashCycle * stepHash) % BitLength);
        }

        /// <summary>
        /// Splits the packet id into a base "hash" and step "hash"
        /// </summary>
        /// <param name="packetID">The packet id to split</param>
        /// <returns>The base and step parts of the packet id</returns>
        private static (int BaseHash, int StepHash) GetBaseAndStepHashes(Guid packetID)
        {
            // convert id to bytes
            byte[] packetIdBytes = packetID.ToByteArray();

            // byte reader to read the id bytes
            ByteReader byteReader = new ByteReader(packetIdBytes);

            // read the first 4 bytes of the id
            int baseHash = byteReader.ReadInt();

            // read the next 4 bytes, AND convert into an odd number
            // to avoid step loops
            //
            int stepHash = byteReader.ReadInt() | 1;

            return (baseHash, stepHash);
        }
    }
}
