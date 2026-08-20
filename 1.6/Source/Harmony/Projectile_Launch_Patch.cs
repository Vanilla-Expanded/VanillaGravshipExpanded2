using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HotSwappable]
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.Launch), new[] { typeof(Thing), typeof(Vector3), typeof(LocalTargetInfo), typeof(LocalTargetInfo), typeof(ProjectileHitFlags), typeof(bool), typeof(Thing), typeof(ThingDef) })]
    public static class Projectile_Launch_Patch
    {
        public static void Postfix(Projectile __instance, Thing launcher, Vector3 origin, LocalTargetInfo usedTarget, LocalTargetInfo intendedTarget)
        {
            var map = __instance.Map;
            var start = origin.ToIntVec3();
            var end = __instance.destination.ToIntVec3();
            var dir = (end - start).ToVector3();
            float dist = dir.magnitude;
            if (dist > 0.1f)
            {
                dir /= dist;
                for (float f = 0.5f; f < dist; f += 0.5f)
                {
                    var cell = (start.ToVector3() + dir * f).ToIntVec3();
                    if (!cell.InBounds(map)) continue;

                    var building = cell.GetFirstThing(map, InternalDefOf.VGE_GravshipArmor);
                    if (building is null || launcher?.Faction is null) continue;
                    if (building.Faction != null && building.Faction.HostileTo(launcher.Faction))
                    {
                        __instance.intendedTarget = new LocalTargetInfo(cell);
                        __instance.usedTarget = new LocalTargetInfo(cell);
                        __instance.destination = __instance.usedTarget.Cell.ToVector3Shifted();
                        __instance.ticksToImpact = Mathf.CeilToInt(__instance.StartingTicksToImpact);
                        if (__instance.ticksToImpact < 1)
                        {
                            __instance.ticksToImpact = 1;
                        }
                        __instance.lifetime = __instance.ticksToImpact;
                        break;
                    }
                }
            }
        }
    }
}
