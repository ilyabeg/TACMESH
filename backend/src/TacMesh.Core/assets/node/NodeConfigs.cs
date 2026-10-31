using System.Text;
using System.Text.Json.Serialization;
using TacMesh.Core.graph_related;

namespace TacMesh.Core.assets.node
{
    /// <summary>
    /// global public configurations class that defines the node's settings.
    /// provides an easy way to update at runtime.
    /// </summary>
    public class NodeConfigs
    {
        // Important Note: if any class holds references to the NodeConfigs fields themselves,
        // and the refernce types (such as the Permitted Neighbors list) are overwritten by 'new',
        // those classes may hold expired data types and unexpected behavior may accour. 


        // --- node settings ---
        [JsonInclude]
        public double Speed { get; private set; }
        [JsonInclude]
        public Location Location { get; private set; }
        [JsonInclude]
        public double LinkRange { get; private set; }
        [JsonInclude]
        public List<string> PermittedNeighbors { get; private set; }
        [JsonInclude]
        public Queue<Location> Waypoints { get; private set; }

        // --- global constants ---
        public const int DEFAULT_LOCATION_X = 0;
        public const int DEFAULT_LOCATION_Y = 0;
        public const double MIN_SPEED = 1;
        public const double MAX_SPEED = 6;
        public const double MAX_LINK_RANGE = 400;
        public const double MIN_LINK_RANGE = 150;


        // constructors
        public NodeConfigs() { } // empty constructor for json serialization/deserialization

        public NodeConfigs(double speed, Location location, double linkRange, List<string> permitted, Queue<Location> waypoints)
        {
            Speed = speed;
            Location = location;
            LinkRange = linkRange;
            PermittedNeighbors = permitted;
            Waypoints = waypoints;
        }

        // template to get default configs
        public static NodeConfigs GetDefaultEngineConfigs(List<string>? permittedList = null) // non-mandatory list of permitted nodes
        {
            // default ref types
            Location location = new Location(DEFAULT_LOCATION_X, DEFAULT_LOCATION_Y);
            List<string> permittedNodes = permittedList ?? new List<string>();
            Queue<Location> waypoints = new Queue<Location>();

            return new NodeConfigs(MIN_SPEED, location, MAX_LINK_RANGE, permittedNodes, waypoints);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("Node Configuration Settings:");
            stringBuilder.AppendLine($"\tNode Speed: {Speed}");
            stringBuilder.AppendLine($"\tNode Location: {Location}");
            stringBuilder.AppendLine($"\tNode Link Range: {LinkRange}");
            stringBuilder.AppendLine($"\tPermitted Neighbors: {PermittedNeighbors?.ToString()}");
            stringBuilder.AppendLine($"\tWaypoints: {Waypoints?.ToString()}");

            return stringBuilder.ToString();
        }

        // lock to update the settings safely
        private readonly object _updateLock = new object();

        /// <summary>
        /// updates the engine process node's configuration settings to the new provided ones
        /// </summary>
        /// <param name="speed"></param>
        /// <param name="location"></param>
        /// <param name="linkRange"></param>
        /// <param name="permitted"></param>
        /// <param name="waypoints"></param>
        public void UpdateConfigurations(
            double? speed = null, 
            Location? location = null,
            double? linkRange = null,
            List<string>? permitted = null,
            Queue<Location>? waypoints = null
        ) {
            // locak before updating to handle race conditions
            lock (_updateLock)
            {
                // update the specified paramters ONLY if they are not null,
                // otherwise keep the current value
                //
                Speed = speed ?? Speed;
                LinkRange = linkRange ?? LinkRange;

                // reference types cannot change because other classes hold
                // references to them, they need to copy the actual values 
                // of the new fields to their's
                // 
                location?.CopyTo(Location);

                if (permitted != null)                
                    CopyValues(permitted);

                if (waypoints != null)
                    CopyValues(waypoints); 
            }
        }
        public void UpdateConfigurations(NodeConfigs otherConfigs)
        {
            // locak before updating to handle race conditions
            lock (_updateLock)
            {
                // update the specified paramters ONLY if they are not null,
                // otherwise keep the current value
                //
                Speed = otherConfigs.Speed;
                LinkRange = otherConfigs.LinkRange;

                // reference types cannot change because other classes hold
                // references to them, they need to copy the actual values 
                // of the new fields to their's
                // 
                otherConfigs.Location?.CopyTo(Location);                       

                if (otherConfigs.PermittedNeighbors != null)
                    CopyValues(otherConfigs.PermittedNeighbors);

                if (otherConfigs.Waypoints != null)
                    CopyValues(otherConfigs.Waypoints);
            }
        }

        // copies the values of the provided field
        //
        // --- overloads ---
        private void CopyValues(List<string> permitted)
        {
            // clear the current list
            PermittedNeighbors.Clear();

            // add the values from the other list to this list ref
            PermittedNeighbors.AddRange(permitted);
        }
        private void CopyValues(Queue<Location> waypoints)
        {
            // clear original waypoints
            Waypoints.Clear();

            // add the new waypoints 
            foreach (Location newWaypoint in waypoints)
                Waypoints.Enqueue(newWaypoint);
        }
    }
}
