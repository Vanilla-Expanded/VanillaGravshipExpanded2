using RimWorld;
using Verse;
namespace VanillaGravshipExpanded2
{
    public class PlaceWorker_NearHiveOrCreeper : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            foreach (Thing structure in GenRadial.RadialDistinctThingsAround(loc, map, 5.9f, true))
            {
                Building hiveBuilding = structure as Building;
                if (hiveBuilding != null && hiveBuilding.def == InternalDefOf.VFEI2_VGE_ArtificialExoHive)
                { return true; }

            }
            foreach (Thing structure in GenRadial.RadialDistinctThingsAround(loc, map, 12.9f, true))
            {
                Building subCreeper = structure as Building;
                if (subCreeper != null && subCreeper.def == InternalDefOf.VFEI2_VGE_Subcreeper)
                { return true; }

            }
            return new AcceptanceReport("VGE_NeedsHiveOrSubcreeper".Translate());
        }
    }
}
