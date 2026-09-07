using HarmonyLib;
using RimWorld;
using Verse;
using VanillaGravshipExpanded;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(ThingUtility), nameof(ThingUtility.CheckAutoRebuildOnDestroyed))]
    public static class VanillaGravshipExpanded2_ThingUtility_CheckAutoRebuildOnDestroyed_Patch
    {
        public static void Postfix(Thing thing, ref Blueprint_Build __result)
        {
            if (thing?.def?.designationCategory == InternalDefOf.VGE_Designer)
            {               
                    __result = null;                
            }
        }
    }
}
