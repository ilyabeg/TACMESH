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


        /// <summary>
        /// calculates the positions of the node on a path from 1 waypoint to another
        /// </summary>
        /// <param name="startPoint"></param>
        /// <param name="endPoint"></param>
        /// <param name="nodeSpeed"></param>
        /// <returns>A result of True if the node successfuly advanced to the endpoint, or False otherwise.</returns>
        public async Task<bool> AdvancePosition(Location startPoint, Location endPoint, double nodeSpeed)
        {
            try
            {
                // physics parameters
                double totalDist = startPoint.Distance(endPoint);
                double totalTime = Kinematics.TotalTime(totalDist, nodeSpeed);
                double time = 0; // time in ms
                int ticks = 0;   // ticks representing the amount of 10ms 'jumps' performed (1 tick = 10 ms)

                // the initial location and previous waypoint start at
                // the node's initial location
                Location currentLocation = startPoint;

                // while didn't reach the endpoint (or advance time is less than the total time)
                while (currentLocation.Distance(endPoint) > _DISTANCE_THRESHOLD && time <= totalTime)
                {
                    // calculate the position at time t
                    currentLocation = Kinematics.PosAt(time, totalTime, startPoint, endPoint);
                    //PrintPosition(currentLocation, time);

                    // do something with the position...
                    // for now, this something is invoking a position update event. may be changed
                    // (the location may be pushed to the frontend for example) ...

                    // invokes the current position every quarter of a second to throttle the mcasts
                    // quarter of a second = 25 ticks (10ms * 25 = 250ms)
                    if (ticks % 25 == 0)
                        NodePositionUpdate?.Invoke(this, currentLocation);
                    ticks++;

                    // get next time from converting the ms to seconds
                    // in other words, jump each 10ms
                    time += _FREQUENCY / 1000.0;

                    // sleep for 10ms
                    await Task.Delay(_FREQUENCY);
                }
                // if position successfully advanced, return true
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
