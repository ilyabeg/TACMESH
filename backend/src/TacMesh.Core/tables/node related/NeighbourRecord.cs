using System.Net;

namespace TacMesh.Core.tables.node_related
{
    public class NeighbourRecord
    {
        public string NeighbourID { get; private set; }
        public IPEndPoint Address { get; private set; }
        //public ILocation Location { get; private set; } // location implementation comes in the future
        public DateTime LastHeartbeatTime { get; private set; }

        public NeighbourRecord(string id, IPEndPoint address, DateTime heartbeat_time)
        {
            NeighbourID = id;
            Address = address;
            //Location = location;
            LastHeartbeatTime = heartbeat_time;
        }
    }
}
