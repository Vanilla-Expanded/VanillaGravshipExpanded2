using HarmonyLib;
using RimWorld;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(SectionLayer_SubstructureProps), nameof(SectionLayer_SubstructureProps.Regenerate))]
    public static class SectionLayer_SubstructureProps_Regenerate_Patch
    {
        public static bool IsRegenerating = false;

        public static void Prefix()
        {
            IsRegenerating = true;
        }

        public static void Finalizer()
        {
            IsRegenerating = false;
        }
    }
}
