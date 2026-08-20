using HarmonyLib;
using RimWorld;
using Verse;
using VanillaGravshipExpanded;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(SectionLayer_GravshipHull), nameof(SectionLayer_GravshipHull.ShouldDrawCornerPiece))]
    public static class SectionLayer_GravshipHull_ShouldDrawCornerPiece_Patch
    {
        public static void Postfix(IntVec3 pos, Map map, TerrainGrid terrGrid, ref bool __result)
        {
            if (__result)
            {
                var terrainDef = terrGrid.FoundationAt(pos) ?? terrGrid.TerrainAt(pos);
                if (terrainDef != null && terrainDef.GetModExtension<SubstructureEdgeGraphicsExtension>()?.renderAsSubstructure == true)
                {
                    __result = false;
                }
            }
        }
    }
}
