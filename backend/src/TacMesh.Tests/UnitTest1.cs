using TacMesh.Core;
using TacMesh.Core.serializers;
namespace TacMesh.Tests;

public class UnitTest1
{
    [Theory]

    // hop count 255 test
    [InlineData((byte)1, PacketType.Heartbeat, "11111111-1111-1111-1111-111111111111", "node-a", "node-b", (byte)255, 1234L, 255L, 100L, (byte)1)]
    // unknown message type
    [InlineData((byte)1, (PacketType)3, "22222222-2222-2222-2222-222222222222", "node-a", "node-b", (byte)255, 1234L, 255L, 100L, (byte)1)]
    // unkown protocol version
    [InlineData((byte)2, PacketType.Heartbeat, "33333333-3333-3333-3333-333333333333", "node-a", "node-b", (byte)255, 1234L, 255L, 100L, (byte)1)]

    // Round trip: serialize then deserialize returns an identical object, for each of the three packet types.
    [InlineData((byte)1, PacketType.Heartbeat, "44444444-4444-4444-4444-444444444444", "node-a", "node-b", (byte)1, 1234L, 255L, 100L, (byte)1)]
    [InlineData((byte)1, PacketType.LinkState, "55555555-5555-5555-5555-555555555555", "node-a", "node-b", (byte)1, 1234L, 255L, 100L, (byte)1)]
    [InlineData((byte)1, PacketType.UserMessage, "66666666-6666-6666-6666-666666666666", "node-a", "node-b", (byte)1, 1234L, 255L, 100L, (byte)1)]

    public void TestPacketHeaderSerialization(
            byte version, PacketType msgType,
            string str, string src, string dst,
            byte hop, long clock,
            long ttl, long sender, byte priority)
    {
        PacketHeaderSerializer serializer = new PacketHeaderSerializer();
        Guid msgId = Guid.Parse(str);

        // original packet header
        PacketHeader header1 = new PacketHeader(version, msgType, msgId, src, dst, hop, clock, ttl, priority, sender);

        // serialized byte stream
        byte[] serialized_header_bytes = serializer.Serialize(header1);

        // new header from the deserialization
        PacketHeader header2 = serializer.Deserialize(serialized_header_bytes);

        Assert.NotNull(header2);
        Assert.Equal(version, header2.ProtocolVersion);
        Assert.Equal(msgType, header2.MsgType);
        Assert.Equal(msgId, header2.MsgID);
        Assert.Equal(src, header2.SrcID);
        Assert.Equal(dst, header2.DstID);
        Assert.Equal(hop, header2.HopCount);
        Assert.Equal(clock, header2.LogicalClock);
        Assert.Equal(ttl, header2.TTL);
        Assert.Equal(priority, header2.Priority);
        Assert.Equal(sender, header2.SenderCounter);
    }

    [Theory]
    // test a packet header (as bytes) shorter than 76 bytes
    [InlineData(new byte[] { 1, 2, 3, 4, 5 })]

    public void TestShortByteStream(byte[] byteStream)
    {
        PacketHeaderSerializer serializer = new PacketHeaderSerializer();

        PacketHeader header2 = serializer.Deserialize(byteStream);
        Console.WriteLine(header2);

        Assert.NotNull(header2);
    }
}
