using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using VEF.Storyteller;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class GenStep_DerelictGravship : GenStep
    {
        public override int SeedPart => 1345184427;
        public StructureSetDef structureSetDef;

        public override void Generate(Map map, GenStepParams parms)
        {
            map.OrbitalDebris = InternalDefOf.VGE_GravshipDebris;
            var cells = StructureSetGenerator.Generate(map, structureSetDef, map.ParentFaction).SelectMany(x => x.Cells).Distinct().ToList();
            GenStep_Warplatform.MakeAllCratesANew(map);
            var derelict = map.Parent as MapParent_DerelictGravship;
            var points = Mathf.Max(derelict.threatPoints, StorytellerUtility.DefaultThreatPointsNow(Find.World));
            switch (derelict.threatType)
            {
                case DerelictThreatType.Exoinsectoids:
                    ScatterThings(map, ThingDefOf.Filth_Slime, Rand.RangeInclusive(20, 40), cells);
                    ScatterThings(map, ThingDefOf.GlowPod, Rand.RangeInclusive(8, 15), cells);
                    var hives = Rand.RangeInclusive(3, 5);
                    for (var i = 0; i < hives; i++)
                    {
                        if (cells.Where(c => c.Standable(map) && c.GetTerrain(map) != TerrainDefOf.Space).TryRandomElement(out var cell))
                        {
                            var hive = GenSpawn.Spawn(InternalDefOf.VGE_ExoHive_Building, cell, map);
                            hive.SetFaction(Faction.OfInsects);
                        }
                    }
                    SpawnInsects(map, Faction.OfInsects, points, new List<(PawnKindDef, float)> {
                            (InternalDefOf.VGE_Exoworm, 100f),
                            (InternalDefOf.VGE_Exopede, 50f),
                            (InternalDefOf.VGE_Exoleech, 20f),
                            (InternalDefOf.VGE_ExowormKnot, 5f)
                        }, cells);
                    break;

                case DerelictThreatType.Mechanoids:
                    ScatterThings(map, ThingDefOf.ChunkMechanoidSlag, Rand.RangeInclusive(5, 15), cells);
                    SpawnPawnGroup(map, Faction.OfMechanoids, points, cells);
                    break;

                case DerelictThreatType.Drones:
                    ScatterThings(map, ThingDefOf.WaspDroneTrap, Rand.RangeInclusive(2, 3), cells);
                    ScatterThings(map, ThingDefOf.HunterDroneTrap, Rand.RangeInclusive(2, 3), cells);
                    var droneCount = Rand.RangeInclusive(5, 8);
                    for (var i = 0; i < droneCount; i++)
                    {
                        if (cells.Where(c => c.Standable(map) && c.GetTerrain(map) != TerrainDefOf.Space).TryRandomElement(out var cell))
                        {
                            var drone = PawnGenerator.GeneratePawn(PawnKindDefOf.Drone_Sentry, Faction.OfAncientsHostile);
                            GenSpawn.Spawn(drone, cell, map);
                            drone.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter);
                        }
                    }
                    break;

                case DerelictThreatType.Pirates:
                    SpawnPawnGroup(map, Faction.OfSalvagers, points, cells);
                    break;
            }
        }

        private void SpawnInsects(Map map, Faction faction, float points, List<(PawnKindDef, float)> weights, List<IntVec3> cells)
        {
            var curPts = 0f;
            while (curPts < points)
            {
                var kind = weights.RandomElementByWeight(w => w.Item2).Item1;
                var pawn = PawnGenerator.GeneratePawn(kind, faction);
                if (cells.Where(c => c.Standable(map) && c.GetTerrain(map) != TerrainDefOf.Space && c.Roofed(map)).TryRandomElement(out var cell) || cells.Where(c => c.Standable(map) && c.GetTerrain(map) != TerrainDefOf.Space).TryRandomElement(out cell))
                {
                    GenSpawn.Spawn(pawn, cell, map);
                    curPts += kind.combatPower;
                    pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter);
                }
                else break;
            }
        }

        private void SpawnPawnGroup(Map map, Faction faction, float points, List<IntVec3> cells)
        {
            points = Mathf.Max(points, faction.def.MinPointsToGeneratePawnGroup(PawnGroupKindDefOf.Combat));
            var groupParms = new PawnGroupMakerParms
            {
                faction = faction,
                groupKind = PawnGroupKindDefOf.Combat,
                points = points
            };
            foreach (var pawn in PawnGroupMakerUtility.GeneratePawns(groupParms))
            {
                if (cells.Where(c => c.Standable(map) && c.GetTerrain(map) != TerrainDefOf.Space && c.Roofed(map)).TryRandomElement(out var cell) || cells.Where(c => c.Standable(map) && c.GetTerrain(map) != TerrainDefOf.Space).TryRandomElement(out cell))
                {
                    GenSpawn.Spawn(pawn, cell, map);
                }
            }
        }

        private void ScatterThings(Map map, ThingDef def, int count, List<IntVec3> cells)
        {
            for (var i = 0; i < count; i++)
            {
                if (cells.Where(c => c.Standable(map) && c.GetTerrain(map) != TerrainDefOf.Space && c.Roofed(map)).TryRandomElement(out var cell) || cells.Where(c => c.Standable(map) && c.GetTerrain(map) != TerrainDefOf.Space).TryRandomElement(out cell))
                {
                    GenSpawn.Spawn(def, cell, map);
                }
            }
        }
    }
}
