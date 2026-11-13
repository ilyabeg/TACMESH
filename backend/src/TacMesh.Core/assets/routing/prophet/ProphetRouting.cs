using System.Collections.Concurrent;
using TacMesh.Core.interfaces.routing;
using TacMesh.Core.packet;
using TacMesh.Core.tables.node;

namespace TacMesh.Core.assets.routing.prophet
{
    public class ProphetRouting : IRouting
    {
        // delivery probability for every known neighbor of the node.
        // determines the probability of the packet getting delivered to the node 
        //
        private readonly ConcurrentDictionary<string, double> _deliveryProbability;

        // dependencies
        private readonly NeighborTable _neighborTable;

        // --- PRoPHET constants ---

        /// <summary>
        /// the initial probability constant
        /// a scaling factor setting
        /// the rate at which the predictability increases upon a direct
        /// physical encounter between two nodes
        /// 
        /// the reason for 0.75 is so the initial predictability of two nodes
        /// won't be 1 (meaning 100% delivery), but high enough to make the 
        /// remote node a candidate for a next hop
        /// </summary>
        private const double EncounterProbability = .75;

        /// <summary>
        /// delta is a small positive number that effectively sets an upper 
        /// bound for P_(A, B). the limit is set so that predictabilities 
        /// between different nodes stay strictly less than 1. a predictability
        /// of 1 is strictly reserved for a delivery from a node to itself
        /// </summary>
        private const double Delta = 0.01;

        /// <summary>
        /// the aging factor constant (or Gamma as specified in the protocol)
        /// a decay multiplier applied every time cycle to the predictability of 
        /// unheard nodes. it ensures that older routing information loses it's
        /// relevance over time
        /// 
        /// the reason for the factor being 0.98 is to make sure the predictability
        /// won't drop down too fast (if it were a lower value), while making the factor 
        /// effect the value so it loses it's relevance over time
        /// </summary>
        private const double AgingFactor = .98;

        /// <summary>
        /// the "transitivity" constant (or Beta as specified in the protocol)
        ///
        /// Transitivity principle: if node A encounters node B frequently, and node B
        /// encounters node C frequently, than node A can determine that node B is a
        /// trusted "transmitter" node to node C
        ///
        /// the constant is a scaling factor which determines what percentage of the
        /// indirect delivery potential (A->B * B->C) is added to the overall
        /// probability of delivery to the target
        /// 
        /// the reason for the factor being 0.25 is to prioritize direct physical
        /// encounters over indirect recommendations. if this value was too high, 
        /// the system would mistakenly think that a long chain of nodes is a reliable 
        /// path, and will cause the network to send too many messages through weak, 
        /// long paths, which wastes network speed (bandwidth) and fills up node 
        /// memory buffers with stuck messages
        /// </summary>
        private const double TransitivityFactor = .25;


        // constructor
        public ProphetRouting(NeighborTable neighborTable)
        {
            _deliveryProbability = new ConcurrentDictionary<string, double>();
            _neighborTable = neighborTable;
        }

        /// <summary>
        /// Router method to determine the next hops for the DataPacket
        /// </summary>
        /// <param name="packet">The packet to route through to the next hops</param>
        /// <returns>A list containing the identifiers of the next hops</returns>
        public List<string> Route(DataPacket packet)
        {
            List<string> nextHops = new List<string>();

            // pull node C id and A -> C delivery probability
            //
            string nodeC = packet.PacketHeader.DstID;
            double p_AC = _deliveryProbability.TryGetValue(nodeC, out double val) ? val : 0;

            // iterate through all current node A's neighbors
            //
            foreach (string nodeB in _neighborTable.GetCurrentNeighbors())
            {
                // if node B is node C, add to next hop immedietly and continue
                //
                if (nodeB.Equals(nodeC))
                {
                    nextHops.Add(nodeB);
                    continue;
                }

                double p_BC = ... ; // B -> C probability

                // Grater Than Rule: clacify node B as a next hop only if the
                // delivery probability of node B to C is grater than the delivery
                // probability of node A to C
                //
                if (p_BC > p_AC)
                    nextHops.Add(nodeB);
            }
            return nextHops;
        }

        // --- Updating methods ---

        /// <summary>
        /// Updator method to update a direct neighbor node's delivery probability.
        /// </summary>
        /// <param name="nodeId">The neighboring node identifier</param>
        private void UpdateProbability(string nodeId)
        {
            // an initial value in case the node does not exist in the dict
            //
            double initialValue = (1 - Delta) * EncounterProbability;

            // add the initial value to the node id, or update using the
            // PRoPHET probability updater equation
            //
            // direct neighbor encounter probability update per the protocol:
            // P_(A,B) = P_(A, B)_old + (1 - delta - P_(A, B)_old ) * P_encounter (Eq. 1)            
            //
            _deliveryProbability.AddOrUpdate(nodeId, initialValue, 
                (key, oldProbability) => oldProbability + (1 - Delta - oldProbability) * EncounterProbability);
        }

        /// <summary>
        /// Updator method to age a direct neighbor node's delivery probability
        /// when the neighbor was unheared from, for K time cycles.
        /// </summary>
        /// <param name="nodeId">The neighboring node identifier</param>
        /// <param name="k">Integer representing the amount of cycles the nighbor was unheared from</param>
        private void DecayProbability(string nodeId, int k)
        {
            double defaultValue = 0; // if no old probability exists

            // add the default value to the node id, or update using the
            // PRoPHET probability aging equation
            //
            // neighbor aging probability update per the protocol:
            // P_(A,B) = P_(A,B)_old * gamma^K  (Eq. 2)
            //
            _deliveryProbability.AddOrUpdate(nodeId, defaultValue,
                (key, oldProbability) => oldProbability * Math.Pow(AgingFactor, k));
        }

        /// <summary>
        /// Updates the probability of delivery for each node C in node B's probability list
        /// via the PRoPHET transitivity probability equation
        /// </summary>
        /// <param name="remoteNodeProbabilities">The remote node B's list of probabilities</param>
        /// <param name="p_AB">The delivery probability from node A to the remote node B</param>
        private void TransitivityUpdate(ConcurrentDictionary<string, double> remoteNodeProbabilities, double p_AB)
        {
            // iterate through all node B's delivery probabilities
            //
            foreach (string node_C_id in remoteNodeProbabilities.Keys)
            {
                double p_BC = remoteNodeProbabilities[node_C_id]; // B -> C probability

                // default value in case no old A -> C probability exists
                //
                double defaultValue = p_AB * p_BC * TransitivityFactor;

                // add the default value to the node C probability, or update using the
                // PRoPHET transitivity probability equation
                //
                // neighbor transitive probability update per the protocol:
                // P_(A,C) = MAX( P_(A,C)_old, P_(A,B) * P_(B,C)_recv * beta )  (Eq. 3)
                //
                _deliveryProbability.AddOrUpdate(node_C_id, defaultValue, 
                    (key, p_AC_old) => Math.Max(p_AC_old, p_AB * p_BC * TransitivityFactor));
            }            
        }
    }
}
