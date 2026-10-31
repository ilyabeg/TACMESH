using TacMesh.Core.assets.node;
using TacMesh.Core.graph_related;
using TacMesh.Core.utils.physics;

namespace TacMesh.Core.assets
{
    public class NodeAdvancer
    {
        // calculate location each 10ms
        private const int _FREQUENCY = 10;
        public event EventHandler<Location>? NodePositionUpdate;

        // the minimum threshold for two locations to be considered the same
        private const double _DISTANCE_THRESHOLD = 0.001;

        // hold reference to node configurations to access waypoints
        private readonly NodeConfigs _nodeConfigs;


        // constructor
        public NodeAdvancer(NodeConfigs nodeConfigs)
        {
            _nodeConfigs = nodeConfigs;
        }

        /// <summary>
        /// calculates the positions of the node on a path from 1 waypoint to another
        /// </summary>
        /// <returns>A result of True if the node successfuly advanced to the endpoint, or False otherwise.</returns>
        public async Task<bool> AdvancePosition()
        {
            try
            {                
                // advance all known waypoints
                while (_nodeConfigs.Waypoints.Count > 0)
                {
                    // the initial location and previous waypoint start at
                    // the node's initial location
                    Location startPoint = _nodeConfigs.Location;
                    Location currentLocation = _nodeConfigs.Location;
                    Location endPoint = _nodeConfigs.Waypoints.Dequeue(); // endpoint is the next waypoint

                    // physics parameters
                    double totalDist = currentLocation.Distance(endPoint);
                    double totalTime = Kinematics.TotalTime(totalDist, _nodeConfigs.Speed);
                    double time = 0; // time in ms

                    // while didn't reach the endpoint (or advance time is less than the total time)
                    while (currentLocation.Distance(endPoint) > _DISTANCE_THRESHOLD && time <= totalTime)
                    {
                        // calculate the position at time t
                        currentLocation = Kinematics.PosAt(time, totalTime, startPoint, endPoint);
                        //PrintPosition(currentLocation, time);

                        // invokes the current position every 10ms tick (MAY CAUSE LAG IN THE FUTURE)
                        NodePositionUpdate?.Invoke(this, currentLocation);

                        // get next time from converting the ms to seconds
                        // in other words, jump each 10ms
                        time += _FREQUENCY / 1000.0;

                        // sleep for 10ms
                        await Task.Delay(_FREQUENCY);
                    }
                }
                // if positions successfully advanced, return true
                return true;
            }
            // catch silent exceptions in the background thread
            catch (Exception e)
            {
                // if position failed to advanced, return false
                Console.WriteLine($"Failed to calculate positions due to: '{e.Message}'");
                return false;
            }
        }

        // print location at time t
        private void PrintPosition(Location position, double time) => Console.WriteLine($"Current position at time {time:F3} is: {position}.\n");
    }
}
