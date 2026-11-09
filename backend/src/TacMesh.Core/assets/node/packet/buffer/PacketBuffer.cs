using TacMesh.Core.interfaces.clock_interfaces;
using TacMesh.Core.packet;

namespace TacMesh.Core.assets.node.packet.buffer
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
        
        // dictionary to hold messages by their recipient id, ordered by max priority
        private readonly Dictionary<string, PriorityQueue<DataPacket, int>> _messagesByRecipient;


        // constructor
        public PacketBuffer(IClock clock)
        {
            _sysClock = clock;

            // init total packets count as 0
            _TOTAL_PACKETS = 0;

            // buckets array which holds 10 'buckets' for each priority. each bucket contains
            // a priority queue of the packets ordered by the TTL (oldest come first)
            //
            _buckets = new PriorityBucket[PacketHeader.MAX_PRIORITY];

            // init each bucket
            //
            for (int i = 0; i < _buckets.Length; i++)
                _buckets[i] = new PriorityBucket(clock);

            // create new messaged by destination dict to efficiently rmeove messaged for specific
            // destinations upon receiving new neigbors and calculating new routes on the net
            //
            _messagesByRecipient = new Dictionary<string, PriorityQueue<DataPacket, int>>();

            // automatically start expiration check on startup
            StartExpiration();
        }


        // --- Contains ---

        /// <summary>
        /// Checks if the packet is already included in the packet buffer
        /// </summary>
        /// <param name="packet">The packet to check if already included</param>
        /// <returns>True if the packet is already in the packet buffer or False otherwise</returns>
        public bool Contains(DataPacket packet)
        {
            // if the packet were already existing, the check is still correct because
            // it would fall into the same bucket by the priority
            //
            PriorityBucket bucket = _buckets[packet.PacketHeader.Priority - 1];
            return bucket.Contains(packet); // O(1) [using HashSet for lookup]
        }


        // --- Addition logic ---

        /// <summary>
        /// adds a new message to the packet buffer.
        /// </summary>
        /// <param name="packet"></param>
        public void Add(DataPacket packet)
        {
            // check packets expiration to clean up before adding
            CheckExpiration();

            // add the packet if it does not exist in the priority queue (by msg id).
            //
            if (!Contains(packet))
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

                // insert the packet to the corrisponding priority bucket
                _buckets[packet.PacketHeader.Priority - 1].Enqueue(packet); // O(logM) [because priority queue used min-heap]

                // insert to recipients dict
                AddToRecipients(packet);

                // increment total count
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
                if (!_buckets[i].Empty() && _buckets[i].TryDequeue(out DataPacket? packet))
                {
                    // remove same message from dictionary 
                    TryRemoveFromRecipients(packet!);

                    // decrement total count
                    Interlocked.Decrement(ref _TOTAL_PACKETS);
                    break;
                }
            }
        }


        // --- Recipient dictionary logic ---

        // max priority comparer to order messages of specific destination by the max priority
        private Comparer<int> MaxPriorityComparer() => Comparer<int>.Create((x, y) => y.CompareTo(x));

        // NOTE: seperate recipient dictionary lock to avoid race conditions. for example: thread A adds new message
        // while thread B removed a message and shifts around the new priority queue state of the messages
        //
        private static readonly object _dictKey = new object();

        /// <summary>
        /// Adds new packet to the messages of the specific recipient messages queue
        /// </summary>
        /// <param name="packet"></param>
        private void AddToRecipients(DataPacket packet)
        {
            lock (_dictKey)
            {
                // insert to specific destinations messages dictionary
                string destId = packet.PacketHeader.DstID;

                // create new messages queue if destination does not exist in the dict
                //
                if (!_messagesByRecipient.ContainsKey(destId))
                    _messagesByRecipient.TryAdd(destId, new PriorityQueue<DataPacket, int>(MaxPriorityComparer()));

                // enqueue the packet with it's priority
                _messagesByRecipient[destId].Enqueue(packet, packet.PacketHeader.Priority);
            }
        }

        /// <summary>
        /// Tries to remove a specific packet from the messages by recipient dictionary
        /// </summary>
        /// <param name="packet"></param>
        /// <returns>True if successful, False otherwise.</returns>
        private bool TryRemoveFromRecipients(DataPacket packet)
        {
            lock (_dictKey)
            {
                string destId = packet.PacketHeader.DstID;
                _messagesByRecipient.TryGetValue(destId, out PriorityQueue<DataPacket, int>? recipientQueue);
                bool result = recipientQueue?.Remove(packet, out _, out _) ?? false;

                // if the queue is empty, remove the key value pair
                if (recipientQueue?.Count == 0)
                    _messagesByRecipient.Remove(destId);

                // return the removal result
                return result;
            }
        }

        /// <summary>
        /// Get the queuesd messages of a specific provided recipient.
        /// </summary>
        /// <param name="recipientId">the specific recipient</param>
        /// <param name="recipientMessages">the recipient's messages in the buffer</param>
        /// <returns>The queued messages of the specific recipient or null if does not exist.</returns>
        public bool GetRecipientMessages(string recipientId, out Queue<DataPacket>? recipientMessages)
        {
            lock (_dictKey)
            {
                // TryGetValue() returns true if the dictionary contains an
                // element with the specified key, or false otherwise
                //
                bool result = _messagesByRecipient.TryGetValue(recipientId, out PriorityQueue<DataPacket, int>? messages);
                recipientMessages = new Queue<DataPacket>();

                // pull all messages of the specific recipient
                //
                while (messages?.Count > 0)
                {
                    // get the next packet
                    DataPacket nextPacket = messages.Dequeue();
                    recipientMessages.Enqueue(nextPacket);

                    // remove it from the buffer entirely
                    // O(1) Removal for K messages = Total O(K) t.c                    
                    //
                    _buckets[nextPacket.PacketHeader.Priority - 1].Remove(nextPacket);

                    // decrement total count
                    Interlocked.Decrement(ref _TOTAL_PACKETS);
                }
                // remove the kvp of the recipient messages dictionary
                if (messages?.Count == 0) _messagesByRecipient.Remove(recipientId);

                // return the result of the removal
                return result;
            }            
        }


        // --- Peek logic ---

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
                    if (_buckets[i].TryDequeue(out DataPacket? packet))
                    {
                        // remove same message from dictionary 
                        TryRemoveFromRecipients(packet!);

                        // decrement total count
                        Interlocked.Decrement(ref _TOTAL_PACKETS);
                        return packet;
                    }                    
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
                if (!_buckets[i].Empty() && _buckets[i].TryPeek(out DataPacket? packet)) {
                    return packet;
                }
            }
            return null;
        }


        // --- Expiration logic ---

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
                        if (currBucket.TryDequeue(out DataPacket? expiredPacket))
                        {
                            // remove same message from dictionary 
                            TryRemoveFromRecipients(expiredPacket!);

                            // decrement total count
                            Interlocked.Decrement(ref _TOTAL_PACKETS);
                        }
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
