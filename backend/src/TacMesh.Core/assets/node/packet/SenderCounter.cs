namespace TacMesh.Core.assets.node.packet
{
    /// <summary>
    /// Sender Counter class to provide the most up to date count
    /// to indicate the number of the packet which is sent
    /// </summary>
    public class SenderCounter
    {
        private long _COUNTER; // the counter state

        // constructor
        public SenderCounter()
        {
            _COUNTER = -1; // init
        }

        /// <summary>
        /// Method to get the current counter and increment it safely.
        /// </summary>
        /// <returns>The current Sender Counter</returns>
        public long GetCurrent()
        {
            // the counter starts at -1, meaning the value returned
            // is allways the current and correct value of the counter
            // while being incremented
            //
            // for example: first value is 0, the counter start at -1, before being returned
            // it gets incremented to 0 and then returned.
            //
            return Interlocked.Increment(ref _COUNTER);
        }
    }
}
