using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using RimWorld;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(SectionLayer_GravshipHull), nameof(SectionLayer_GravshipHull.BakeGravshipIndoorMesh))]
    public static class SectionLayer_GravshipHull_BakeGravshipIndoorMesh_Patch
    {
        public static void Postfix(Map map, CellRect bounds, Vector3 center, ref List<LayerSubMesh> __result)
        {
            var meshes = SectionLayer_GravshipArmorHull.BakeGravshipIndoorMesh(map, bounds, center);
            if (meshes != null && meshes.Count > 0)
            {
                __result ??= new List<LayerSubMesh>();
                __result.AddRange(meshes);
            }
        }
    }
}
