using HarmonyLib;
using RimWorld;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(CompScannerCluster), "TickDoesFind")]
    public static class CompScannerCluster_TickDoesFind_Patch
    {
        public static void Postfix(CompOrbitalScanner __instance)
        {
            if (__instance.parent.IsHashIntervalTick(250))
            {
                WorldComponent_GravshipVisibility.Instance.AddVisibility((320f / 2500f) * 250f);
            }
        }
    }
}
