using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(SectionLayer_GravshipHull), nameof(SectionLayer_GravshipHull.ShouldDrawCornerPiece))]
    public static class SectionLayer_GravshipHull_ShouldDrawCornerPiece_Patch
    {
        public static void Postfix(IntVec3 pos, Map map, TerrainGrid terrGrid, ref bool __result, ref SectionLayer_GravshipHull.CornerType cornerType, ref Color color)
        {
            if (__result)
            {
                return;
            }
            
            if (!SectionLayer_SubstructureProps_Regenerate_Patch.IsRegenerating)
            {
                return;
            }

            if (SectionLayer_GravshipArmorHull.ShouldDrawCornerPiece(pos, map, terrGrid, out var myCornerType, out var myColor))
            {
                __result = true;
                cornerType = (SectionLayer_GravshipHull.CornerType)(int)myCornerType;
                color = myColor;
            }
        }
    }
}