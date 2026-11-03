using System.Net;
using TacMesh.Core.interfaces;
using TacMesh.Core.packet_related;
using TacMesh.Core.tables.node_related;
using TacMesh.Core.utils.serializers;

namespace TacMesh.Core.assets.node.packet
{
    /// <summary>
    /// The class used to send messages to other end users
    /// </summary>
    public class PacketSender
    {
        // dependencies
        private readonly NeighborTable _neighborTable;
        private readonly ITransport _transporter;
        private readonly PacketBuffer _packetBuffer;


        // constructor
        public PacketSender(NeighborTable neighborTable, ITransport transporter, PacketBuffer packetBuffer)
        {
            _neighborTable = neighborTable;
            _transporter = transporter;
            _packetBuffer = packetBuffer;

            // TESTING PACKET BUFFER LOGIC UPON MEETING NEW NEIGHBOR
            _neighborTable.NewNeighborDiscovered += (s,e) => OnNewNeighborDiscovered(e);
        }

        /// <summary>
        /// method used to send a message to the provided peer
        /// </summary>
        /// <param name="packet">The message to send</param>
        /// <returns>True if the message was sent successfuly, or False otherwise.</returns>
        public bool SendPacket(DataPacket packet)
        {
            // if the provided peer is a current neighbor
            //
            string peerId = packet.PacketHeader.DstID;
            IPEndPoint? peerAddress = _neighborTable.GetNeigborAddress(peerId);

            if (peerAddress != null)
            {
                // TEMPORARILY transmit simply the message bytes, later the payload will be encrypted
                //
                byte[] packetBytes = PacketSerializer.Serialize(packet);

                // if message was sent successfuly, exit the function
                //
                if (_transporter.Transmit(packetBytes, peerAddress) != -1)
                    return true;
            }

            // if peer isn't currently a neighbor OR the message could not be sent,
            // stash the message in the buffer until a suited connection evaluates
            //
            _packetBuffer.Add(packet);
            return false;
        }



        // TMP TESTING
        private void OnNewNeighborDiscovered(NeighborRecord newNeighborRecord)
        {
            // NOTE: this test simply tests out the packet buffer logic. it does not test multi-hop forwards
            // upon discovering a new neighbor.

            Console.WriteLine($"[PACKET SENDER] New Neighbor Discovered: \"{newNeighborRecord.NeighborID}\"");

            if (_packetBuffer.GetRecipientMessages(newNeighborRecord.NeighborID, out Queue<DataPacket>? messages))
            {
                Console.WriteLine($"[PACKET SENDER] This Node has {messages!.Count} messages to the neighbor \"{newNeighborRecord.NeighborID}\"");
                int count = 1;

                while (messages!.Count > 0)
                {
                    bool res = SendPacket(messages.Dequeue());
                    Console.WriteLine($"[PACKET SENDER] Message #{count} forward {(res ? "Success!" : "Failed.")}");
                }
            }
        }
    }
}
