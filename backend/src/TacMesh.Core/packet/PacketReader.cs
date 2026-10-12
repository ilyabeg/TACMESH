using TacMesh.Core.events;
using TacMesh.Core.serializers;

namespace TacMesh.Core.packet_related
{
    public class PacketReader
    {
        private static readonly PacketHeaderSerializer _serializer = new PacketHeaderSerializer();

        // IMPORTANT NOTE: THIS METHOD WILL EVENTUALLY RETURN A DATAPACKET OBJECT BUT FOR NOW
        // BECAUSE I HAVEN'T IMPLEMENTED THE DATAPACKET CLASS YET, I WILL RETURN PACKETHEADER. THIS IS A TEMPORARY
        // IMPLEMENTATION AND WILL BE CHANGED IN THE FUTURE, FOR NOW I WILL DESERIALIZE THE HEADER AND RETURN.
        public static PacketHeader ReadPacket(MessageReceivedEventArgs e)
        {
            // read the header first            
            PacketHeader header = _serializer.Deserialize(e.MessageBytes);

            // print the header 
            //Console.WriteLine($"Received Packet from PORT {e.RemoteEndPoint.Port}");

            return header;
        }
    }
}
