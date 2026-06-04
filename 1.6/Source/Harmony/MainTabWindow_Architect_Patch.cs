using HarmonyLib;
using RimWorld;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(MainTabWindow_Architect), "CacheDesPanels")]
    public static class MainTabWindow_Architect_Patch
    {
        public static void Prefix()
        {
            if (!DebugSettings.godMode)
            {
                InternalDefOf.VGE_Designer.modExtensions.Clear();
            }
            else if (InternalDefOf.VGE_Designer.modExtensions.Any(x => x is NestedCategoryExtension) is false)
            {
                InternalDefOf.VGE_Designer.modExtensions.Add(new NestedCategoryExtension
                {
                    iconTexPath = "UI/SubcategoryIcons/SubcategoryIcon_Platform",
                    parentCategory = InternalDefOf.Odyssey
                });
            }
        }
    }

    [HarmonyPatch(typeof(DebugWindowsOpener), nameof(DebugWindowsOpener.ToggleGodMode))]
    public static class DebugWindowsOpener_ToggleGodMode_Patch
    {
        public static void Postfix()
        {
            ((MainTabWindow_Architect)MainButtonDefOf.Architect.TabWindow).CacheDesPanels();
        }
    }
}
