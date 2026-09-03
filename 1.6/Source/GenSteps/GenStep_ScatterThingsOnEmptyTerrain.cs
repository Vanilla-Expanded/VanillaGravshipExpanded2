using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class GenStep_ScatterThingsOnEmptyTerrain : GenStep_ScatterThings
    {
        public List<CellRect> disallowedRects;

        public override bool CanScatterAt(IntVec3 loc, Map map)
        {
            if (base.CanScatterAt(loc, map) is false || loc.Standable(map) is false || loc.GetTerrain(map) == TerrainDefOf.Space)
            {
                return false;
            }
            foreach (var rect in disallowedRects)
            {
                if (rect.Contains(loc))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
