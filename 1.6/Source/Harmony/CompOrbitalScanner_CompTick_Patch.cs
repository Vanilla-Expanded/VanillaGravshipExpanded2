using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(CompOrbitalScanner), "CompTick")]
    public static class CompOrbitalScanner_CompTick_Patch
    {
        public static void Postfix(CompOrbitalScanner __instance)
        {
            if (__instance.parent.GetComp<CompPowerTrader>() is { } compPower && compPower.PowerOn && __instance.parent.IsHashIntervalTick(250))
            {
                WorldComponent_GravshipVisibility.Instance.AddVisibility((320f / 2500f) * 250f);
            }
        }
    }
}
