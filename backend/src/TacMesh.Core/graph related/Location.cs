namespace TacMesh.Core.graph_related
{
    public class Location
    {
        // TEMPORARILY very simple location point to test out VirtualRadioModel
        // this class will be able to change to something more complicated in the future to
        // fit the actual graphs that are going to be used...

        public int X { get; private set; }
        public int Y { get; private set; }

        public Location(int x, int y)
        {
            X = x;
            Y = y;
        }

        public void UpdateCoordinates(int x, int y)
        {
            X = x;
            Y = y;
        }
        
        /// <summary>
        /// Distance calculator
        /// </summary>
        /// <param name="other"></param>
        /// <returns>The Distance between the point and GraphPoint other</returns>
        public double Distance(Location other)
        {
            int xDiff = other.X - this.X;
            int yDiff = other.Y - this.Y;

            double xDiffSquared = Math.Pow(xDiff, 2);
            double yDiffSquared = Math.Pow(yDiff, 2);

            // d = sqrt{ (x2 - x1)^2 + (y2 - y1)^2 }
            return Math.Sqrt(xDiffSquared + yDiffSquared);
        }
    }
}
