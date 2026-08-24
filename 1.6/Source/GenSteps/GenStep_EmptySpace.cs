using Verse;
using RimWorld;

namespace VanillaGravshipExpanded2
{
    [HotSwappable]
    public class GenStep_EmptySpace : GenStep
    {
        public override int SeedPart => 196743;

        public override void Generate(Map map, GenStepParams parms)
        {
            if (!ModsConfig.OdysseyActive)
            {
                return;
            }
            map.regionAndRoomUpdater.Enabled = false;
            TerrainGrid terrainGrid = map.terrainGrid;
            foreach (var allCell in map.AllCells)
            {
                terrainGrid.SetTerrain(allCell, TerrainDefOf.Space);
            }
            MapGenerator.PlayerStartSpot = map.Center;
        }
    }
}
