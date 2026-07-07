using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(Building_TurretGun), "SpawnSetup")]
    public static class Building_TurretGun_SpawnSetup_Patch
    {
        public static void Postfix(Building_TurretGun __instance)
        {
            if (__instance.Faction == Faction.OfPlayer)
            {
                __instance.ApplyForcedMissRadiusBuffs();
            }
        }
    }
}
