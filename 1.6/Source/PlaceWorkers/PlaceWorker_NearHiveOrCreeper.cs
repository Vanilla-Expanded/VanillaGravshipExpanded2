using RimWorld;
using System.Threading;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace VanillaGravshipExpanded2
{
    public class PlaceWorker_NearHiveOrCreeper : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            foreach (Thing structure in GenRadial.RadialDistinctThingsAround(loc, map, 5.9f, true))
            {
               
                if (structure.def == InternalDefOf.VFEI2_VGE_ArtificialExoHive)
                { return true; }

            }
            foreach (Thing structure in GenRadial.RadialDistinctThingsAround(loc, map, 11.9f, true))
            {
                Building subCreeper = structure as Building;
                if (subCreeper != null && subCreeper.def == InternalDefOf.VFEI2_VGE_Subcreeper)
                { return true; }

            }
            return new AcceptanceReport("VGE_NeedsHiveOrSubcreeper".Translate());
        }

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            foreach (Thing structure in GenRadial.RadialDistinctThingsAround(center, Find.CurrentMap, 12.9f, true))
            {               
               
                    if(structure?.def== InternalDefOf.VFEI2_VGE_ArtificialExoHive)
                    {
                        GenDraw.DrawRadiusRing(structure.Position, 5.9f);
                    }
                    if (structure?.def == InternalDefOf.VFEI2_VGE_Subcreeper)
                    {
                        GenDraw.DrawRadiusRing(structure.Position, 12.9f);
                    }
          

            }
            
        }
    }
}
