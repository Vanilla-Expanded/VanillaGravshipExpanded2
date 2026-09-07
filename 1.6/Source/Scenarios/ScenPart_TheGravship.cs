using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using VanillaGravshipExpanded;
using VEF.Storyteller;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class ScenPart_TheGravship : ScenPart
    {
        private const float StartingVisibility = 460000f;
        private const int EngineCooldownDuration = 9 * GenDate.TicksPerHour;
        private const int EnemyArrivalDelay = 8 * GenDate.TicksPerHour;

        public static ScenPart_TheGravship Active => Find.Scenario.AllParts.OfType<ScenPart_TheGravship>().FirstOrDefault();

        private int engineReadyTick = -1;
        public int enemyArrivalTick = -1;
        private bool engineReadyLetterSent;
        public bool enemyArrived;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref engineReadyTick, "engineReadyTick", -1);
            Scribe_Values.Look(ref enemyArrivalTick, "enemyArrivalTick", -1);
            Scribe_Values.Look(ref engineReadyLetterSent, "engineReadyLetterSent", false);
            Scribe_Values.Look(ref enemyArrived, "enemyArrived", false);
        }

        public override void PostMapGenerate(Map map)
        {
            base.PostMapGenerate(map);
            if (Find.GameInitData is null) return;
            enemyArrived = false;
            engineReadyLetterSent = false;
            WorldComponent_GravshipCombat.Instance.visibility = StartingVisibility;
            engineReadyTick = Find.TickManager.TicksGame + EngineCooldownDuration;
            enemyArrivalTick = Find.TickManager.TicksGame + EnemyArrivalDelay;
            var engine = GravEngineTracker.GetPlayerGravEngine();
            engine.cooldownCompleteTick = engineReadyTick;
            // Battery refill handled by ScenPart_PlayerPawnsArriveMethod in VGE1 and our hook to postGravshipGenerated in VGE2
        }

        public void NotifyPlayerEscaped()
        {
            enemyArrivalTick = -1;
        }

        public override void Tick()
        {
            base.Tick();
            if (enemyArrived is false && enemyArrivalTick > 0 && Find.TickManager.TicksGame >= enemyArrivalTick)
            {
                enemyArrived = true;
                TriggerEnemyArrival();
            }

            if (engineReadyLetterSent is false && engineReadyTick > 0 && Find.TickManager.TicksGame >= engineReadyTick)
            {
                engineReadyLetterSent = true;
                Find.LetterStack.ReceiveLetter("VGE_GravshipEngineReady".Translate(), "VGE_GravshipEngineReadyDesc".Translate(), LetterDefOf.NeutralEvent);
            }
        }

        private void TriggerEnemyArrival()
        {
            var map = WorldComponent_GravshipCombat.Instance.GetPlayerTargetMap();
            var shipName = NameGenerator.GenerateName(InternalDefOf.VGE_NamerEnemyGravship);
            var landingStructure = (LandingStructure_EnemyGravshipRaid)ThingMaker.MakeThing(InternalDefOf.VGE_LandingStructure_EnemyGravshipRaid);
            landingStructure.structureSetDef = InternalDefOf.VGE_EnemyGravshipSet;
            var standardLayouts = StructureSetGenerator.SelectStandardLayouts(InternalDefOf.VGE_EnemyGravshipSet);
            landingStructure.selectedDefs = standardLayouts.Select(x => x.def).ToList();
            landingStructure.shipRotation = Rot4.Random;
            landingStructure.shipFaction = Faction.OfSalvagers;
            landingStructure.pawnCountRange = new IntRange(9, 14);
            var spawnSpot = FindEdgeSpawnSpot(map, standardLayouts, landingStructure.shipRotation);
            landingStructure.spawnedByScenario = true;
            GenSpawn.Spawn(landingStructure, spawnSpot, map);
            Find.LetterStack.ReceiveLetter("VGE_RaidEnemyGravship".Translate(), "VGE_RaidEnemyGravshipDesc".Translate(shipName.Colorize(ColorLibrary.RedReadable)), LetterDefOf.ThreatBig, new LookTargets(landingStructure));
        }

        private static IntVec3 FindEdgeSpawnSpot(Map map, List<(StructurePatternOffset layout, Def def)> standardLayouts, Rot4 rotation)
        {
            var footprint = StructureSetGenerator.GetFootprint(standardLayouts, rotation);
            var halfX = footprint.x / 2;
            var halfZ = footprint.z / 2;
            var bestCell = new IntVec3(Mathf.Clamp(map.Center.x, halfX, map.Size.x - 1 - halfX), 0, Mathf.Clamp(map.Center.z, halfZ, map.Size.z - 1 - halfZ));
            var bestDist = int.MaxValue;
            for (var i = 0; i < 300; i++)
            {
                var candidate = new IntVec3(Rand.RangeInclusive(halfX, map.Size.x - 1 - halfX), 0, Rand.RangeInclusive(halfZ, map.Size.z - 1 - halfZ));
                if (CellRect.CenteredOn(candidate, footprint.x, footprint.z).Contains(map.Center)) continue;
                var distToEdge = Mathf.Min(candidate.x, map.Size.x - 1 - candidate.x, candidate.z, map.Size.z - 1 - candidate.z);
                if (distToEdge < bestDist)
                {
                    bestDist = distToEdge;
                    bestCell = candidate;
                }
            }

            return bestCell;
        }
    }
}
