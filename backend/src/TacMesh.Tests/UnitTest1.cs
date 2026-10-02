using System.Net;
using TacMesh.Core.tables.node_related;

namespace TacMesh.Tests;

public class UnitTest1
{
    [Fact]
    public void Test_Add_Neighbours_to_Table()
    {
        // --- Arrange ---
        TestClock clock = new TestClock();
        clock.Time = new DateTime(2026, 10, 2, 12, 0, 0);
        
        NeighbourTable table = new NeighbourTable(clock); // inject clock

        IPEndPoint ep1 = new IPEndPoint(IPAddress.Loopback, 51001);
        IPEndPoint ep2 = new IPEndPoint(IPAddress.Loopback, 51002);
        IPEndPoint ep3 = new IPEndPoint(IPAddress.Loopback, 51003);

        // --- Act ---
        // add nodes 1, 2, 3 records
        table.UpdateRecord("Node-01", ep1);
        table.UpdateRecord("Node-02", ep2);
        table.UpdateRecord("Node-03", ep3);

        // --- Assert ---
        Assert.True(
            table.NeighbourRecords.ContainsKey("Node-01") && 
            table.NeighbourRecords.ContainsKey("Node-02") && 
            table.NeighbourRecords.ContainsKey("Node-03")
        );
    }

    [Fact]
    public void Test_Update_Neighbours_of_Table()
    {
        // --- Arrange ---
        TestClock clock = new TestClock();
        clock.Time = new DateTime(2026, 10, 2, 12, 0, 0);

        NeighbourTable table = new NeighbourTable(clock); // inject clock

        IPEndPoint ep1 = new IPEndPoint(IPAddress.Loopback, 51001);
        IPEndPoint ep2 = new IPEndPoint(IPAddress.Loopback, 51002);
        IPEndPoint ep3 = new IPEndPoint(IPAddress.Loopback, 51003);

        // add nodes 1, 2, 3 records
        table.UpdateRecord("Node-01", ep1);
        table.UpdateRecord("Node-02", ep2);
        table.UpdateRecord("Node-03", ep3);

        // add 5 seconds
        clock.Time = clock.Time.AddSeconds(5);

        // --- Act ---
        // update nodes 1, 2, 3 records. last heartbeat supposed to change to the new clock time
        table.UpdateRecord("Node-01", ep1);
        table.UpdateRecord("Node-02", ep2);
        table.UpdateRecord("Node-03", ep3);

        // --- Assert ---
        Assert.True(
            table.NeighbourRecords["Node-01"].LastHeartbeatTime.Equals(clock.Time) &&
            table.NeighbourRecords["Node-02"].LastHeartbeatTime.Equals(clock.Time) &&
            table.NeighbourRecords["Node-03"].LastHeartbeatTime.Equals(clock.Time)
        );
    }

    [Fact]
    public void Test_Neighbour_Table_Expiration()
    {
        // --- Arrange ---
        TestClock clock = new TestClock();
        clock.Time = new DateTime(2026,10,2,12,0,0);

        NeighbourTable table = new NeighbourTable(clock); // inject clock

        // node 1 should not be in the table. his last heartbeat was at: 2026/10/2 - 12:00:00
        // which means 3 clock cycles have passed.
        table.UpdateRecord("Node-01", new IPEndPoint(IPAddress.Loopback, 51001));

        // add 15 seconds
        clock.Time = clock.Time.AddSeconds(15);

        // node 2 and 3 are good. their last heartbeat was at: 2026/10/2 - 12:00:15
        table.UpdateRecord("Node-02", new IPEndPoint(IPAddress.Loopback, 51002));
        table.UpdateRecord("Node-03", new IPEndPoint(IPAddress.Loopback, 51003));

        // --- Act ---
        table.CheckExpiration(); // check for expired neighbours

        // --- Assert ---
        Assert.False(table.NeighbourRecords.ContainsKey("Node-01")); // check if false
    }
}
