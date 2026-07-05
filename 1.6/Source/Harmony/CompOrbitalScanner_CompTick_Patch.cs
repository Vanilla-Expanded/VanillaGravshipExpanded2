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
            var compPower = __instance.PowerTrader;
            if ((compPower == null || compPower.PowerOn) && __instance.parent.IsHashIntervalTick(250))
            {
                WorldComponent_GravshipCombat.Instance.AddVisibility((320f / 2500f) * 250f);
            }
        }
    }
}
