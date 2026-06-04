using HarmonyLib;
using RimWorld;
using System.Linq;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(GenLeaving), nameof(GenLeaving.DoLeavingsFor), typeof(TerrainDef), typeof(IntVec3), typeof(Map))]
    public static class GenLeaving_DoLeavingsFor_Patch
    {
        public static bool Prefix(TerrainDef terrain, IntVec3 cell, Map map)
        {
            if (terrain == InternalDefOf.VGE_GravshipSubarmor)
            {
                var validCells = GenRadial.RadialCellsAround(cell, 1.5f, true).Where(c => c.InBounds(map) && c.GetTerrain(map) != TerrainDefOf.Space && c.GetFirstItem(map) == null).ToList();
                var gravliteCell = validCells.Any() ? validCells.First() : cell;
                var plasteelCell = validCells.Count > 1 ? validCells[1] : gravliteCell;

                var gravlite = ThingMaker.MakeThing(ThingDefOf.GravlitePanel);
                gravlite.stackCount = Rand.RangeInclusive(2, 4);
                GenPlace.TryPlaceThing(gravlite, gravliteCell, map, ThingPlaceMode.Near, null, c => c.GetTerrain(map) != TerrainDefOf.Space);

                var plasteel = ThingMaker.MakeThing(ThingDefOf.Plasteel);
                plasteel.stackCount = 1;
                GenPlace.TryPlaceThing(plasteel, plasteelCell, map, ThingPlaceMode.Near, null, c => c.GetTerrain(map) != TerrainDefOf.Space);
                return false;
            }
            return true;
        }
    }
}
