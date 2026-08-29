
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class PlaceWorker_ShowBiggerRadius : PlaceWorker
    {
        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
           
            GenDraw.DrawRadiusRing(center, 12.9f);

        }
    }
}
