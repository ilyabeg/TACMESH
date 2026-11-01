using TacMesh.Core.packet_related;

namespace TacMesh.Core.assets.node
{
    public class PacketBuffer
    {
        // the maximum amount of messages that can be stored
        private readonly int _BUFFER_CAPACITY = 100;
        private readonly List<DataPacket> _buffer;


        // constructor
        public PacketBuffer()
        {
            _buffer = new List<DataPacket>(_BUFFER_CAPACITY);
        }

        // adds a message to the buffer
        public void Add(DataPacket packet)
        {
            // add the packet if 
            if (!_buffer.Contains(packet))
            {
                _buffer.Add(packet);
            }
        }
    }
}
