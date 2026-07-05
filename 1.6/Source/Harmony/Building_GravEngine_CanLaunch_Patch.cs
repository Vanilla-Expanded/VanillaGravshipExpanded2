using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(Building_GravEngine), nameof(Building_GravEngine.CanLaunch))]
    public static class Building_GravEngine_CanLaunch_Patch
    {
        public static void Postfix(Building_GravEngine __instance, ref AcceptanceReport __result)
        {
            if (__result.Accepted && __instance.Map.IsAncientSignalJammerOnMap())
            {
                __result = new AcceptanceReport("VGE_AncientSignalJammerLaunchBlocked".Translate());
            }
        }

        public static bool IsAncientSignalJammerOnMap(this Map map)
        {
            return map.listerThings.ThingsOfDef(InternalDefOf.VGE_AncientSignalJammer).Count > 0;
        }
    }
}
