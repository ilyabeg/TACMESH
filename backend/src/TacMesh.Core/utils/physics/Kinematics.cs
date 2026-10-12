using TacMesh.Core.graph_related;

namespace TacMesh.Core.utils.physics
{
    // a static kinematic equations provider class
    public static class Kinematics
    {
        // returns the total time it takes to travel the provided distance, given a constant velocity
        public static double TotalTime(double distance, double velocity) => distance / velocity;


        // calculates the location of the object between two provided points at the given time
        public static Location PosAt(double time, double totalTime, Location startPoint, Location endPoint)
        {
            // the parameters
            double x1 = startPoint.X;
            double x2 = endPoint.X;

            double y1 = startPoint.Y;
            double y2 = endPoint.Y;

            // the points at the given time
            double xT = x1 + time / totalTime * (x2 - x1);
            double yT = y1 + time / totalTime * (y2 - y1);

            // the location at time t
            return new Location(xT, yT);
        }
    }
}
