# Race condition

A race condition is a situation when two or more processes or threads access and modify the same resource at the same time, and the final result depends on the order in which they run. Without proper coordination, this can lead to incorrect or unpredictable results.

For example:
Two threads try to update the same shared resource, which for the sake of the example will be a simple Integer *counter* which starts as 0.

* Thread A - looks at the counter, increments the value from 0 to 1, and saves the new value in the counter.
* Thread B - at the same exact instant, looks at the counter too, sees the value 0, increments the value from 0 to 1, and saves the new value in the counter.
* The result: both updated the counter to 1 instead of updating the counter seperately to the value 2. The final result is an incorrect.

Some prevention techniques include:

* **Mutex**: Restricts access to a critical operation (the incrementation) to one thread or process at a time.
* **Atomic Operations**: Execute critical operations as a single, undividable hardware steps that cannot be interrupted.
* **Monitors**: High-level synchronization tools that manage shared resources.

An example of race condition handling in my code: In NeighbourTable class, i have a background thread that runs and every couple of seconds, iterates through the neighbour dictionary to check if there are any neighbours whose heartbeat wasn't heard for more than the legal time limit. If there are any, the thread adds them to a list, and then removes each expired neighbour from the table using that list.

Without any synchronization, that background checker thread could build his local *expired* list, end the iteration through the table dictionary, and start removing all of the expired neighbours. But if another background thread, that received heartbeats from neighbours, were to suddenly receive a new heartbeat from a neighbour who the checking thread considers expired, that would cause a race condition because the un-expired neighbour would be removed from the neighbour table and the table's state would be incorrect.

I deal with this using a Read-Write Lock, which brings us to the next concept...

*Code Example*:
```csharp
// checks for expired nodes and removes
public void CheckExpiration()
{
    using (_rwLock.Write())
    {
        DateTime current_time = _tableClock.GetTime();
        List<string> expiredNeighbours = new List<string>();

        // O(NeighbourRecords.Length) - iterates through all records
        foreach (NeighbourRecord record in NeighbourRecords.Values)
        {
            DateTime last_heartbeat = record.LastHeartbeatTime;
            int time_diff = (int)(current_time - last_heartbeat).TotalSeconds;

            // if heartbeat wasn't received from this neighbour for over N, heartbeat time cycles
            if (time_diff >= _legal_timeDiff_seconds)
                expiredNeighbours.Add(record.NeighbourID);
        }

        // remove all expired neighbours
        foreach (string expired_neighbour in expiredNeighbours)
            RemoveRecord(expired_neighbour);
    }
}
```

# Mutex vs Reader-Writer Lock

- **Mutex (Mutual Exclusion)** - is a programming tool that allows only one thread or process to access a shared resource or critical section of code at a time. In software engineering, a single global mutex that protects an entire system, is referred to as a *"Blanket Mutex"*.

- **Read-Write Lock** - is a synchronization mechanism that allows multiple threads to read shared data concurrently (shared access), but requires exclusive access for a single thread to write or modify the data.

**Mutex vs. RWLock**: A Mutex strictly allows only one thread at a time to access the shared resource, regardless of whether the thread is reading or writing, while a read-write lock allows multiple readers to access the resource simultaneously, but requires exclusive access for a single writer (blocking all other readers and writers).

**Why a blanket Mutex creates a bottleneck?** Because in a system that relies heavily on reading operations such as traversing a NeighbourTable, for each and every message that is received, a blanket Mutex forces threads that only want to read data to wait in line one by one, which creates a massive performance bottleneck. A Read-Write Lock solves this problem by letting all reading threads run concurrently, but in doing so creates a new potential problem: *Writer Starvation*, which brings us to the next topic.

Code example of my Read-Write-Lock mechanism:

*Code Example*:
```csharp
// Multiple threads can execute this simultaniously without blocking each other
public void PrintTable()
{
    // ...

    using (_rwLock.Read())
    {
        foreach (string neighbourID in NeighbourRecords.Keys)
        {
            // Thread-safe concurrent reading
            string address = NeighbourRecords[neighbourID].Address.Port.ToString();
            string heartbeat = NeighbourRecords[neighbourID].LastHeartbeatTime.ToString();

            // ...
        }
    }   

    // ...         
}

// Only ONE thread can execute this at a time, blocking all readers and other writers
public void UpdateRecord(string neighbourId, IPEndPoint address)
{
    using (_rwLock.Write())
    {
        // Exclusive write access
        NeighbourRecords[neighbourId] = new NeighbourRecord(neighbourId, address, _tableClock.GetTime());
    }
}
```

# Writer starvation

**Writer starvation** in a read-write lock is a situation when waiting writer threads are blocked because new reader threads continuously acquire the lock and prevent them from updating the shared data.

* **How my lock prevents this**: My custom lock prevents this outcome by blocking new incoming reader threads from acquiring the lock if there are currently waiting writer threads in line, or a current active thread is writing. A writer cannot acquire the lock if a reader is currently reading, so each pending writer increments a shared variable indicating how many writers are waiting to write. When new readers arrive, they cannot acquire the lock if this variable shows that there are writers in front of them in line.
By using the **Monitor** class in C# (a synchronization tool) I put threads to sleep (with Wait) when they are blocked, and wake them up (with PulseAll) when the lock is released, ensuring CPU resources aren't wasted.

*Code Example*:
```csharp
bool _activeWriter = false; // a boolean indicating if a writer is currently active
int _pendingWriters = 0;    // current number of pending writer threads

// Only when there aren't writers pending or an active one, a reader is allowed to acquire the lock
public IDisposable Read()
{
    lock (_lock)
    {
        // If a writer is currently active or pending, go to sleep
        while (_activeWriter || _pendingWriters > 0)
            Monitor.Wait(_lock);

        // reader thread operations ...
    }
}
```

# Deadlock

A **Deadlock** is a state where two or more processes or threads cannot proceed and are stuck forever because each is waiting for a resource held by another.

A fixed lock order prevents deadlocks by eliminating the possibility of circular waiting between threads by forcing all threads to acquire locks in the same sequence, making a circular wait impossible.

In my project, threads will eventually need to interact with two or more shared resources at a time. For example: Authorize a received message by the NeighbourTable and then saving that message in the messages Buffer. If there was no order rule for aquiring the locks, a background thread could lock the Table and then ask for the Buffer, while another background processing thread might lock the Buffer and then ask for the Table. If they run in the same exact moment they could get stuck in a deadlock waiting forever.

The ordering rule that I enforce: Always acquire the Table lock before Aquiring the Buffer lock. This matches the natural data flow: A received message is **FIRST**, authorized by the Table, and only **THEN** added to the buffer. Plus, dealing with the heavier operations of the Table first, doesn't cause the Buffer thread to unnecessarily wait for the slower Table operations, preventing a slight bottleneck.

*Code Example*:
```csharp
// Theoretical code example, because Buffer is still not implemented
public void ProcessPacket(Packet packet)
{
    // LOCK ORDER RULE: Always acquire access to the NeighbourTable lock before the Buffer lock to prevent Deadlocks.
    using (table.Read())
    {
        // Authorize sender first...
        if (table.IsValidNeighbour(packet.SrcID))
        {
            // Add to buffer after...
            using (buffer.Write())
            {
                buffer.AddPacket(packet);
            }
        }
    }
}
```
