using TacMesh.Core;
using TacMesh.Core.builders;

namespace TacMesh.Agent
{
    public class Program
    { 
        static void Main(string[] args)
        {
            //Node node = new Node();
            //node.Test();
            //Console.ReadKey();

            PacketHeaderBuilder builder = new PacketHeaderBuilder(new PacketHeader());
            try
            {
                // serialize test
                Console.WriteLine("Packet Header 1\n");

                builder.SetProtocolVersion(1.0)
                .SetSourceID("ilya")
                .SetHopCount()
                .SetSenderCounter(66)
                .SetTimeToLive(1257)
                .SetMessageType(PacketType.Heartbeat)
                .SetMessageID("MESSAGE1")
                .SetDestinationID("Computer01")
                .SetPriority(1)
                .SetLogicalClock(256);

                PacketHeader header = builder.BuildPacketHeader();
                Console.WriteLine(header + "\n\n");

                // deserialize test
                Console.WriteLine("Packet Header 2\n");

                PacketHeader deserializedHeader = PacketHeader.Deserialize(header.ArrayList);
                Console.WriteLine(deserializedHeader);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
