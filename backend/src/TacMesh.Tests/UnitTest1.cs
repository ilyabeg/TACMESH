using TacMesh.Core;
namespace TacMesh.Tests;

public class UnitTest1
{
    [Theory]

    // roundtrip tests for each of the three packet types returns identical objects
    [InlineData(1.0, "abcdefg", 0, 100L, 255L, 0, "MSG-01", "hijklmnop", 1, 1234L)]
    [InlineData(1.0, "ilya", 3, 10L, 256L, 1, "MSG-02", "beg", 10, 4321L)]
    [InlineData(1.0, "pc1", 100, 160L, 15L, 2, "MSG-03", "pc2", 5, 1551L)]

    public void Test1(  
        double version, string src, int hop, long sender, long ttl,
        int msgType, string msgId, string dst, int priority, long clock)
    {
        PacketHeader header1 = PacketHeader.BuildPacketHeader(
            version, src, hop, sender, ttl, (PacketType)msgType, msgId, dst, priority, clock);

        PacketHeader header2 = PacketHeader.Deserialize(header1.ArrayList);

        Assert.NotNull(header2);
        Assert.Equal(version, header2.ProtocolVersion);
        Assert.Equal(src, header2.SrcID);
        Assert.Equal(hop, header2.HopCount);
        Assert.Equal(sender, header2.SenderCounter);
        Assert.Equal(ttl, header2.TTL);
        Assert.Equal((PacketType)msgType, header2.MsgType);
        Assert.Equal(msgId, header2.MsgID);
        Assert.Equal(dst, header2.DstID);
        Assert.Equal(priority, header2.Priority);
        Assert.Equal(clock, header2.LogicalClock);
    }
}
