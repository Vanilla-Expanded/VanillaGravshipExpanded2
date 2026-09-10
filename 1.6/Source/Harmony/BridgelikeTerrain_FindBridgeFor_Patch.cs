using System.Reflection;
using HarmonyLib;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch]
    public static class BridgelikeTerrain_FindBridgeFor_Patch
    {
        public static bool Prepare()
        {
            return ModsConfig.IsActive("Memegoddess.ReplaceStuff");
        }

        public static MethodBase TargetMethod()
        {
            return AccessTools.Method("Replace_Stuff.PlaceBridges.BridgelikeTerrain:FindBridgeFor");
        }

        public static void Postfix(ref TerrainDef __result)
        {
            if (__result == null)
            {
                return;
            }
            if (__result.IsSubstructure || __result.designationCategory == InternalDefOf.VGE_Designer)
            {
                __result = null;
            }
        }
    }
}
