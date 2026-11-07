using System.Buffers.Binary;
using System.Text.Json.Serialization;
using TacMesh.Core.interfaces;
using TacMesh.Core.interfaces.location;

namespace TacMesh.Core.assets.graph
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

        // public default location 
        //
        public static readonly Location DEFAULT_LOCATION = new (0, 0);

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

        public override string ToString() => $"{X:F2},{Y:F2}";

        /// <summary>
        /// Get the byte representation of the Location
        /// </summary>
        /// <returns>The bytes of the location object</returns>
        public byte[] ToBytes()
        {
            // write X bytes in big endian order
            //
            byte[] xBytes = new byte[ByteCount._8bytes];
            BinaryPrimitives.WriteDoubleBigEndian(xBytes, X);

            // write Y bytes in big endian order
            //
            byte[] yBytes = new byte[ByteCount._8bytes];
            BinaryPrimitives.WriteDoubleBigEndian(yBytes, Y);

            // return all bytes appended as one array
            //
            return [.. xBytes, .. yBytes];
        }

        /// <summary>
        /// Get the Location from the byte representation
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>The Location from the bytes</returns>
        public static Location FromBytes(byte[] bytes)
        {
            // NOTE: this method may throw an exception if the bytes aren't a valid location,
            // which is not wrong, it is supposed to throw the error.

            // get the first 8 bytes
            //
            byte[] xBytes = bytes.AsSpan().Slice(start: 0, ByteCount._8bytes).ToArray();
            BinaryPrimitives.TryReadDoubleBigEndian(xBytes, out double x);

            // get the last 8 bytes
            //
            byte[] yBytes = bytes.AsSpan().Slice(start: ByteCount._8bytes).ToArray();
            BinaryPrimitives.TryReadDoubleBigEndian(yBytes, out double y);
            
            return new Location(x, y); // return the deserialized location object
        }

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

        /// <summary>
        /// Overridden GetHashCode() which is dependent on the Location X and Y points
        /// </summary>
        /// <returns>The Location Hashcode</returns>
        public override int GetHashCode() => X.GetHashCode() + Y.GetHashCode();
    }
}
