using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HotSwappable]
    public class Designator_GenerateEmptyOrbit : Designator
    {
        public Designator_GenerateEmptyOrbit()
        {
            defaultLabel = "VGE_GenerateEmptyOrbit".Translate();
            defaultDesc = "VGE_GenerateEmptyOrbitDesc".Translate();
            icon = ContentFinder<Texture2D>.Get("UI/Gizmo/Gizmo_DevGenerateOrbit");
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 loc) => false;
        public override void ProcessInput(Event ev)
        {
            var currentTileId = Find.CurrentMap.Tile;
            var orbitLayer = Find.WorldGrid.Orbit;
            var orbitTile = currentTileId.LayerDef != PlanetLayerDefOf.Orbit
                ? orbitLayer.GetClosestTile_NewTemp(currentTileId)
                : currentTileId;
            LongEventHandler.QueueLongEvent(delegate
            {
                var mapParent = WorldObjectMaker.MakeWorldObject(InternalDefOf.VGE_EmptySpaceObj) as MapParent;
                mapParent.Tile = orbitTile;
                mapParent.SetFaction(Faction.OfPlayer);
                Find.WorldObjects.Add(mapParent);
                var map = MapGenerator.GenerateMap(new IntVec3(250, 1, 250), mapParent, InternalDefOf.VGE_EmptySpace);
                CameraJumper.TryJump(map.Center, map);
            }, "GeneratingMap", true, null);
        }
    }
}
