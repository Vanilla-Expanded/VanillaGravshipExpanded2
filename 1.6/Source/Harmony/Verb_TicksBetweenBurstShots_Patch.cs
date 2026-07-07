using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(Verb), "TicksBetweenBurstShots", MethodType.Getter)]
    public static class Verb_TicksBetweenBurstShots_Patch
    {
        public static void Postfix(Verb __instance, ref int __result)
        {
            if (__instance.EquipmentSource is Building building && building.Faction == Faction.OfPlayer && building.Map.IsWarcomputerPresent())
            {
                if (building.def == InternalDefOf.VGE_JavelinPod || building.def == InternalDefOf.VGE_JavelinLauncher)
                {
                    __result = 10;
                }
            }
        }
    }
}
