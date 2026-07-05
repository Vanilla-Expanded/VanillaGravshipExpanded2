using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(CompShuttle), nameof(CompShuttle.CanLaunch), MethodType.Getter)]
    public static class CompShuttle_CanLaunch_Patch
    {
        public static void Postfix(CompShuttle __instance, ref AcceptanceReport __result)
        {
            if (__result.Accepted && __instance.parent.Map.IsAncientSignalJammerOnMap())
            {
                __result = new AcceptanceReport("VGE_AncientSignalJammerShuttleBlocked".Translate());
            }
        }
    }
}
