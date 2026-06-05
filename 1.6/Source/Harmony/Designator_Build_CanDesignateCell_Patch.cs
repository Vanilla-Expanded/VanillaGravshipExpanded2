using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(Designator_Build), nameof(Designator_Build.CanDesignateCell))]
    public static class Designator_Build_CanDesignateCell_Patch
    {
        static void Postfix(Designator_Build __instance, IntVec3 c, ref AcceptanceReport __result)
        {
            if (__instance.PlacingDef == TerrainDefOf.Space && DebugSettings.godMode)
            {
                __result = c.InBounds(__instance.Map) && !c.Fogged(__instance.Map);
            }
        }
    }
}
