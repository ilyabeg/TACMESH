using TacMesh.Core.database_related;
using TacMesh.Core.events;
using TacMesh.Core.serializers;

namespace TacMesh.Core.packet_related
{
    public class PacketReader
    {

        // IMPORTANT NOTE: THIS METHOD WILL EVENTUALLY RETURN A DATAPACKET OBJECT BUT FOR NOW
        // BECAUSE I HAVEN'T IMPLEMENTED THE DATAPACKET CLASS YET, I WILL RETURN VOID. THIS IS A TEMPORARY
        // IMPLEMENTATION AND WILL BE CHANGED IN THE FUTURE, FOR NOW I WILL DESERIALIZE THE HEADER AND PRINT.
        public static void ReadPacket(MessageReceivedEventArgs e)
        {
            // read the header first
            PacketHeaderSerializer serializer = new PacketHeaderSerializer();
            PacketHeader header = serializer.Deserialize(e.MessageBytes);

            // print the header 
            Console.WriteLine($"Received PacketHeader from PORT {e.RemoteEndPoint.Port}:\n\n" + header);

            // log if packet is heartbeat
            if (header.MsgType == PacketType.Heartbeat)
                Logger.LogHeartbeat(LoggingMode.Received, header.SrcID, header.DstID, DateTime.Now);
        }
    }
}
