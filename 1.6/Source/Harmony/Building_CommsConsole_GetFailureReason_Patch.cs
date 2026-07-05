using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{

    [HarmonyPatch(typeof(Building_CommsConsole), "GetFailureReason")]
    public static class Building_CommsConsole_GetFailureReason_Patch
    {
        public static void Postfix(Building_CommsConsole __instance, ref FloatMenuOption __result)
        {
            if (__result == null && __instance.Map.IsAncientSignalJammerOnMap())
            {
                __result = new FloatMenuOption("VGE_AncientSignalJammerCommsBlocked".Translate(), null);
            }
        }
    }
}
