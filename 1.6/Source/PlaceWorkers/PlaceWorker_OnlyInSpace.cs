using RimWorld;
using Verse;
namespace VanillaGravshipExpanded2
{
    public class PlaceWorker_OnlyInSpace : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            var terrain = map.terrainGrid.TerrainAt(loc);
            if (terrain != TerrainDefOf.Space)
            {
                return "VGE_MustBePlacedInSpace".Translate();
            }
            return true;
        }
    }
}
