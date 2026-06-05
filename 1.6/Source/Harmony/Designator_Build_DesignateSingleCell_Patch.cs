using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(Designator_Build), nameof(Designator_Build.DesignateSingleCell))]
    public static class Designator_Build_DesignateSingleCell_Patch
    {
        static void Prefix(Designator_Build __instance, IntVec3 c)
        {
            if (__instance.PlacingDef == TerrainDefOf.Space && DebugSettings.godMode)
            {
                if (__instance.Map.terrainGrid.CanRemoveTopLayerAt(c)) __instance.Map.terrainGrid.RemoveTopLayer(c, false);
                if (__instance.Map.terrainGrid.CanRemoveFoundationAt(c)) __instance.Map.terrainGrid.RemoveFoundation(c, false);
            }
        }
    }
}
