using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded2
{
    public static class DistanceUtil
    {
        public static float GetDistanceInOrbitTiles(PlanetTile from, PlanetTile to)
        {
            var fromOrbit = from.GetOrbitEqualentTile();
            var toOrbit = to.GetOrbitEqualentTile();
            if (fromOrbit.Valid && toOrbit.Valid)
            {
                return Find.WorldGrid.TraversalDistanceBetween(fromOrbit, toOrbit) * fromOrbit.LayerDef.rangeDistanceFactor;
            }
            return int.MaxValue;
        }

        public static PlanetTile GetOrbitEqualentTile(this PlanetTile tile)
        {
            if (tile.LayerDef == PlanetLayerDefOf.Orbit)
            {
                return tile;
            }
            if (!Find.WorldGrid.TryGetFirstAdjacentLayerOfDef(tile, PlanetLayerDefOf.Orbit, out var orbitLayer))
            {
                return PlanetTile.Invalid;
            }
            return orbitLayer.GetClosestTile_NewTemp(tile);
        }
    }
}
