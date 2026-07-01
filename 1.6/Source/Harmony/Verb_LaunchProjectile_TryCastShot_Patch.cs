using HarmonyLib;
using RimWorld;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(Verb_LaunchProjectile), "TryCastShot")]
    public static class Verb_LaunchProjectile_TryCastShot_Patch
    {
        public static void Postfix(Verb_LaunchProjectile __instance, bool __result)
        {
            if (__result && __instance.EquipmentSource is Building_GravshipTurret turret)
            {
                turret.TryAddVisibility();
            }
        }

        public static void TryAddVisibility(this Thing thing)
        {
            if (thing.Faction != Faction.OfPlayer) return;
            var ext = thing.def.GetModExtension<VisibilityGainExtension>();
            if (ext != null)
            {
                WorldComponent_GravshipVisibility.Instance.AddVisibility(ext.visibilityGainPerShot);
            }
        }
    }
}
