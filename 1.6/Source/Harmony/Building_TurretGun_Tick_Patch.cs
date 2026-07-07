using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(Building_TurretGun), "Tick")]
    public static class Building_TurretGun_Tick_Patch
    {
        public static void Postfix(Building_TurretGun __instance)
        {
            if (__instance.IsHashIntervalTick(250) && __instance.Faction == Faction.OfPlayer)
            {
                __instance.ApplyForcedMissRadiusBuffs();
            }
        }
    }
}
