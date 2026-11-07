namespace TacMesh.Core.assets.node.packet.validation
{
    /// <summary>
    /// Record class to hold the highest logical clock received and 
    /// sender counter window for every user.
    /// </summary>
    public class ValidationRecord
    {
        // the highest logical clock value received for this specific user
        private long _logicalClockValue;
        private readonly object _clockLock = new object();

        // the values window for the packet sender counts received from this user
        //
        private readonly int _WINDOW_SIZE = 64; // bits
        private long _sender_count_window; // holds the sender newCount values window 
        private long _MAX_SENDER_COUNT; // holds the max seen sender newCount
        private readonly object _senderCountLock = new object();


        // counstructor
        public ValidationRecord(long logicalClock)
        {
            _logicalClockValue = logicalClock;
            _sender_count_window = 0;
            _MAX_SENDER_COUNT = -1; // init so the first heard newCount will allways be greater
        }

        /// <summary>
        /// Updates the logical clock value only if the new value is greater than the current.
        /// </summary>
        /// <param name="newValue">The new value</param>
        /// <returns>True if updated successfuly or False otherwise</returns>
        public bool TryUpdateClock(long newValue)
        {
            lock (_clockLock)
            {
                if (newValue > _logicalClockValue)
                {
                    _logicalClockValue = newValue;
                    return true;
                }
                return false;
            }
        }


        // marked bit as 1
        private readonly long _markedBit = 1;

        /// <summary>
        /// Updates the sender counter value only if the new value is greater in the 64 values window.
        /// </summary>
        /// <param name="newCount">The new counter</param>
        /// <returns>True if the counter successfuly updates or false otherwise</returns>
        public bool TryUpdateSenderCount(long newCount)
        {
            lock (_senderCountLock)
            {
                // if the new newCount is greater than the max, slide the window
                //
                if (newCount > _MAX_SENDER_COUNT)
                {
                    int shifts = (int)(newCount - _MAX_SENDER_COUNT); // the shifts needed to move the window

                    // new window resets if larger than the size
                    if (shifts >= _WINDOW_SIZE)
                    {
                        _sender_count_window = _markedBit; // set as 1 to mark the new received count
                    }                    
                    else
                    {
                        // shift the window and keep the marked bits as is,
                        // bits that fall off the shift get deleted
                        //
                        _sender_count_window = (_sender_count_window << shifts) | _markedBit;
                    }

                    _MAX_SENDER_COUNT = newCount; // update the max seen count
                    return true;
                }

                // if the newCount is inside the window, update the bit
                // at the corrisponding counter value
                //
                else if (newCount > _MAX_SENDER_COUNT - _WINDOW_SIZE && newCount <= _MAX_SENDER_COUNT)
                {
                    // the diff between the max knwon newCount and the new one
                    //
                    int diff = (int)(_MAX_SENDER_COUNT - newCount);

                    // get the corrsiponding bit value
                    //
                    long corrispondingBit = (_sender_count_window >> diff) & _markedBit;

                    // if the bit was already marked as received, the newCount is invalid
                    //
                    if (corrispondingBit == _markedBit) return false;

                    // mark the corrsiponding bit as a marked bit (flip that bit to a 1)
                    //
                    _sender_count_window = _sender_count_window | (_markedBit << diff);
                    return true;
                }

                // if newCount is out of the window range, the newCount is invalid
                //
                return false;
            }
        }
    }
}
