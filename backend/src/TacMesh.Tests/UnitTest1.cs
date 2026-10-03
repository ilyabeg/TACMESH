using System.Collections.Concurrent;
using TacMesh.Core.assets.read_write_lock;

namespace TacMesh.Tests;

public class UnitTest1
{
    // counters to ensure lock is working correctly, when a reader is reading
    // writer count should be 0, and when a writer is writing, reader count
    // should be 0 and writer count should be 1
    int reader_count = 0;
    int writer_count = 0;
    int total_count = 0;

    // stress test
    [Fact]
    public void Test_Read_Write_Lock_Stress()
    {
        // Arrange
        TacReadWriteLock rw_lock = new TacReadWriteLock();
        Task[] threads = new Task[10];

        // 8 readers
        for (int i = 0; i < 8; i++)
        {
            threads[i] = Task.Run(() => ReadThread(rw_lock));
        }
        // 2 writers
        threads[8] = Task.Run(() => WriteThread(rw_lock));
        threads[9] = Task.Run(() => WriteThread(rw_lock));

        // Acr
        // 10.000 operations (10 threads * 1000 iterations each)
        Task.WaitAll(threads);

        // Assert
        Assert.Equal(10000, total_count);
    }

    public void ReadThread(TacReadWriteLock rw_lock)
    {
        for (int i = 0; i < 1000; i++)
        {
            using (rw_lock.Read())
            {
                // check no writers are active
                Assert.Equal(0, writer_count);

                // increment using interlocked to avoid race condition.               
                // why can a race condition happen? because of the lock's intended behaviour, more than 1
                // reader thread can read at a time, which means two reader threads can increment the reader_count
                // or total_count in the same instant. and even though the lock works as intended the test will
                // come out negative because the counts won't match in the final assert.               

                Interlocked.Increment(ref reader_count);
                Interlocked.Decrement(ref reader_count);

                // add to total
                Interlocked.Increment(ref total_count);
            }
        }
    }
    public void WriteThread(TacReadWriteLock rw_lock)
    {
        for (int i = 0; i < 1000; i++)
        {
            using (rw_lock.Write())
            {
                // check no readers and writers are active
                Assert.Equal(0, reader_count);

                Interlocked.Increment(ref writer_count);
                Assert.Equal(1, writer_count); // check no 2 writters overlaped
                Interlocked.Decrement(ref writer_count);

                // add to total
                Interlocked.Increment(ref total_count);
            }
        }
    }


    // testing writer getting in even though readers keep on coming
    [Fact]
    public void Test_Writer_prefference()
    {
        // arrange
        TacReadWriteLock rw_lock = new TacReadWriteLock();
        ConcurrentQueue<string> thread_order = new ConcurrentQueue<string>();
        Task[] threads = new Task[3];

        // act
        threads[0] = Task.Run(() => EnterReader(thread_order, rw_lock, "reader1", true));
        Thread.Sleep(30); // let the thread initialize and enter lock
        threads[1] = Task.Run(() => EnterWriter(thread_order, rw_lock, "writer"));
        Thread.Sleep(30); // let the thread initialize and enter lock
        threads[2] = Task.Run(() => EnterReader(thread_order, rw_lock, "reader2", false));
        
        Task.WaitAll(threads);

        // assert
        thread_order.TryDequeue(out string first);
        thread_order.TryDequeue(out string second);
        thread_order.TryDequeue(out string third);

        // if writer prefference works correctly this is what should happen:

        // 1. reader thread 1 enters lock, enqueues his name, and sleeps 300 ms
        // 2. in those 300 ms, the writer thread enters and goes to sleep while waiting for reader 1 to finish
        // 3. another reader thread 2 enters but should see that there is a pending writer so he should go to sleep
        // 4. reader 1 finishes, and pulses all waiting
        // 5. reader 2 and the writer wake up but the reader sees that a writer is still pending in line, goes back to sleep
        // 6. writer finaly moves on, enqueues his name, when finishes- pulses that he finished
        // 7. reader 2 wakes up, enqueues his name and finishes

        // the order should be 1. reader1, 2. writer, 3. reader2 if the lock is implemented correctly.
        Assert.Equal("reader1", first);
        Assert.Equal("writer", second);
        Assert.Equal("reader2", third);
    }

    public void EnterReader(ConcurrentQueue<string> queue, TacReadWriteLock rw_lock, string thread_name, bool first_reader)
    {
        using (rw_lock.Read())
        {
            queue.Enqueue(thread_name);
            // keep first reader in the lock for 300ms
            if (first_reader) 
                Thread.Sleep(300);
        }
    }
    public void EnterWriter(ConcurrentQueue<string> queue, TacReadWriteLock rw_lock, string thread_name)
    {
        using (rw_lock.Write())
        {
            queue.Enqueue(thread_name);
            Thread.Sleep(300); // keep writer in the lock for 300ms
        }
    }
}
