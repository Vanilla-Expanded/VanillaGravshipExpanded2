using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.CanBuildOnTerrain))]
    public static class GenConstruct_CanBuildOnTerrain_Patch
    {
        public static void Postfix(BuildableDef entDef, IntVec3 c, Map map, Rot4 rot, ThingDef stuffDef, ref bool __result)
        {
            if (__result)
            {
                return;
            }
            var terrainAffordanceNeed = entDef.GetTerrainAffordanceNeed(stuffDef);
            if (terrainAffordanceNeed != InternalDefOf.VGE_SubstructureAndOrbitalPlatform)
            {
                return;
            }
            var cellRect = GenAdj.OccupiedRect(c, rot, entDef.Size);
            cellRect.ClipInsideMap(map);
            foreach (var item in cellRect)
            {
                var underTerrain = map.terrainGrid.UnderTerrainAt(item);
                if (underTerrain == null || !underTerrain.affordances.Contains(terrainAffordanceNeed))
                {
                    return;
                }
            }
            __result = true;
        }
    }
}
