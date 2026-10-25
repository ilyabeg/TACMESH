using System.Net;
using System.Net.Sockets;
using TacMesh.Core;
using TacMesh.Core.communication.simulation_communication;
using TacMesh.Core.events;
using TacMesh.Core.hardware_mode;
using TacMesh.Core.interfaces;
using TacMesh.Core.interfaces.communication_interfaces;
using TacMesh.Core.utils;
using TacMesh.Core.utils.packet;
using TacMesh.Core.utils.serializers;

namespace TacMesh.Tests;

public class UnitTest1
{
    // temporary class for the test. doesn't do anything
    internal class TempRadioModel : IRadioModel
    {
        public Dictionary<string, IPEndPoint> GetPeers() => new Dictionary<string, IPEndPoint>();

        public Task<bool> TestConnection(IPEndPoint source, IPEndPoint destination) => Task.FromResult(true);
    }

    // TEMPORARY class to simpy test the topology filter without many dipendencies
    internal class MockPacketProcessor
    {
        // indicator to check if passed or not
        public bool PacketPassed { get; private set; } = false;
        private readonly TopologyFilter _tf;

        public MockPacketProcessor(TopologyFilter tf)
        {
            _tf = tf;
        }

        public bool ProcessPacket(MessageReceivedEventArgs e)
        {
            PacketHeader header = PacketReader.ReadPacket(e);

            // drop packet if it is from an un-permitted neighbor before processing
            if (!_tf.Filter(header.SrcID))
            {
                Console.WriteLine($"Dropping Packet from source \"{header.SrcID}\" due to: un-permitted neighbor");
                return false;
            }

            // if allowed to process packet return true
            PacketPassed = true;
            return true;
        }
    }


    [Fact]
    public void Test_Topology_Filter_Permitted()
    {
        // --- Arrange --- 

        // build the topology filter with a permitted node
        TopologyFilter TF = new TopologyFilter();
        string permittedID = "PERMITTED-ENGINE";
        TF.AddPermittedNeighbor(permittedID);

        // engine who receives the test packet on port 51001
        int enginePort = 51001;
        IPEndPoint engineEndPoint = new IPEndPoint(IPAddress.Loopback, enginePort);
        Socket engine = GetEngineSocket(enginePort);
        ITransport engineTransport = new SimTransport(engine, new TempRadioModel());
        engineTransport.StartReceiving(); // start listening

        // NOTE: This is simply to test out the topology filter working without having the 
        // processor take many dependencies. the logic in the topology filter is what matters
        MockPacketProcessor processor = new MockPacketProcessor(TF);

        // let the engine hear packets and process them 
        engineTransport.MessageReceived += (s, e) => processor.ProcessPacket(e);

        // engine who is permitted for the engine node
        Socket permitted_engine = GetEngineSocket(0);
        ITransport permittedEngineTransport = new SimTransport(permitted_engine, new TempRadioModel());

        // create the packet from the permitted node
        byte[] headerBytes = CreatePacket(permittedID);


        // --- Act --- 

        // transmit the peritted packet to the engine
        permittedEngineTransport.Transmit(headerBytes, engineEndPoint);
        Thread.Sleep(50);


        // --- Assert --- 

        // verify the packet was received and permitted
        Assert.True(processor.PacketPassed);
    }

    static Socket GetEngineSocket(int port)
    {
        Socket s = SocketGenerator.GenerateUdpSocket();
        SocketGenerator.BindSocket(s, IPAddress.Loopback, port);
        return s;
    }

    static byte[] CreatePacket(string sourceId)
    {
        PacketHeaderSerializer serializer = new PacketHeaderSerializer();
        PacketHeader header = new PacketHeader(
            1,
            PacketType.Heartbeat,
            Guid.NewGuid(),
            sourceId,
            "temp",
            1,
            1,
            1,
            1,
            1
        );
        return serializer.Serialize(header);
    }


    [Fact]
    public void Test_Topology_Filter_Denied()
    {
        // --- Arrange --- 

        // build the topology filter without permitted nodes
        TopologyFilter TF = new TopologyFilter();        

        // engine who receives the test packet on port 51002
        int enginePort = 51002;
        IPEndPoint engineEndPoint = new IPEndPoint(IPAddress.Loopback, enginePort);
        Socket engine = GetEngineSocket(enginePort);
        ITransport engineTransport = new SimTransport(engine, new TempRadioModel());
        engineTransport.StartReceiving(); // start listening

        // NOTE: This is simply to test out the topology filter working without having the 
        // processor take many dependencies. the logic in the topology filter is what matters
        MockPacketProcessor processor = new MockPacketProcessor(TF);

        // let the engine hear packets and process them 
        engineTransport.MessageReceived += (s, e) => processor.ProcessPacket(e);

        // engine who is NOT permitted for the engine node
        Socket denied_engine = GetEngineSocket(0);
        ITransport deniedEngineTransport = new SimTransport(denied_engine, new TempRadioModel());

        // create the packet from the denied node
        byte[] headerBytes = CreatePacket("DENIED-ENGINE");


        // --- Act --- 

        // transmit the packet to the engine
        deniedEngineTransport.Transmit(headerBytes, engineEndPoint);
        Thread.Sleep(50);


        // --- Assert --- 

        // verify the packet was received and DENIED
        Assert.False(processor.PacketPassed);
    }


    ////counters to ensure lock is working correctly, when a reader is reading
    ////writer count should be 0, and when a writer is writing, reader count
    //// should be 0 and writer count should be 1
    //int reader_count;
    //int writer_count;
    //int total_count;

    //[Fact]
    //public void Test_Read_Write_Lock_Stress()
    //{
    //    for (int repeat = 0; repeat < 20; repeat++)
    //    {
    //        // Arrange
    //        reader_count = 0;
    //        writer_count = 0;
    //        total_count = 0;
    //        TacReadWriteLock rw_lock = new TacReadWriteLock();
    //        Task[] threads = new Task[10];

    //        // 8 readers
    //        for (int i = 0; i < 8; i++)
    //        {
    //            threads[i] = Task.Run(() => ReadThread(rw_lock));
    //        }
    //        // 2 writers
    //        threads[8] = Task.Run(() => WriteThread(rw_lock));
    //        threads[9] = Task.Run(() => WriteThread(rw_lock));

    //        // Acr
    //        // 10.000 operations (10 threads * 1000 iterations each)
    //        Task.WaitAll(threads);

    //        // Assert
    //        Assert.Equal(10000, total_count);
    //    }
    //}

    //public void ReadThread(TacReadWriteLock rw_lock)
    //{
    //    for (int i = 0; i < 1000; i++)
    //    {
    //        using (rw_lock.Read())
    //        {
    //            // check no writers are active
    //            Assert.Equal(0, writer_count);

    //            // increment using interlocked to avoid race condition.               
    //            // why can a race condition happen? because of the lock's intended behaviour, more than 1
    //            // reader thread can read at a time, which means two reader threads can increment the reader_count
    //            // or total_count in the same instant. and even though the lock works as intended the test will
    //            // come out negative because the counts won't match in the final assert.

    //            // IMPORTANT NOTE: I AM AWARE THAT USING A READER THREAD THIS WAY IS STRICTLY FORBIDDEN.
    //            // THIS GOES AGAINST ALL RULES OF A READER: A READER SHOULD NEVER ALTER DATA WHILE READING.
    //            // THE ONLY REASON I AM DOING THIS OPERATION THIS WAY IS BECAUSE THE COUNTERS THAT THE 
    //            // THREAD ALTERS (READER AND TOTAL COUNTS) IS NOT ACTUAL, REAL USE CASE, DATA THAT A REAL
    //            // WORKING LOCK IN THE ACTUAL SYSTEM WILL GUARD. THIS IS STRICTLY TEST VARIABLES USED TO
    //            // EXAMINE THE LOCK'S CORRECTNESS, BY EXAMINING THAT NO WRITER THREADS ARE ACTIVE DURING
    //            // A READER THREAD, AND NO READER THREAD (OR AN OTHER WRITER) IS ACTIVE DURING A WRITER THREAD.
    //            // IN THE REAL SYSTEM THIS TYPE OF BEHAVIOUR WILL NEVER BE USED, THIS IS STRICTLY FOR TESTING.

    //            Interlocked.Increment(ref reader_count);
    //            Interlocked.Decrement(ref reader_count);

    //            // add to total
    //            Interlocked.Increment(ref total_count);
    //        }
    //    }
    //}
    //public void WriteThread(TacReadWriteLock rw_lock)
    //{
    //    for (int i = 0; i < 1000; i++)
    //    {
    //        using (rw_lock.Write())
    //        {
    //            // check no readers and writers are active
    //            Assert.Equal(0, reader_count);

    //            Interlocked.Increment(ref writer_count);
    //            Assert.Equal(1, writer_count); // check no 2 writters overlaped
    //            Interlocked.Decrement(ref writer_count);

    //            // add to total
    //            Interlocked.Increment(ref total_count);
    //        }
    //    }
    //}


    //// testing writer getting in even though readers keep on coming
    //[Fact]
    //public void Test_Writer_prefference()
    //{
    //    for (int repeat = 0; repeat < 20; repeat++)
    //    {
    //        // arrange
    //        TacReadWriteLock rw_lock = new TacReadWriteLock();
    //        ConcurrentQueue<string> thread_order = new ConcurrentQueue<string>();
    //        Task[] threads = new Task[3];

    //        // act
    //        threads[0] = Task.Run(() => EnterReader(thread_order, rw_lock, "reader1", true));
    //        Thread.Sleep(30); // let the thread initialize and enter lock
    //        threads[1] = Task.Run(() => EnterWriter(thread_order, rw_lock, "writer"));
    //        Thread.Sleep(30); // let the thread initialize and enter lock
    //        threads[2] = Task.Run(() => EnterReader(thread_order, rw_lock, "reader2", false));

    //        Task.WaitAll(threads);

    //        // assert
    //        thread_order.TryDequeue(out string first);
    //        thread_order.TryDequeue(out string second);
    //        thread_order.TryDequeue(out string third);

    //        // if writer prefference works correctly this is what should happen:

    //        // 1. reader thread 1 enters lock, enqueues his name, and sleeps 300 ms
    //        // 2. in those 300 ms, the writer thread enters and goes to sleep while waiting for reader 1 to finish
    //        // 3. another reader thread 2 enters but should see that there is a pending writer so he should go to sleep
    //        // 4. reader 1 finishes, and pulses all waiting
    //        // 5. reader 2 and the writer wake up but the reader sees that a writer is still pending in line, goes back to sleep
    //        // 6. writer finaly moves on, enqueues his name, when finishes- pulses that he finished
    //        // 7. reader 2 wakes up, enqueues his name and finishes

    //        // the order should be 1. reader1, 2. writer, 3. reader2 if the lock is implemented correctly.
    //        Assert.Equal("reader1", first);
    //        Assert.Equal("writer", second);
    //        Assert.Equal("reader2", third);
    //    }        
    //}

    //public void EnterReader(ConcurrentQueue<string> queue, TacReadWriteLock rw_lock, string thread_name, bool first_reader)
    //{
    //    using (rw_lock.Read())
    //    {
    //        queue.Enqueue(thread_name);
    //        // keep first reader in the lock for 300ms
    //        if (first_reader) 
    //            Thread.Sleep(300);
    //    }
    //}
    //public void EnterWriter(ConcurrentQueue<string> queue, TacReadWriteLock rw_lock, string thread_name)
    //{
    //    using (rw_lock.Write())
    //    {
    //        queue.Enqueue(thread_name);
    //    }
    //}
}
