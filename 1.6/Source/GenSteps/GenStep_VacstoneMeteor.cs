using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class GenStep_VacstoneMeteor : GenStep_ScatterLumpsMineable
    {
        public override int SeedPart => 1634184422;
        public override void Generate(Map map, GenStepParams parms)
        {
            forcedDefToScatter = InternalDefOf.VGE_Compressed_Vacstone;
            count = 1;
            forcedLumpSize = Rand.RangeInclusive(20, 25);
            base.Generate(map, parms);

            var center = recentLumpCells[0];
            var usedCells = new HashSet<IntVec3>(recentLumpCells);

            var chunks = Rand.RangeInclusive(12, 24);
            var chunkCandidates = GetCandidateCells(map, center, 25, usedCells);
            for (int i = 0; i < chunks && chunkCandidates.Count > 0; i++)
            {
                var c = chunkCandidates.RandomElementByWeight(cell => 1f / cell.DistanceTo(center));
                chunkCandidates.Remove(c);
                usedCells.Add(c);
                foreach (var thing in map.thingGrid.ThingsAt(c).ToList())
                {
                    thing.Destroy(DestroyMode.Vanish);
                }
                GenSpawn.Spawn(InternalDefOf.ChunkVacstone, c, map);
            }

            var craters = Rand.RangeInclusive(7, 12);
            var craterCandidates = GetCandidateCells(map, center, 30, usedCells);
            var spawnedCraters = 0;
            var attempts = 0;
            while (spawnedCraters < craters && attempts < 1000 && craterCandidates.Count > 0)
            {
                var c = craterCandidates.RandomElementByWeight(cell => 1f / cell.DistanceTo(center));
                var craterDef = Rand.Bool ? ThingDefOf.CraterMedium : ThingDefOf.CraterSmall;
                var rect = CellRect.CenteredOn(c, craterDef.size.x, craterDef.size.z);
                if (rect.InBounds(map) && !usedCells.Overlaps(rect))
                {
                    craterCandidates.Remove(c);
                    foreach (var cell in rect.Cells)
                    {
                        foreach (var thing in map.thingGrid.ThingsAt(cell).ToList())
                        {
                            thing.Destroy(DestroyMode.Vanish);
                        }
                        usedCells.Add(cell);
                    }
                    GenSpawn.Spawn(craterDef, c, map);
                    spawnedCraters++;
                }
                else
                {
                    craterCandidates.Remove(c);
                }
                attempts++;
            }
        }

        private List<IntVec3> GetCandidateCells(Map map, IntVec3 center, int radius, HashSet<IntVec3> usedCells)
        {
            var candidates = new List<IntVec3>();
            foreach (var c in GenRadial.RadialCellsAround(center, radius, useCenter: false))
            {
                if (c.InBounds(map) && !usedCells.Contains(c))
                {
                    candidates.Add(c);
                }
            }
            return candidates;
        }

        public override bool CanScatterAt(IntVec3 c, Map map)
        {
            if (MapGenerator.TryGetVar<List<CellRect>>("UsedRects", out var var) && var.Any((CellRect x) => x.Contains(c)))
            {
                return false;
            }
            return map.reachability.CanReachMapEdge(c, TraverseParms.For(TraverseMode.PassDoors));
        }

        public override void ScatterAt(IntVec3 c, Map map, GenStepParams parms, int stackCount = 1)
        {
            base.ScatterAt(c, map, parms, stackCount);
            int minX = recentLumpCells.Min((IntVec3 x) => x.x);
            int minZ = recentLumpCells.Min((IntVec3 x) => x.z);
            int maxX = recentLumpCells.Max((IntVec3 x) => x.x);
            int maxZ = recentLumpCells.Max((IntVec3 x) => x.z);
            CellRect var = CellRect.FromLimits(minX, minZ, maxX, maxZ);
            MapGenerator.SetVar("RectOfInterest", var);
        }
    }
}
