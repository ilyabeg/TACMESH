using TacMesh.Core.interfaces.clock_interfaces;
using TacMesh.Core.packet_related;

namespace TacMesh.Core.assets.node.packet.buffer
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
        
        // the hashset is used for O(1) lookups for a Contains() check in the buffer,
        // the hashset is also used to remove messages upon returning all of the pending 
        // messages of a specific destination id, in O(K) time [simply deleting from
        // the set], instead of O(K * M) [for each message iterate through all of the
        // messages in the buffer, which is M, find it and remove that specific message]
        //
        private readonly HashSet<DataPacket> _packetSet;


        // constructor
        public PriorityBucket(IClock clock)
        {
            _sysClock = clock;
            _elements = new PriorityQueue<BucketRecord, DateTime>();
            _packetSet = new HashSet<DataPacket>();
        }


        // --- Hashset methods ---

        /// <summary>
        /// Specifies wether the priority bucket is empty or not
        /// </summary>
        /// <returns>True if the priority bucket is empty, or False otherwise</returns>
        ///
        /// NOTE: determine via the set which holds the only true packets stored in this bucket
        ///
        public bool Empty()
        {
            lock (_bucketLock)
            {
                return _packetSet.Count == 0;
            }
        }

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
        /// Removal function to remove specific packets from the packet set
        /// </summary>
        /// <param name="packet">the packet to remove</param>
        public void Remove(DataPacket packet)
        {
            lock (_bucketLock)
            {
                // O(1) Removal [hashset lookup]
                _packetSet.Remove(packet);
            }
        }

        /// <summary>
        /// Iterates through the elements and removes records containing packets that 
        /// are supposed to be removed (ghost packets) from the front of the queue
        /// </summary>
        /// <param name="validRecord">returns the top valid packet record or null if the elements are empty</param>
        /// <returns>True if the top most valid record is not null.</returns>
        private bool RemoveGhostRecords(out BucketRecord? validRecord)
        {
            // remove while not empty
            while (_elements.Count > 0) 
            {
                BucketRecord topRecord = _elements.Peek();

                // remove record if the packet was removed from the set
                if (!_packetSet.Contains(topRecord.Packet))
                    _elements.Dequeue();
                else
                    break;
            }

            // if empty output as null
            if (_elements.Count == 0)
                validRecord = null;
            else
                validRecord = _elements.Peek();

            // return true if top valid is not null
            return validRecord != null;
        }


        // --- Enqueue & Dequeue ---

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
                // get rid of ghost records and output the top most packet
                //
                if (!Empty() && RemoveGhostRecords(out _))
                {
                    // remove the top valid packet which actualy is part of the bucket
                    // and not a packet which got removed
                    //
                    packet = _elements.Dequeue().Packet;
                    _packetSet.Remove(packet);
                }
                else packet = null;
            }
            return packet != null;
        }


        // --- Peeks ---

        /// <summary>
        /// Tries to peek in the elements priority queue and outputs the packet or null if empty
        /// </summary>
        /// <param name="packet"></param>
        /// <returns>True if successful, False otherwise</returns>
        public bool TryPeek(out DataPacket? packet)
        {
            lock (_bucketLock)
            {
                // get rid of ghost records and output the top most packet
                //
                if (!Empty() && RemoveGhostRecords(out BucketRecord? topValid))
                {
                    packet = topValid!.Packet;
                }
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
                // get rid of ghost records and output the top most packet
                //
                if (!Empty() && RemoveGhostRecords(out BucketRecord? topValid))
                {
                    // save the first valid element's expiration
                    expirationTime = topValid!.ExpirationTime;
                }
                else
                    expirationTime = default;
            }
            return expirationTime != default;
        }
    }
}