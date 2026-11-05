using TacMesh.Core.packet_related;

namespace TacMesh.Core.interfaces
{
    /// <summary>
    /// Defines the packet validation rules
    /// </summary>
    public interface IPacketValidator
    {
        bool Validate(DataPacket packet); // standard validation. validations for all packets
        bool ValidateLogicalClock(DataPacket packet); // validation for topology related packets
    }
}
