using System.Collections.Concurrent;
using TacMesh.Core.assets.clocks;
using TacMesh.Core.interfaces;
using TacMesh.Core.packet;
using TacMesh.Core.packet_related;

namespace TacMesh.Core.assets.node.packet.validation
{
    /// <summary>
    /// Packet validation class responsible for validating if a packet is valid or not.
    /// Open for more validation chhecks in the future
    /// </summary>
    public class PacketValidator : IPacketValidator
    {
        private readonly LamportClock _lamportClock; // local clock state to sync with others
        private readonly ConcurrentDictionary<string, ValidationRecord> _userValidationRecords; // save user validation data


        // constructor
        public PacketValidator(LamportClock lamportClock)
        {
            _userValidationRecords = new ConcurrentDictionary<string, ValidationRecord>();
            _lamportClock = lamportClock;
        }

        /// <summary>
        /// Standard validation function. Validates the neccessary packet header fields 
        /// for each packet which is received
        /// </summary>
        /// <param name="packet">The packet to validate</param>
        /// <returns>True if the packet is valid or False otherwise</returns>
        public bool Validate(DataPacket packet)
        {
            // if a new record added,it can't be invalid
            if (NewUser(packet)) return true;

            // check packet version
            if (packet.PacketHeader.ProtocolVersion != PacketHeader.PROTOCOL_VERSION) 
                return false;

            // check hop count
            if (packet.PacketHeader.HopCount >= PacketHeader.MAX_HOP)
                return false;

            // check sender counter
            // To Do ...

            // if the packet is valid, sync the local lamport clock
            //
            _lamportClock.Sync(packet.PacketHeader.LogicalClock);
            return true;
        }

        /// <summary>
        /// Topology related validation function. Validates if the received packet, which is
        /// related to topolofy updates, is valid in terms of events order
        /// </summary>
        /// <param name="packet">The packet to validate</param>
        /// <returns>True if the packet is valid or False otherwise</returns>
        public bool ValidateLogicalClock(DataPacket packet)
        {
            // if a new record added,it can't be invalid
            if (NewUser(packet)) return true;

            // try to update the new value
            string source = packet.PacketHeader.SrcID;
            long newValue = packet.PacketHeader.LogicalClock;

            // return true if successfuly updated
            //
            if (_userValidationRecords[source].TryUpdateClock(newValue))
                return true;

            return false;
        }

        /// <summary>
        /// Adds a new user if he does not exist yet and syncs with his clock
        /// </summary>
        /// <param name="packet">The new receive packet</param>
        /// <returns>True if the a new remote user was added and synced with, or False otherwise</returns>
        private bool NewUser(DataPacket packet)
        {
            string source = packet.PacketHeader.SrcID;

            // init with the value of the current logical clock - 1, so the first ever record
            // doesn't get dropped immedietly after the standard validation
            //
            if (_userValidationRecords.TryAdd(source, new ValidationRecord(packet.PacketHeader.LogicalClock - 1)))
            {
                // sync clocks ...
                //
                _lamportClock.Sync(packet.PacketHeader.LogicalClock);
                return true;
            }                

            return false;
        }
    }
}
