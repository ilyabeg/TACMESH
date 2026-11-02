using TacMesh.Core.interfaces.clock_interfaces;
using TacMesh.Core.packet_related;

namespace TacMesh.Core.assets.node.packet_buffer
{
    /// <summary>
    /// Priority bucket object to hold a MinHeap containing DataPackets of the same priority
    /// ordered by their TTL (oldest first)
    /// </summary>
    public class PriorityBucket
    {
        // record to hold the packet and its expiration time for the priority queue elements
        private record BucketRecord(DataPacket Packet, DateTime ExpirationTime);

        private readonly IClock _sysClock; // system clock for packet expiration timestamps
        private readonly object _bucketLock = new object(); // lock for thread safety
        private readonly PriorityQueue<BucketRecord, DateTime> _elements;
        private readonly HashSet<DataPacket> _packetSet; // for contains check


        // constructor
        public PriorityBucket(IClock clock)
        {
            _sysClock = clock;
            _elements = new PriorityQueue<BucketRecord, DateTime>();
            _packetSet = new HashSet<DataPacket>();
        }

        // is empty helper func
        public bool Empty() => _elements.Count == 0;

        /// <summary>
        /// Returns true if the priority queue contains the packet
        /// </summary>
        /// <param name="packet"></param>
        /// <returns></returns>
        public bool Contains(DataPacket packet)
        {
            lock (_bucketLock)
            {
                return _packetSet.Contains(packet);
            }
        }

        /// <summary>
        /// Enqueues a packet element to the Priority bucket queue
        /// </summary>
        /// <param name="packet"></param>
        public void Enqueue(DataPacket packet)
        {
            lock (_bucketLock)
            {
                // calculate the packet expiration time by adding the TTL to the current time
                //
                DateTime packetExpirationTime = _sysClock.GetTime().AddMilliseconds(packet.PacketHeader.TTL);
                BucketRecord newRecord = new BucketRecord(packet, packetExpirationTime);

                // add the new record for the packet and to the hashset
                //
                _elements.Enqueue(newRecord, packetExpirationTime);
                _packetSet.Add(packet);
            }
        }

        /// <summary>
        /// Tries to dequeue the top packet element from the Priority bucket queue
        /// </summary>
        /// <returns>True if successful, False otherwise</returns>
        public bool TryDequeue(out DataPacket? packet)
        {
            // NOTE: use the out to avoid race condition, for example when the queue
            // is not empty when the if statement is called and at the exact same
            // moment a different thread dequeues, making the queue empty and
            // causing an exception

            lock (_bucketLock)
            {
                if (!Empty())
                {
                    packet = _elements.Dequeue().Packet;
                    _packetSet.Remove(packet);
                }
                else packet = null;
            }
            return packet != null;
        }

        /// <summary>
        /// Tries to peek in the elements priority queue and outputs the packet or null if empty
        /// </summary>
        /// <param name="packet"></param>
        /// <returns>True if successful, False otherwise</returns>
        public bool TryPeek(out DataPacket? packet)
        {
            lock (_bucketLock)
            {
                if (!Empty())
                    packet = _elements.Peek().Packet;
                else
                    packet = null;
            }
            return packet != null;
        }

        /// <summary>
        /// Tries to peek in the elements priority queue and outputs the expiration time or null if empty
        /// </summary>
        /// <param name="expirationTime"></param>
        /// <returns>True if successful, False otherwise</returns>
        public bool TryPeekExpiration(out DateTime expirationTime)
        {
            lock (_bucketLock)
            {
                if (!Empty())
                    expirationTime = _elements.Peek().ExpirationTime;
                else
                    expirationTime = default;
            }
            return expirationTime != default;
        }
    }
}