using TacMesh.Core.graph_related;
using TacMesh.Core.interfaces.raster;

namespace TacMesh.Core.map_related
{
    public class TerrainLineOfSight : ILineOfSight
    {
        // actual terrain elevation model used to test the line of sight on
        //private ... _elevationModel;

        // inject elevation model
        public TerrainLineOfSight(/* ... elvationModel */)
        {
            //_elevationModel = elvationModel;
        }

        /// <summary>
        /// Test line of sight between two points on the elevation model
        /// </summary>
        /// <param name="point1">Point on the DEM</param>
        /// <param name="point2">Point on the DEM</param>
        /// <returns>True if there is a line of sight between the two points, False otherwise.</returns>
        public bool TestLineOfSight(Location point1, Location point2)
        {
            // ...

            // TEMPORARILY ALLWAYS TRUE untill the elevation map plugs in Sprint 7.
            return true;
        }
    }
}
