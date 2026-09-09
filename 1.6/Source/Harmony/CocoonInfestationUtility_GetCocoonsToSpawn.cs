using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(CocoonInfestationUtility), nameof(CocoonInfestationUtility.GetCocoonsToSpawn))]
    public static class VanillaGravshipExpanded2_CocoonInfestationUtility_GetCocoonsToSpawn_Patch
    {
        public static IEnumerable<ThingDef> Postfix(IEnumerable<ThingDef> __result)
        {
            foreach (ThingDef g in __result)
            {
                if (g == InternalDefOf.VGE_ExowormCocoon)
                {
                    yield return InternalDefOf.CocoonMegascarab;
                }
                else { yield return g; }
                
            }
        }
    }
}