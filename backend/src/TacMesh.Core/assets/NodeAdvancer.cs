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
        private const double _DISTANCE_THRESHOLD = 0.01; // 1cm


        /// <summary>
        /// calculates the positions of the node on a path from 1 waypoint to another
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="nodeSpeed"></param>
        public void AdvancePosition(Location startPoint, Location endPoint, double nodeSpeed)
        {
            Task.Run(async () => {
                try
                {
                    // physics parameters
                    double totalDist = startPoint.Distance(endPoint);
                    double totalTime = Kinematics.TotalTime(totalDist, nodeSpeed);
                    double time = 0;

                    // the initial location and previous waypoint start at
                    // the node's initial location
                    Location currentLocation = startPoint;

                    // while didn't reach the endpoint
                    while (currentLocation.Distance(endPoint) > _DISTANCE_THRESHOLD)
                    {
                        // calculate the position at time t
                        currentLocation = Kinematics.PosAt(time, totalTime, startPoint, endPoint);
                        PrintPosition(currentLocation, time);

                        // do something with the position...
                        // for now, this something is invoking a position update event. may be changed...
                        NodePositionUpdate?.Invoke(this, currentLocation);

                        // get next time from converting the ms to seconds
                        // in other words, jump each 10ms
                        time += _FREQUENCY / 1000.0;

                        // sleep for 10ms
                        await Task.Delay(_FREQUENCY);
                    }
                }
                // catch silent exceptions in the background thread
                catch (Exception e)
                {
                    Console.WriteLine($"Failed to calculate positions due to: '{e.Message}'");
                }
            });
        }

        // print location at time t
        private void PrintPosition(Location position, double time) => Console.WriteLine($"Current position at time {time:F3} is: ({position}).\n");
    }
}
