using System.Text.Json.Serialization;
using TacMesh.Core.interfaces;

namespace TacMesh.Core.graph_related
{
    public class Location : ILocation
    {
        // TEMPORARILY very simple location point to test out VirtualRadioModel
        // this class will be able to change to something more complicated in the future to
        // fit the actual graphs that are going to be used...

        [JsonInclude]
        public double X { get; private set; }
        [JsonInclude]
        public double Y { get; private set; }

        public Location(double x, double y)
        {
            X = x;
            Y = y;
        }

        public void UpdateLocation(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Distance calculator
        /// </summary>
        /// <param name="other"></param>
        /// <returns>The Distance between the point and the other point</returns>
        public double Distance(Location other)
        {
            double xDiff = other.X - this.X;
            double yDiff = other.Y - this.Y;

            double xDiffSquared = Math.Pow(xDiff, 2);
            double yDiffSquared = Math.Pow(yDiff, 2);

            // d = sqrt{ (x2 - x1)^2 + (y2 - y1)^2 }
            return Math.Sqrt(xDiffSquared + yDiffSquared);
        }

        public override string ToString() => $"({X:F3},{Y:F3})";

        /// <summary>
        /// Copies the values from the current Location object to the other Location object.
        /// </summary>
        /// <param name="other"></param>
        public void CopyTo(Location other)
        {
            other.X = this.X;
            other.Y = this.Y;
        }

        /// <summary>
        /// determines if the current Location obbject is equal to the other Location object.
        /// </summary>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            if (obj != null && obj is Location other) {
                return this.X == other.X && this.Y == other.Y;
            }
            return false;
        }
    }
}
