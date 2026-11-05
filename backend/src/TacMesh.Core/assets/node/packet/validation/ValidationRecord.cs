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

        // the 64 values window for the packet sender counts received from this user
        // COMES LATER ...


        // counstructor
        public ValidationRecord(long logicalClock)
        {
            _logicalClockValue = logicalClock;
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
    }
}
