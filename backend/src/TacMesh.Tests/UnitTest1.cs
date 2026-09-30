using TacMesh.Core;
using TacMesh.Core.serializing_related;
namespace TacMesh.Tests;

public class UnitTest1
{
    [Theory]

    [InlineData(1, "abcdefg", 0, 100L, 255L, PacketType.Heartbeat, "1234567890123456", "hijklmnop", 1, 1234L)]
    [InlineData(1, "abcdefg", 0, 100L, 255L, PacketType.Heartbeat, "1234567890123456", "hijklmnop", 1, 1234L)]

    public void TestPacketHeaderSerialization(
        byte version, string src, byte hop, long sender, long ttl,
        PacketType msgType, string msgId, string dst, byte priority, long clock)
    {
        PacketHeaderSerializer serializer = new PacketHeaderSerializer();

        // original packet header
        PacketHeader header1 = PacketHeader.BuildPacketHeader(version, src, hop, sender, ttl, msgType, msgId, dst, priority, clock);

        // serialized byte stream
        byte[] serialized_header_bytes = serializer.Serialize(header1);

        // new header from the deserialization
        PacketHeader header2 = serializer.Deserialize(serialized_header_bytes);

        Assert.NotNull(header2);
        Assert.Equal(version, header2.ProtocolVersion);
        Assert.Equal(src, header2.SrcID);
        Assert.Equal(hop, header2.HopCount);
        Assert.Equal(sender, header2.SenderCounter);
        Assert.Equal(ttl, header2.TTL);
        Assert.Equal(msgType, header2.MsgType);
        Assert.Equal(msgId, header2.MsgID);
        Assert.Equal(dst, header2.DstID);
        Assert.Equal(priority, header2.Priority);
        Assert.Equal(clock, header2.LogicalClock);
    }
}
