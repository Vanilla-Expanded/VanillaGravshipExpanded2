using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(CompLaunchable), nameof(CompLaunchable.CanLaunch))]
    public static class CompLaunchable_CanLaunch_Patch
    {
        public static void Postfix(CompLaunchable __instance, ref AcceptanceReport __result)
        {
            if (__result.Accepted && __instance.parent.Map.IsAncientSignalJammerOnMap())
            {
                __result = new AcceptanceReport("VGE_AncientSignalJammerBlockedGeneric".Translate(__instance.parent.Label));
            }
        }
    }
}
