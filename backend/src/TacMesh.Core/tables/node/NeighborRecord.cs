using System.Net;

namespace TacMesh.Core.tables.node_related
{
    public class NeighborRecord
    {
        public string NeighborID { get; private set; }
        public IPEndPoint Address { get; private set; }
        //public ILocation Location { get; private set; } // location implementation comes in the future
        public DateTime LastHeartbeatTime { get; private set; }

        public NeighborRecord(string id, IPEndPoint address, DateTime heartbeat_time)
        {
            NeighborID = id;
            Address = address;
            //Location = location;
            LastHeartbeatTime = heartbeat_time;
        }
    }
}
