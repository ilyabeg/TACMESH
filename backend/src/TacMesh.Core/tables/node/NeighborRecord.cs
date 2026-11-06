using System.Net;
using TacMesh.Core.assets.graph;

namespace TacMesh.Core.tables.node
{
    public class NeighborRecord
    {
        public string NeighborID { get; private set; }
        public IPEndPoint Address { get; private set; }
        public Location Location { get; private set; }
        public DateTime LastHeartbeatTime { get; private set; }

        public NeighborRecord(string id, IPEndPoint address, Location location, DateTime heartbeat_time)
        {
            NeighborID = id;
            Address = address;
            Location = location;
            LastHeartbeatTime = heartbeat_time;
        }
    }
}
