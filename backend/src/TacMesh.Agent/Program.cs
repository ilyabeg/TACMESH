using TacMesh.Core;
using TacMesh.Core.builders;

namespace TacMesh.Agent
{
    public class Program
    { 
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("ERROR: Agent Process arguments did not match");
                return;
            }

            Node node = new Node();
            node.Test();
            Console.ReadKey();

            //PacketHeaderBuilder builder = new PacketHeaderBuilder(new PacketHeader());
            //try
            //{
            //    // serialize test
            //    Console.WriteLine("Packet Header 1\n");

            //    builder.SetProtocolVersion(1.0)
            //    .SetSourceID("ILYA")
            //    .SetHopCount(255)
            //    .SetSenderCounter(255L)
            //    .SetTimeToLive(100L)
            //    .SetMessageType(PacketType.Heartbeat)
            //    .SetMessageID("MSG-2")
            //    .SetDestinationID("BEG")
            //    .SetPriority(1)
            //    .SetLogicalClock(256L);

            //    PacketHeader header = builder.BuildPacketHeader();
            //    Console.WriteLine(header + "\n\n");

            //    // deserialize test
            //    Console.WriteLine("Packet Header 2\n");

            //    PacketHeader deserializedHeader = PacketHeader.Deserialize(header.ArrayList);
            //    Console.WriteLine(deserializedHeader);
            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(e.Message);
            //}
        }
    }
}
