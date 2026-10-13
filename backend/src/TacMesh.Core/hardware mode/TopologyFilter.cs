namespace TacMesh.Core.hardware_mode
{
    public class TopologyFilter
    {
        // list to hold the permitted neighbors
        private readonly List<string> _permittedNeighbors;

        // constructors
        public TopologyFilter()
        {
            _permittedNeighbors = new List<string>();
        }
        public TopologyFilter(List<string> permitedNeighbors)
        {
            _permittedNeighbors = permitedNeighbors;
        }

        /// <summary>
        /// Filter out the remote ID of the sender via the permittes neighbor list of the Topology Filter.
        /// </summary>
        /// <param name="remoteId">The ID of the remote sender.</param>
        /// <returns>True if remoteId is a permitted neighbor, or False otherwise.</returns>
        public bool Filter(string remoteId) => _permittedNeighbors.Contains(remoteId);

        public void AddPermittedNeighbor(string remoteId) => _permittedNeighbors.Add(remoteId);
        public void RemovePermittedNeighbor(string remoteId) => _permittedNeighbors.Remove(remoteId);

        // print out the neighbors nicely if needed
        public void PrintPermittedNeighbor(string remoteId)
        {
            Console.WriteLine($"[TOPOLOGY FILTER] Permitted neighbors: {(_permittedNeighbors.Count == 0 ? "None." : "")}");

            int count = 0;
            foreach (string neighbor in _permittedNeighbors)
            {
                Console.WriteLine($"\t#{++count} - {neighbor}");
            }

            if (count > 0)
                Console.WriteLine($"Total: {count}.\n");
        }
    }
}
