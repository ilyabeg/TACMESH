namespace TacMesh.Core.assets.clocks
{
    /// <summary>
    /// Lamport's logical clock to determine the order of updates
    /// </summary>
    public class LamportClock
    {
        // counter which starts as 0
        private long _COUNTER;
        private static readonly object _syncLock = new object();

        // constructor
        public LamportClock()
        {
            // init count
            _COUNTER = 0;
        }

        /// <summary>
        /// Gets the current value of the local logical clock
        /// </summary>
        /// <returns>The current value of the clock</returns>
        public long GetCurrent() => Interlocked.Read(ref _COUNTER);

        /// <summary>
        /// Get's the next value of the counter 
        /// </summary>
        /// <returns>Returns the value of the counter + 1</returns>
        public long GetNext() => Interlocked.Increment(ref _COUNTER); // using atomic increment for thread safety

        /// <summary>
        /// Syncs the local counter with the maximum between the current and remote counter, 
        /// and increments the value by 1
        /// </summary>
        /// <param name="remoteCounter">The remote counter which was received</param>
        /// <returns>The new value of the local counter after the change</returns>
        public long Sync(long remoteCounter)
        {
            // lock to avoid race conditions
            //
            lock (_syncLock)
            {                
                _COUNTER = Math.Max(_COUNTER, remoteCounter) + 1;
                return _COUNTER;
            }
        }
    }
}
