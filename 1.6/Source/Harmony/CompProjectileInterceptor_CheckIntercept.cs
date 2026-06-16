using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2;

[HarmonyPatch(typeof(CompProjectileInterceptor), nameof(CompProjectileInterceptor.CheckIntercept))]
public static class CompProjectileInterceptor_CheckIntercept
{
    private static void Prefix(int ___currentHitPoints, out int __state)
    {
        __state = ___currentHitPoints;
    }

    private static void Postfix(CompProjectileInterceptor __instance, int __state)
    {
        if (__instance is not CompGravshipShieldGeneratorWithHeat gravshipGenerator)
            return;

        if (__state > gravshipGenerator.currentHitPoints)
            gravshipGenerator.TryPushHeat((__state - gravshipGenerator.currentHitPoints) * gravshipGenerator.Props.HeatPerDamage);
    }
}