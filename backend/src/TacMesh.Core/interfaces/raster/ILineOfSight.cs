using TacMesh.Core.graph_related;

namespace TacMesh.Core.interfaces.raster
{
    /// <summary>
    /// Line of Sight Interface to provide if there is a line of sight between two
    /// points on a real Elavation map of the Scenario.
    /// </summary>
    public interface ILineOfSight
    {
        bool TestLineOfSight(Location point1, Location point2);
    }
}
