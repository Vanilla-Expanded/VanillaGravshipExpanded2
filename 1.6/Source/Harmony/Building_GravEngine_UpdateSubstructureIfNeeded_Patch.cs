using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(Building_GravEngine), "UpdateSubstructureIfNeeded")]
    public static class Building_GravEngine_UpdateSubstructureIfNeeded_Patch
    {
        public static void Prefix(Building_GravEngine __instance, bool regenerateSectionLayers)
        {
            if (!ModsConfig.OdysseyActive || __instance.Destroyed || !__instance.Spawned || !__instance.substructureDirty)
            {
                return;
            }
            if (regenerateSectionLayers && __instance.Map != null && __instance.Map.mapDrawer != null)
            {
                __instance.Map.mapDrawer.RegenerateLayerNow(typeof(SectionLayer_GravshipArmorHull));
            }
        }
    }
}