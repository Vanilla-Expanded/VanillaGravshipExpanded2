using HarmonyLib;
using RimWorld;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(Building_TurretGun), nameof(Building_TurretGun.TryStartShootSomething))]
    public static class VanillaGravshipExpanded2_Building_TurretGun_TryStartShootSomething_Patch
    {
        public static void Postfix(Building_TurretGun __instance, LocalTargetInfo ___currentTargetInt)
        {
            if (__instance?.def == InternalDefOf.VGE_GiantWormspitter)
            {
                if (___currentTargetInt.IsValid)
                {
                    InternalDefOf.VEG_InsectoidTurretTargetAcquired.PlayOneShot(new TargetInfo(__instance.Position, __instance.Map));
                }

            }
        }
    }
}