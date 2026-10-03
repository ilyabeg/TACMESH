namespace TacMesh.Core.assets.read_write_lock
{
    /// <summary>
    /// Custom, writer preffering, Read/Write Lock class to handle thread synchronization
    /// and data integrity without causing writer starvation.
    /// </summary>
    public class TacReadWriteLock
    {
        // standard lock to lock operations on the internal R/W Lock's state (the counters..)
        private readonly object _lock = new object();

        private int _numOfReaders = 0; // current number of reading threads
        private bool _activeWriter = false; // a boolean indecating if a writer is currently active
        private int _pendingWriters = 0; // current number of pending writer threads


        // --- lock operations ---

        // reading operation. this is the reading access autherizer: let's a reader
        // increment the amount of readers and returns an IDisposable representing 
        // the autherization to the actual critical operation. when that reader finished he
        // disposes and releases the 'access' and decrements the reader counter
        public IDisposable Read()
        {
            lock (_lock)
            {
                // if a writer is currently active or pending, go to sleep
                while (_activeWriter || _pendingWriters > 0)
                    Monitor.Wait(_lock);

                // increment number of readers
                _numOfReaders++;

                // return IDisposable object to automatically decrement counter on disposing
                return new RLock(this);
            }
        }

        // the method that the reader runs when disposing itself. this is the 'cleanup'
        private void UnlockRead()
        {
            lock (_lock)
            { 
                // after reader is done, decrement number of readers
                _numOfReaders--;

                // if reader was last, pulse all
                if (_numOfReaders == 0)
                    Monitor.PulseAll(_lock);
            }
        }

        // writing operation. this is the writing access autherizer: let's a writer
        // perform the writing operation and returns an IDisposable representing 
        // the autherization to the actual critical operation. when that writer finished he
        // disposes and releases the 'access' and turns _activeWriter to false
        public IDisposable Write()
        {
            lock (_lock)
            {
                // increment number of pending writers
                _pendingWriters++;

                // if a writer or reader is currently active, go to sleep
                while (_activeWriter || _numOfReaders > 0)
                    Monitor.Wait(_lock);

                // decrement number of pending writers (because he no longer waits)
                _pendingWriters--;
                _activeWriter = true; // because he is now actively writting

                return new WLock(this);
            }
        }

        // the method that the writer runs when disposing itself. this is the 'cleanup'
        private void UnlockWrite()
        {
            lock (_lock)
            {
                _activeWriter = false; // because he is no longer active

                // notify all because he finished
                Monitor.PulseAll(_lock);
            }
        }


        // ----- PRIVATE NESTED CLASSES -----

        // Why make the classes private? so only TacReadWriteLock could access these classes.
        // no one should know about their existance, and their purpose is to manage the lifecycle
        // of the lock, and ensuring the lock is automatically released with the syntax: 'using (...) { ... }'.

        private class RLock : IDisposable
        { 
            private TacReadWriteLock _rwLock;

            // inject the ReadWriteLock to call Unlock method
            public RLock(TacReadWriteLock rwLock)
            {
                _rwLock = rwLock;
            }

            // 'cleanup' after reader is done and dispose of the object 
            public void Dispose() => _rwLock.UnlockRead();
        }

        private class WLock : IDisposable
        {
            private TacReadWriteLock _rwLock;

            // inject the ReadWriteLock to call Unlock method
            public WLock(TacReadWriteLock rwLock)
            {
                _rwLock = rwLock;
            }

            // 'cleanup' after writer is done and dispose of the object 
            public void Dispose() => _rwLock.UnlockWrite();
        }
    }
}
