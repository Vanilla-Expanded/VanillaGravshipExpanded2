using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(BuildCopyCommandUtility), nameof(BuildCopyCommandUtility.BuildCopyCommand))]
    public static class BuildCopyCommandUtility_BuildCopyCommand_Patch
    {
        public static void Postfix(BuildableDef buildable, ref Command __result)
        {
            if (buildable?.designationCategory == InternalDefOf.VGE_Designer)
            {
                __result = null;
            }
        }
    }
}
