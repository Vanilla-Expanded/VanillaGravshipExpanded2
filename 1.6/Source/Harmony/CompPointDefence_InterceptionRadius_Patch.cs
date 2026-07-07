using HarmonyLib;
using RimWorld;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(CompPointDefence), "InterceptionRadius", MethodType.Getter)]
    public static class CompPointDefence_InterceptionRadius_Patch
    {
        public static void Postfix(ThingComp __instance, ref float __result)
        {
            if (__instance.parent != null && __instance.parent.Faction == Faction.OfPlayer && __instance.parent.Map.IsWarcomputerPresent())
            {
                __result = 19.9f;
            }
        }
    }
}
