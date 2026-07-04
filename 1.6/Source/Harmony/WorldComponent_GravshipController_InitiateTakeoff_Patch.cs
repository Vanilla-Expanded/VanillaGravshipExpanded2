using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(WorldComponent_GravshipController), "InitiateTakeoff")]
    public static class WorldComponent_GravshipController_InitiateTakeoff_Patch
    {
        public static void Postfix(Building_GravEngine engine, PlanetTile targetTile)
        {
            var distance = GravshipHelper.GetDistance(engine.Map.Tile, targetTile);
            var size = engine.ValidSubstructure.Count;
            WorldComponent_GravshipCombat.Instance.AddVisibility(size * distance);
        }
    }
}
