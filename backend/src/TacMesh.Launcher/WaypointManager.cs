using TacMesh.Core.graph_related;
using TacMesh.Launcher.startup;

namespace TacMesh.Launcher
{
    public class WaypointManager
    {
        // TEMPORARILY create static waypoints
        private readonly Dictionary<string, Queue<Location>> _waypoints = new()
        {       
            // the max range for this scenario: 10 m
            // node1 and node2 end as neigbours, 1.41 m apart -> 85.9% of packet pass
            ["Node-01"] = new Queue<Location>([
                new Location(2, 5),
                new Location(3, 8),
                new Location(2, 12)
            ]),
            ["Node-02"] = new Queue<Location>([
                new Location(1, 5),
                new Location(1, 8),
                new Location(1, 11)
            ]),

            // node3 and node4 end as neigbours at the same spot, 0 m apart -> 100% of packet pass
            ["Node-03"] = new Queue<Location>([
                new Location(5, 3),
                new Location(8, 3),
                new Location(11, 2)
            ]),
            ["Node-04"] = new Queue<Location>([
                new Location(5, 1),
                new Location(8, 2),
                new Location(11, 2)
            ]),

            // node5 ends up isolated at (15,15)
            // 13.6 m apart from node3, and node4
            // 13.34 m apart from node1 (can't reach node 2 aswell)
            ["Node-05"] = new Queue<Location>([
                new Location(5, 5),
                new Location(10, 10),
                new Location(15, 15)
            ])
        };

        // agent manager to handle the sending of the waypoint to the nodes
        private readonly AgentManager _agentManager;

        public WaypointManager(AgentManager agentManager)
        {
            // attach waypoint reached event handler to the agent manager
            _agentManager = agentManager;
            _agentManager.WaypointReached += (s, nodeId) => SendAgentNextPos(nodeId);
        }


        // send the agent his next waypooint
        public void SendAgentNextPos(string agentId)
        {
            // pull next waypoint from the agent's waypoints queue
            if (_waypoints[agentId].Count > 0)
            {
                Location nextWaypoint = _waypoints[agentId].Dequeue();

                // convert the position to a string
                string pos = $"{nextWaypoint.X},{nextWaypoint.Y}";

                // push the location to the Node
                _agentManager.MoveAgent(agentId, pos);
            }
            else {
                // temporary check
                Console.WriteLine($"[Waypoint Manager] Node '{agentId}' reached his destination.");
            }
        }

        /*
        // handler to advance the node to his next waypoint upon reaching the previous
        private void OnWaypointReached(string nodeId)
        {
            // Note: this is currently a prototype method in case I were to add 
            // new logic or different handling to a 'Position Reached' event.
            // For now, I simply use the SendAgentNextPos(string agentId) method instead.

            SendAgentNextPos(nodeId);
        }
        */
    }
}
