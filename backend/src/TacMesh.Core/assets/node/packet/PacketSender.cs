using System.Net;
using TacMesh.Core.assets.node.packet.buffer;
using TacMesh.Core.interfaces;
using TacMesh.Core.packet;
using TacMesh.Core.tables.node;

namespace TacMesh.Core.assets.node.packet
{
    /// <summary>
    /// The class used to send messages to other end users
    /// </summary>
    public class PacketSender
    {
        // dependencies
        private readonly IByteSerializer<DataPacket> _packetSerializer;
        private readonly NeighborTable _neighborTable;
        private readonly ITransport _transporter;
        private readonly PacketBuffer _packetBuffer;


        // constructor
        public PacketSender(IByteSerializer<DataPacket> packetSerializer, NeighborTable neighborTable, ITransport transporter, PacketBuffer packetBuffer)
        {
            _packetSerializer = packetSerializer;
            _neighborTable = neighborTable;
            _transporter = transporter;
            _packetBuffer = packetBuffer;
        }

        /// <summary>
        /// method used to send a message to the provided peer
        /// </summary>
        /// <param name="packet">The message to send</param>
        /// <param name="exluciveAddress">Non-mandatory address to exclude when sending the packet</param>
        /// <returns>True if the message was sent successfuly, or False otherwise.</returns>
        public bool SendPacket(DataPacket packet, IPEndPoint? exluciveAddress = null)
        {
            // if the provided peer is a current neighbor
            //
            string peerId = packet.PacketHeader.DstID;

            // transmit the packet to all current neighbors
            //
            if (peerId.Equals(PacketHeader.ALL_SPECIFIER))
                return TransmitToAll(packet, exluciveAddress);

            // transmit to specific neighbor
            //
            return ExecuteSend(packet, peerId, exluciveAddress);
        }

        /// <summary>
        /// Transmits the packet to all the current neighbors of this node
        /// </summary>
        /// <param name="packet">The packet to send</param>
        /// <param name="exluciveAddress">Address to exclude when sending the packet</param>
        /// <returns>True if transmitted successfuly to all neighbors or False otherwise</returns>
        private bool TransmitToAll(DataPacket packet, IPEndPoint? exluciveAddress)
        {
            // flag to determine if the packets were sent to all
            //
            bool transmittedToAll = true; // true untill proven false
            List<string> currentNeighbors = _neighborTable.GetCurrentNeighbors();

            foreach (string neighbor in currentNeighbors)
            {
                bool sent = ExecuteSend(packet, neighbor, exluciveAddress);

                if (!sent) transmittedToAll = false;
            }
            return transmittedToAll;
        }

        /// <summary>
        /// Transmits the packet to the specific neighbor
        /// </summary>
        /// <param name="packet">The packet to send</param>
        /// <param name="peerId">The specified peer</param>
        /// <param name="exluciveAddress">Address to exclude when sending the packet</param>
        /// <returns>True if transmitted successfuly or False otherwise</returns>
        private bool ExecuteSend(DataPacket packet, string peerId, IPEndPoint? exluciveAddress)
        {
            IPEndPoint? peerAddress = _neighborTable.GetNeigborAddress(peerId);
            
            if (peerAddress != null)
            {
                // exclude this transmission if the address is the excluded one
                //
                if (exluciveAddress != null && peerAddress == exluciveAddress) 
                    return false;

                byte[] packetBytes = _packetSerializer.Serialize(packet);

                // if message was sent successfuly, exit the function
                //
                if (_transporter.Transmit(packetBytes, peerAddress) != -1)
                    return true;
            }

            // if peer isn't currently a neighbor OR the message could not be sent,
            // AND it is a message with a specific destination id, then stash the
            // message in the buffer until a suited connection evaluates
            //
            if (!packet.PacketHeader.DstID.Equals(PacketHeader.ALL_SPECIFIER)) 
                _packetBuffer.Add(packet);

            return false;
        }
    }
}
