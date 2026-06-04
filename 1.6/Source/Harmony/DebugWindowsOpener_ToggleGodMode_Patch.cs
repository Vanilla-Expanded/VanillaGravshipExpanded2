using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(DebugWindowsOpener), nameof(DebugWindowsOpener.ToggleGodMode))]
    public static class DebugWindowsOpener_ToggleGodMode_Patch
    {
        public static void Postfix()
        {
            ((MainTabWindow_Architect)MainButtonDefOf.Architect.TabWindow).CacheDesPanels();
        }
    }
}
