using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class EnemyStructure : IExposable
    {
        private Dictionary<Thing, PositionData> things = new Dictionary<Thing, PositionData>();
        private Dictionary<IntVec3, RoofDef> roofs = new Dictionary<IntVec3, RoofDef>();
        private Dictionary<IntVec3, TerrainDef> foundations = new Dictionary<IntVec3, TerrainDef>();
        private Dictionary<IntVec3, TerrainDef> terrains = new Dictionary<IntVec3, TerrainDef>();
        private Dictionary<IntVec3, ColorDef> terrainColors = new Dictionary<IntVec3, ColorDef>();
        public ColorDef[] colorGrid;

        public Dictionary<Thing, PositionData> Things => things;
        public Dictionary<IntVec3, RoofDef> Roofs => roofs;
        public Dictionary<IntVec3, TerrainDef> Foundations => foundations;
        public Dictionary<IntVec3, TerrainDef> Terrains => terrains;
        public Dictionary<IntVec3, ColorDef> TerrainColors => terrainColors;

        public static EnemyStructure CaptureFrom(Map map, Thing engine)
        {
            var structure = new EnemyStructure();
            var origin = engine.Position;
            var gravjumperCells = new HashSet<IntVec3>();
            var queue = new Queue<IntVec3>();
            queue.Enqueue(origin);
            gravjumperCells.Add(origin);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                for (var i = 0; i < 8; i++)
                {
                    var neighbor = current + GenAdj.AdjacentCells[i];
                    if (neighbor.InBounds(map) && !gravjumperCells.Contains(neighbor) && IsGravshipCell(map, neighbor))
                    {
                        gravjumperCells.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            var capturedThings = new List<Thing>();

            foreach (var c in gravjumperCells)
            {
                var relPos = c - origin;

                var terrain = map.terrainGrid.TerrainAt(c);
                if (terrain != TerrainDefOf.Space)
                    structure.terrains[relPos] = terrain;

                var foundation = map.terrainGrid.FoundationAt(c);
                if (foundation != null)
                    structure.foundations[relPos] = foundation;

                var roof = map.roofGrid.RoofAt(c);
                if (roof != null)
                    structure.roofs[relPos] = roof;

                var color = map.terrainGrid.ColorAt(c);
                if (color != null)
                    structure.terrainColors[relPos] = color;

                foreach (var thing in c.GetThingList(map).ToList())
                {
                    if (capturedThings.Contains(thing) || thing.Destroyed) continue;
                    if (thing is Skyfaller || thing.def.category == ThingCategory.Ethereal) continue;

                    if (thing.def.category == ThingCategory.Building || thing is Pawn || thing.def.category == ThingCategory.Item)
                    {
                        capturedThings.Add(thing);

                        var relativeRot = thing.Rotation switch
                        {
                            Rot4 r when r == Rot4.East => RotationDirection.Clockwise,
                            Rot4 r when r == Rot4.South => RotationDirection.Opposite,
                            Rot4 r when r == Rot4.West => RotationDirection.Counterclockwise,
                            _ => RotationDirection.None
                        };

                        structure.things[thing] = new PositionData
                        {
                            position = thing.Position - origin,
                            relativeRotation = relativeRot
                        };
                    }
                }
            }

            foreach (var thing in capturedThings)
            {
                if (thing.Spawned)
                    thing.DeSpawn(DestroyMode.Vanish);
            }

            return structure;
        }

        private static bool IsGravshipCell(Map map, IntVec3 c)
        {
            var terrain = map.terrainGrid.TerrainAt(c);
            if (terrain == InternalDefOf.VGE_EnemySubstructure || terrain == InternalDefOf.VGE_EnemySubarmor) return true;
            var foundation = map.terrainGrid.FoundationAt(c);
            return foundation == InternalDefOf.VGE_EnemySubstructure || foundation == InternalDefOf.VGE_EnemySubarmor;
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref things, "things", LookMode.Deep, LookMode.Deep);
            Scribe_Collections.Look(ref roofs, "roofs", LookMode.Value, LookMode.Def);
            Scribe_Collections.Look(ref foundations, "foundations", LookMode.Value, LookMode.Def);
            Scribe_Collections.Look(ref terrains, "terrains", LookMode.Value, LookMode.Def);
            Scribe_Collections.Look(ref terrainColors, "terrainColors", LookMode.Value, LookMode.Def);
        }
    }
}
