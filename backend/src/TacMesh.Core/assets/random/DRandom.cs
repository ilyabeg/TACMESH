using System.Security.Cryptography;
using System.Text;

namespace TacMesh.Core.assets.random
{
    // Deterministic Random - provides a deterministic random number based on the provided ID
    public class DRandom
    {
        // fixed seed to generate the same Randoms to repeat tests
        private const int FIXED_SEED = 13021995;
        private readonly Dictionary<string, Random> _agentsRandoms = new Dictionary<string, Random>();        
        private readonly object _lock = new object(); // lock to safely access a specific random


        /// <summary>
        /// returns a new random double based on the provided ID
        /// </summary>
        /// <param name="id">The provided ID to base the random on.</param>
        /// <returns></returns>
        public double DetermineRandom(string id)
        {
            lock (_lock)
            {
                if (!_agentsRandoms.ContainsKey(id))
                    CreateNewRandom(id);
                return _agentsRandoms[id].NextDouble();
            }
        }

        /// <summary>
        /// returns a new random integer between the provided range based on the provided ID
        /// </summary>
        /// <param name="id">The provided ID to base the random on.</param>
        /// <param name="lowerBound">The lower bound of the random range (including).</param>
        /// <param name="higherBound">The higher bound of the random range (excluding).</param>
        /// <returns></returns>
        public int DetermineRandom(string id, int lowerBound, int higherBound)
        {
            lock (_lock)
            {
                if (!_agentsRandoms.ContainsKey(id))
                    CreateNewRandom(id);
                return _agentsRandoms[id].Next(lowerBound, higherBound);
            }
        }

        /// <summary>
        /// returns a new random integer between 0 and the provided max value based on the provided ID
        /// </summary>
        /// <param name="id">The provided ID to base the random on.</param>
        /// <param name="maxValue">The provided maximum value of the range (excluding).</param>
        /// <returns></returns>
        public int DetermineRandom(string id, int maxValue)
        {
            lock (_lock)
            {
                if (!_agentsRandoms.ContainsKey(id))
                    CreateNewRandom(id);
                return _agentsRandoms[id].Next(maxValue);
            }
        }

        // creates a new random based on the id, if it is not existing yet
        private void CreateNewRandom(string id)
        {
            // the random depends on the seed and the specific id
            int seed = DetermineSeed(id);
            _agentsRandoms[id] = new Random(seed);
        }

        // creates the seed to generate the random instance based on the id
        private int DetermineSeed(string id)
        {
            // NOTE: id.GetHashCode() does NOT return the same hash code between execution runs

            // convert the id to bytes to hash using SHA256
            byte[] idBytes = Encoding.UTF8.GetBytes(id);
            byte[] hashedId = SHA256.HashData(idBytes);

            // convert the hashed bytes to 32 bit integer
            int randSeed = BitConverter.ToInt32(hashedId); 

            // base the Random instance seed on the fixed seed and the string ID
            return FIXED_SEED + randSeed;
        }
    }
}
