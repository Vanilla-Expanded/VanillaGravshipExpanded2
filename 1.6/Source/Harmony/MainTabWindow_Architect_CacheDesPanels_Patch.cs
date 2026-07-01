using System.Linq;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(MainTabWindow_Architect), "CacheDesPanels")]
    public static class MainTabWindow_Architect_CacheDesPanels_Patch
    {
        public static void Prefix()
        {
            var allDefs = DefDatabase<DesignationCategoryDef>.AllDefs;
            if (!DebugSettings.godMode)
            {
                if (allDefs.Contains(InternalDefOf.VGE_Designer))
                {
                    DefDatabase<DesignationCategoryDef>.Remove(InternalDefOf.VGE_Designer);
                }
            }
            else if (allDefs.Contains(InternalDefOf.VGE_Designer) is false)
            {
                DefDatabase<DesignationCategoryDef>.Add(InternalDefOf.VGE_Designer);
            }
        }
    }
}
