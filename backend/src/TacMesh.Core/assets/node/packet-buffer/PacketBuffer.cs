using TacMesh.Core.assets.node.packet_buffer;
using TacMesh.Core.interfaces.clock_interfaces;
using TacMesh.Core.packet;
using TacMesh.Core.packet_related;

namespace TacMesh.Core.assets.node
{
    public class PacketBuffer
    {
        // the maximum amount of messages that can be stored in TOTAL
        private readonly int _BUFFER_CAPACITY = 1000;
        private long _TOTAL_PACKETS; // packet counter
        private readonly IClock _sysClock; // system clock for TTL timestamps
        private const int _PACKET_EXPIRATION_DELAY = 5000; //ms

        // the priority buckets
        private readonly PriorityBucket[] _buckets;


        // constructor
        public PacketBuffer(IClock clock)
        {
            _sysClock = clock;

            // init total packets count as 0
            //
            _TOTAL_PACKETS = 0;

            // buckets array which holds 10 'buckets' for each priority. each bucket contains
            // a priority queue of the packets ordered by the TTL (oldest come first)
            //
            _buckets = new PriorityBucket[PacketHeader.MAX_PRIORITY];

            // init each bucket
            //
            for (int i = 0; i < _buckets.Length; i++)
                _buckets[i] = new PriorityBucket(clock);

            // automatically start expiration check on startup
            StartExpiration();
        }

        /// <summary>
        /// adds a new message to the packet buffer.
        /// </summary>
        /// <param name="packet"></param>
        public void Add(DataPacket packet)
        {
            // makes new room while the buffer is full.
            //
            // NOTE: interlocked must be used to read/increment/decrement the total packets count
            // because if two threads change the value of the count at the same time, a race 
            // condition may happen, so using atomic operations on it is mandatory
            //
            while (Interlocked.Read(ref _TOTAL_PACKETS) >= _BUFFER_CAPACITY) {
                RemoveOldestPacket();
            }

            // check packets expiration to clean up before adding
            CheckExpiration();

            // add the packet if it does not exist in the priority queue (by reference).
            // if the packet were already existing, the check is still correct because
            // it would fall into the same bucket by the priority
            //
            PriorityBucket bucket = _buckets[packet.PacketHeader.Priority - 1];

            if (!bucket.Contains(packet)) // O(1) [using HashSet for lookup]
            {
                // insert the packet
                bucket.Enqueue(packet); // O(logM) [because priority queue used min-heap]
                Interlocked.Increment(ref _TOTAL_PACKETS);
            }
            // NOTE: if the packet already exists in the queue it is dropped ...
        }

        // if the buffer is full, remove a packet with the lowest
        // priority and oldest TTL
        //
        // O(1) - potential iteration over all buckets (constant 10)
        //     +
        // O(logM) - dequeue the lowest priority queue of the priority bucket (which uses a min-heap)
        //
        // Total: O(logM)
        //
        private void RemoveOldestPacket()
        {
            for (int i = 0; i < _buckets.Length; i++)
            {
                // remove oldest packet from the lowest priority
                //
                if (!_buckets[i].Empty())
                {
                    _buckets[i].TryDequeue(out _);
                    Interlocked.Decrement(ref _TOTAL_PACKETS);
                    break;
                }
            }
        }

        /// <summary>
        /// Gets and removes the most prioritiezed next packet saved in the buffer
        /// </summary>
        /// <returns>Returns the next packet or null if buffer is empty</returns>
        public DataPacket? GetNext()
        {
            for (int i = _buckets.Length - 1; i >= 0; i--)
            {
                if (!_buckets[i].Empty())
                {
                    _buckets[i].TryDequeue(out DataPacket? packet);
                    Interlocked.Decrement(ref _TOTAL_PACKETS);
                    return packet;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets without removing the most prioritiezed next packet saved in the buffer
        /// </summary>
        /// <returns>Returns the next packet or null if buffer is empty</returns>
        public DataPacket? PeekNext()
        {
            for (int i = _buckets.Length - 1; i >= 0; i--)
            {
                if (!_buckets[i].Empty() && _buckets[i].TryPeek(out DataPacket? packet))
                {
                    return packet;
                }
            }
            return null;
        }

        // O(M) Worst [M - number of total packets in the buffer]
        //
        private void CheckExpiration()
        {
            for (int i = 0; i < _buckets.Length; i++)
            {
                PriorityBucket currBucket = _buckets[i];

                // remove expired packets
                while (!currBucket.Empty() && currBucket.TryPeekExpiration(out DateTime expirationTime))
                {
                    // compare between current time and packet's TTL timestamp
                    DateTime currentTime = _sysClock.GetTime();

                    // the time difference between the timestamps
                    double time_diff = (double)(currentTime - expirationTime).TotalSeconds;

                    // positive = current time passed the packet expiration time
                    //
                    if (time_diff >= 0)
                    {
                        // remove if packet expired
                        if (currBucket.TryDequeue(out _))
                            Interlocked.Decrement(ref _TOTAL_PACKETS);
                        else
                            // continue to next packet if dequeue failed, meaning the packet was already removed
                            continue;
                    }
                    // as soon as there isn't an expired packet, all other packets in
                    // that queue won't be expired because they are ordered by oldest
                    //
                    else break; // break out of the loop
                }
            }
        }

        public void StartExpiration()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    try {
                        // check for expired packets every couple of seconds
                        CheckExpiration();
                        await Task.Delay(_PACKET_EXPIRATION_DELAY);
                    }
                    catch (Exception e) {
                        Console.WriteLine($"[PACKET BUFFER] Encountered an error checking for expired packets due to: \"{e.Message}\"");
                    }
                }
            });
        }
    }
}
