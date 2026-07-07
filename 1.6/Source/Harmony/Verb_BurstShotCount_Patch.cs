using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(Verb), "BurstShotCount", MethodType.Getter)]
    public static class Verb_BurstShotCount_Patch
    {
        public static void Postfix(Verb __instance, ref int __result)
        {
            if (__instance.EquipmentSource is Building building && building.Faction == Faction.OfPlayer && building.Map.IsWarcomputerPresent())
            {
                if (building.def == InternalDefOf.VGE_AnticraftCaster)
                {
                    __result += 10;
                }
                else if (building.def == InternalDefOf.VGE_AnticraftEmitter)
                {
                    __result += 15;
                }
            }
        }
    }
}
