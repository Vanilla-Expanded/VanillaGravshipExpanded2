using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace VanillaGravshipExpanded2
{
    [HotSwappable]
    public class LandingStructure_StructureSet : VanillaGravshipExpanded.LandingStructureBase
    {
        public VEF.Storyteller.StructureSetDef structureSetDef;
        public List<Def> selectedDefs;
        public Rot4 shipRotation;
        public Faction shipFaction;
        public IntRange pawnCountRange = new IntRange(0, 0);

        protected Dictionary<Pawn, IntVec3> pawnPositions = new Dictionary<Pawn, IntVec3>();

        protected override IEnumerator CaptureGravshipCoroutine()
        {
            coroutineStarted = true;
            CreateTempMap(new IntVec3(250, 1, 250), Map, out var mapParent, out var tempMap);
            var originalMap = Current.Game.CurrentMap;
            var mainCamera = Find.Camera;
            var cameraDriver = mainCamera.GetComponent<CameraDriver>();
            var wasCamDriverEnabled = cameraDriver.enabled;
            var wasCamEnabled = mainCamera.enabled;
            cameraDriver.enabled = false;
            mainCamera.enabled = false;
            Current.Game.CurrentMap = tempMap;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();
            try
            {
                Rand.PushState(randomSeed);
                var standardLayouts = VEF.Storyteller.StructureSetGenerator.SelectStandardLayouts(structureSetDef, 0f, selectedDefs);
                var preExisting = new HashSet<Thing>(tempMap.listerThings.AllThings);
                var rects = VEF.Storyteller.StructureSetGenerator.Generate(tempMap, structureSetDef, shipFaction, tempMap.Center, standardLayouts, 0f, shipRotation);
                var vacuumCells = tempMap.AllCells.Where(c => tempMap.terrainGrid.TerrainAt(c) == TerrainDefOf.Space).ToHashSet();
                foreach (var c in tempMap.AllCells)
                {
                    if (tempMap.terrainGrid.TerrainAt(c) == TerrainDefOf.Space)
                    {
                        tempMap.terrainGrid.SetTerrain(c, VanillaGravshipExpanded.VGEDefOf.VGE_FakeTerrain);
                    }
                }
                var minX = rects.Min(r => r.minX);
                var minZ = rects.Min(r => r.minZ);
                var maxX = rects.Max(r => r.maxX);
                var maxZ = rects.Max(r => r.maxZ);
                var cellRect = CellRect.FromLimits(minX, minZ, maxX, maxZ);
                Refog(tempMap, cellRect);
                PostProcessMap(tempMap, shipFaction, preExisting);
                GeneratePawns(tempMap, cellRect, vacuumCells);
                Rand.PopState();
                ScanGeneratedLayout(tempMap, cellRect, out var engine);
                enginePos = IntVec3.Invalid;
                landingRotation = Rot4.North;
                RenderAndSaveTexture(tempMap, mainCamera, cellRect, engine);
                DespawnPawns();
            }
            catch (Exception ex)
            {
                Log.Error("Failed to capture " + structureSetDef + ": " + ex.ToString());
            }
            Current.Game.CurrentMap = originalMap;
            mainCamera.enabled = wasCamEnabled;
            cameraDriver.enabled = wasCamDriverEnabled;
            Find.WorldObjects.Remove(mapParent);
            Find.Maps.Remove(tempMap);
            coroutineStarted = false;
        }

        public override void Impact()
        {
            var map = Map;

            var prefabDefsBackup = new Dictionary<PrefabDef, List<PrefabTerrainData>>();
            foreach (var def in DefDatabase<PrefabDef>.AllDefsListForReading)
            {
                if (def.terrain.Any(t => t.def == TerrainDefOf.Space))
                {
                    prefabDefsBackup[def] = new List<PrefabTerrainData>(def.terrain);
                    def.terrain.RemoveAll(t => t.def == TerrainDefOf.Space);
                }
            }

            try
            {
                Rand.PushState(randomSeed);
                var standardLayouts = VEF.Storyteller.StructureSetGenerator.SelectStandardLayouts(structureSetDef, 0f, selectedDefs);
                var preExisting = new HashSet<Thing>(map.listerThings.AllThings);
                var rects = VEF.Storyteller.StructureSetGenerator.Generate(map, structureSetDef, shipFaction, Position, standardLayouts, 0f, shipRotation);
                var minX = rects.Min(r => r.minX);
                var minZ = rects.Min(r => r.minZ);
                var maxX = rects.Max(r => r.maxX);
                var maxZ = rects.Max(r => r.maxZ);
                var cellRect = CellRect.FromLimits(minX, minZ, maxX, maxZ);
                PostProcessMap(map, shipFaction, preExisting);
                Rand.PopState();
                OnImpact(map, cellRect, preExisting);
                foreach (var c in cellRect)
                {
                    if (c.InBounds(map) && c.GetTerrain(map) != TerrainDefOf.Space)
                    {
                        map.fogGrid.Refog(CellRect.SingleCell(c));
                    }
                }
                Destroy(DestroyMode.Vanish);
            }
            finally
            {
                foreach (var kvp in prefabDefsBackup)
                {
                    kvp.Key.terrain.AddRange(kvp.Value);
                }
            }
        }

        private static void Refog(Map map, CellRect cellRect)
        {
            map.fogGrid.SetAllFogged();
            foreach (var allCell in map.AllCells)
            {
                map.mapDrawer.MapMeshDirty(allCell, MapMeshFlagDefOf.FogOfWar);
            }
            FloodFillerFog.FloodUnfog(cellRect.ExpandedBy(1).EdgeCells.RandomElement(), map);
        }

        private void DespawnPawns()
        {
            foreach (var pawn in pawnPositions.Keys)
            {
                pawn.DeSpawn();
            }
        }

        protected virtual void OnImpact(Map map, CellRect cellRect, HashSet<Thing> preExisting)
        {
            var center = cellRect.CenterCell;
            foreach (var pair in pawnPositions)
            {
                GenSpawn.Spawn(pair.Key, center + pair.Value, map);
            }
            if (pawnPositions.Count > 0)
            {
                LordMaker.MakeNewLord(shipFaction, new LordJob_DefendBase(shipFaction, map.Center, 25000), map, pawnPositions.Keys.ToList());
            }
        }

        private void PostProcessMap(Map map, Faction faction, HashSet<Thing> preExisting)
        {
            GenStep_Warplatform.MakeAllCratesANew(map, preExisting);
            foreach (var thing in map.listerThings.AllThings)
            {
                if (preExisting.Contains(thing)) continue;
                if (faction != null && thing.Faction != faction) continue;
                if (thing.TryGetComp<CompRefuelable>() is CompRefuelable r) r.Refuel(r.Props.fuelCapacity);
                if (thing.TryGetComp<CompPowerBattery>() is CompPowerBattery b) b.SetStoredEnergyPct(1f);
                if (thing.TryGetComp<CompPower_InputOnlyBattery>() is CompPower_InputOnlyBattery ib) ib.SetStoredEnergyPct(1f);
                if (thing.TryGetComp<PipeSystem.CompResourceStorage>() is PipeSystem.CompResourceStorage rs) rs.AddResource(rs.Props.storageCapacity);
            }
        }

        private void GeneratePawns(Map map, CellRect cellRect, HashSet<IntVec3> vacuumCells)
        {
            if (shipFaction == null || pawnCountRange.max <= 0) return;
            var cells = cellRect.Cells.Where(c => c.Standable(map) && c.Roofed(map) && !vacuumCells.Contains(c)).ToList();
            var center = cellRect.CenterCell;
            var pawnCount = pawnCountRange.RandomInRange;
            for (var i = 0; i < pawnCount; i++)
            {
                var kind = shipFaction.def.pawnGroupMakers.SelectMany(gm => gm.options).RandomElementByWeight(opt => opt.selectionWeight).kind;
                var pawn = PawnGenerator.GeneratePawn(kind, shipFaction);
                var cell = cells.RandomElement();
                pawnPositions[pawn] = cell - center;
                GenSpawn.Spawn(pawn, cell, map);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref structureSetDef, "structureSetDef");
            Scribe_Collections.Look(ref selectedDefs, "selectedDefs", LookMode.Def);
            Scribe_Values.Look(ref shipRotation, "shipRotation");
            Scribe_Values.Look(ref ticksToImpact, "ticksToImpact");
            Scribe_Values.Look(ref ticksToImpactMax, "ticksToImpactMax");
            Scribe_References.Look(ref shipFaction, "shipFaction");
            Scribe_Values.Look(ref pawnCountRange, "pawnCountRange");
            Scribe_Collections.Look(ref pawnPositions, "pawnPositions", LookMode.Deep, LookMode.Value);
        }
    }
}
