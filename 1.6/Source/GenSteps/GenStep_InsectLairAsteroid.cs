using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Noise;
using static HarmonyLib.Code;

namespace VanillaGravshipExpanded2
{
    public class GenStep_InsectLairAsteroid : GenStep
    {

        public class MineableCountConfig
        {
            public ThingDef mineable;

            public IntRange countRange;

            public void LoadDataFromXmlCustom(XmlNode xmlRoot)
            {
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "mineable", xmlRoot.Name);
                countRange = IntRange.FromString(xmlRoot.FirstChild.Value);
            }
        }

        public List<MineableCountConfig> mineableCounts;

        public IntRange wormKnotAmount;
        public IntRange wormspitterAmount;
        public IntRange exohiveAmount;
        public IntRange mineableAmounts;
        public IntRange eggSacsInRooms;
        public IntRange numMiscPOIsRange;
        public IntRange numThreatPOIsRange;

        private enum ThreatPOIType
        {
            InsectHives,
            InsectCocoons
        }

        private enum MiscPOIType
        {
            EggSacs,
            Metals,
            Corpses
        }

        private const int MinDistBetweenPOIs = 15;

        private const int MinDistFromEdge = 10;

        private const int MinDistFromEntrance = 20;

        private const float SludgeThreshold = 0.2f;

        public const float SlimeChance = 0.2f;

        public const float SacChance = 0.02f;

        private const int BurrowWallSize = 154;

    
        private static readonly IntRange GravlitePanelCount = new IntRange(100, 150);

        private static readonly SimpleCurve HivesFromThreatPointsCurve = new SimpleCurve
    {
        new CurvePoint(100f, 1f),
        new CurvePoint(800f, 2f),
        new CurvePoint(5000f, 4f),
        new CurvePoint(10000f, 8f)
    };



        private static readonly SimpleCurve HiveThreatPointsCurve = new SimpleCurve
    {
        new CurvePoint(100f, 1f),
        new CurvePoint(800f, 2f),
        new CurvePoint(5000f, 7f),
        new CurvePoint(10000f, 10f)
    };

        private static readonly SimpleCurve BossHivesThreatPointsCurve = new SimpleCurve
    {
        new CurvePoint(100f, 1f),
        new CurvePoint(800f, 2f),
        new CurvePoint(5000f, 4f),
        new CurvePoint(10000f, 8f)
    };

        private List<IntVec3> poiCells = new List<IntVec3>();

        public override int SeedPart => 98236175;

        public override void Generate(Map map, GenStepParams parms)
        {
            if (!ModsConfig.OdysseyActive)
            {
                return;
            }
            map.regionAndRoomUpdater.Enabled = true;
            poiCells.Clear();
            ModuleBase moduleBase = new Perlin(0.1, 0.9, 1.5, 4, Rand.Int, QualityMode.Medium);
            foreach (IntVec3 allCell in map.AllCells)
            {
                if (allCell.GetEdifice(map) == null && GenConstruct.CanBuildOnTerrain(InternalDefOf.VGE_Subcreep, allCell, map, Rot4.North) && moduleBase.GetValue(allCell) > 0.2f)
                {
                    TryPlaceSludge(allCell, map);
                }
            }


            for (int i = 0; i < numThreatPOIsRange.max + numMiscPOIsRange.max + 1; i++)
            {
                IntVec3 pOILocation = GetPOILocation(map);
                if (pOILocation.IsValid)
                {
                    poiCells.Add(pOILocation);
                }
            }
            GenerateRandomBurrowWalls(map);
            SpawnOres(map, parms);
            foreach (IntVec3 allCell2 in map.AllCells)
            {
                if (Rand.Chance(0.02f) && allCell2.GetTerrain(map) == InternalDefOf.VGE_Subcreep)
                {
                    Thing thing = ThingMaker.MakeThing(InternalDefOf.VGE_EggSac);
                    thing.SetFaction(Faction.OfInsects);
                    GenPlace.TryPlaceThing(thing, allCell2, map, ThingPlaceMode.Near);
                }
            }

            GenerateThreatPOIs(map);
            GenerateMiscPOIs(map);
            Map target = (map.Parent as PocketMapParent)?.sourceMap ?? map;
            GenerateRandomHives(map, exohiveAmount.RandomInRange);
            int wormspitters = wormspitterAmount.RandomInRange;
            int wormKnots = wormKnotAmount.RandomInRange;

            foreach (IntVec3 allCell3 in map.AllCells.InRandomOrder())
            {
                if (allCell3.GetTerrain(map) == InternalDefOf.VGE_Subcreep && !allCell3.Roofed(map))
                {
                    Thing thing = ThingMaker.MakeThing(InternalDefOf.VGE_GiantWormspitter);
                    thing.SetFaction(Faction.OfInsects);
                    GenPlace.TryPlaceThing(thing, allCell3, map, ThingPlaceMode.Near);
                    wormspitters--;
                }
                if (wormspitters <= 0)
                {
                    break;
                }
            }

            foreach (IntVec3 allCell4 in map.AllCells.InRandomOrder())
            {
                if (allCell4.GetTerrain(map) == InternalDefOf.VGE_Subcreep && allCell4.Walkable(map))
                {
                    Pawn p = PawnGenerator.GeneratePawn(InternalDefOf.VGE_ExowormKnot, Faction.OfInsects);
                    GenSpawn.Spawn(p, allCell4, map);
                    wormKnots--;
                }
                if (wormKnots <= 0)
                {
                    break;
                }
            }
        }

        private void SpawnOres(Map map, GenStepParams parms)
        {
            foreach (MineableCountConfig mineable in mineableCounts)
            {
                ThingDef thingDef = mineable?.mineable;
                if (thingDef != null)
                {
                    int forcedLumpSize = mineable.countRange.RandomInRange;
                    GenStep_ScatterLumpsMineable genStep_ScatterLumpsMineable = new GenStep_ScatterLumpsMineable();
                    genStep_ScatterLumpsMineable.count = mineableAmounts.RandomInRange;
                    genStep_ScatterLumpsMineable.forcedDefToScatter = thingDef;
                    genStep_ScatterLumpsMineable.forcedLumpSize = forcedLumpSize;
                    genStep_ScatterLumpsMineable.Generate(map, parms);
                }
            }
        }

        private static void GenerateRandomBurrowWalls(Map map)
        {
            Perlin perlin = new Perlin(0.08, 2.0, 2.0, 2, Rand.Int, QualityMode.Medium);
            foreach (IntVec3 allCell in map.AllCells)
            {
                float value = perlin.GetValue(allCell);
                if (allCell.GetFirstBuilding(map) == null && value > 0.7f && GenSpawn.CanSpawnAt(ThingDefOf.BurrowWall, allCell, map))
                {
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.BurrowWall);
                    if (GenPlace.TryPlaceThing(thing, allCell, map, ThingPlaceMode.Direct))
                    {
                        thing.SetFaction(Faction.OfInsects);
                    }
                }
            }
        }

        private IntVec3 GetPOILocation(Map map)
        {

            if (!CellFinder.TryFindRandomCell(map, delegate (IntVec3 c)
            {
                if (!c.Standable(map))
                {
                    return false;
                }
                if (c.DistanceToEdge(map) < 10)
                {
                    return false;
                }

                foreach (IntVec3 poiCell in poiCells)
                {
                    if (c.InHorDistOf(poiCell, 15f))
                    {
                        return false;
                    }
                }
                return true;
            }, out var result))
            {
                return IntVec3.Invalid;
            }
            return result;
        }

        private void GenerateThreatPOIs(Map map)
        {
            int randomInRange = numThreatPOIsRange.RandomInRange;
            for (int i = 0; i < randomInRange; i++)
            {
                ThreatPOIType threatPOIType = Rand.EnumValue<ThreatPOIType>();
                if (!poiCells.Empty())
                {
                    IntVec3 intVec = poiCells.RandomElement();
                    poiCells.Remove(intVec);
                    ClearArea(intVec, map);
                    GeneratePOIWalls(intVec, map, out var _);
                    switch (threatPOIType)
                    {
                        case ThreatPOIType.InsectHives:
                            GenerateInsectHivePOI(intVec, map);
                            break;
                        case ThreatPOIType.InsectCocoons:
                            GenerateInsectCocoonPOI(intVec, map);
                            break;

                    }
                }
            }
        }

        private void GenerateMiscPOIs(Map map)
        {
            int randomInRange = numMiscPOIsRange.RandomInRange;
            for (int i = 0; i < randomInRange; i++)
            {
                MiscPOIType miscPOIType = Rand.EnumValue<MiscPOIType>();
                if (!poiCells.Empty())
                {
                    IntVec3 intVec = poiCells.RandomElement();
                    poiCells.Remove(intVec);
                    ClearArea(intVec, map);
                    GeneratePOIWalls(intVec, map, out var _);
                    switch (miscPOIType)
                    {
                        case MiscPOIType.EggSacs:
                            GenerateEggSacs(intVec, map, eggSacsInRooms.RandomInRange);
                            GenerateHermeticCratePOI(intVec, map);
                            break;
                        case MiscPOIType.Metals:
                            GenerateRandomMetal(intVec, map);
                            GenerateHermeticCratePOI(intVec, map);
                            break;

                        case MiscPOIType.Corpses:
                            GenStep_UndercaveInterest.GenerateCorpsePile(map, intVec);
                            GenerateHermeticCratePOI(intVec, map);
                            break;


                    }
                }
            }
        }
        private void GenerateHermeticCratePOI(IntVec3 loc, Map map)
        {
            Rot4 rot = Rot4.Random;
            if (CellFinder.TryRandomClosewalkCellNear(loc, map, 5, out var result, (IntVec3 c) => GenSpawn.CanSpawnAt(ThingDefOf.AncientHermeticCrate, c, map, rot)))
            {
                RoomGenUtility.SpawnCrate(ThingDefOf.AncientHermeticCrate, result, map, rot, ThingSetMakerDefOf.MapGen_HighValueCrate);
                int num = Rand.Range(3, 4);
                GenerateEggSacs(loc, map, num);
            }
        }

        private void GenerateRandomHives(Map map, int count)
        {

            for (int i = 0; i < count; i++)
            {
                if (CellFinder.TryFindRandomCell(map, (IntVec3 c) => GenSpawn.CanSpawnAt(ThingDefOf.Hive, c, map), out var result))
                {
                    SpawnExoHive(result, map, WipeMode.VanishOrMoveAside, spawnInsectsImmediately: true, canSpawnHives: false);
                }
            }
        }


        private void ClearArea(IntVec3 loc, Map map)
        {
            foreach (IntVec3 item in GridShapeMaker.IrregularLump(loc, map, 50, (IntVec3 c) => true))
            {
                item.GetEdifice(map)?.Destroy();
            }
        }

        private void GeneratePOIWalls(IntVec3 loc, Map map, out List<IntVec3> shape)
        {
            shape = GridShapeMaker.IrregularLump(loc, map, 154, (IntVec3 c) => true);
            foreach (IntVec3 item in shape)
            {
                TryPlaceSludge(item, map);
            }
            List<IntVec3> list = new List<IntVec3>();
            foreach (IntVec3 item2 in shape)
            {
                bool flag = false;
                foreach (IntVec3 item3 in GenRadial.RadialCellsAround(item2, 2f, useCenter: false))
                {
                    if (!shape.Contains(item3))
                    {
                        flag = true;
                    }
                }
                if (!flag)
                {
                    list.Add(item2);
                }
            }
            foreach (IntVec3 item4 in list)
            {
                shape.Remove(item4);
            }
            foreach (IntVec3 item5 in shape)
            {
                if (item5.DistanceToEdge(map) > 1)
                {
                    Thing thing = ThingMaker.MakeThing(ThingDefOf.BurrowWall);
                    if (GenPlace.TryPlaceThing(thing, item5, map, ThingPlaceMode.Direct))
                    {
                        thing.SetFaction(Faction.OfInsects);
                    }
                }
            }
        }

        private void GenerateEggSacs(IntVec3 loc, Map map, int num)
        {
            for (int i = 0; i < num; i++)
            {
                Thing thing = ThingMaker.MakeThing(InternalDefOf.VGE_EggSac);
                thing.SetFaction(Faction.OfInsects);
                GenPlace.TryPlaceThing(thing, loc, map, ThingPlaceMode.Radius, null, null, null, 5);
            }
        }

        private void GenerateRandomMetal(IntVec3 loc, Map map)
        {
            MineableCountConfig mineable = mineableCounts.RandomElement();

            ThingDef thingDef = mineable?.mineable;
            if (thingDef != null)
            {              
                foreach (IntVec3 item in GridShapeMaker.IrregularLump(loc, map, mineable.countRange.RandomInRange, Validator))
                {
                    GenSpawn.Spawn(thingDef, item, map);                 
                }
                bool Validator(IntVec3 cell)
                {                    
                        return true;                    
                }
            }
        }

        private void GenerateInsectHivePOI(IntVec3 loc, Map map)
        {
            int num = (int)HivesFromThreatPointsCurve.Evaluate(StorytellerUtility.DefaultThreatPointsNow(Find.AnyPlayerHomeMap));
            for (int i = 0; i < num; i++)
            {
                if (CellFinder.TryRandomClosewalkCellNear(loc, map, 5, out var result, (IntVec3 c) => GenSpawn.CanSpawnAt(ThingDefOf.Hive, c, map)))
                {
                    SpawnExoHive(result, map, WipeMode.VanishOrMoveAside, spawnInsectsImmediately: false, canSpawnHives: false, canSpawnInsects: true, dormant: true);
                }
            }
            int num2 = Rand.Range(2, 4);
            GenerateEggSacs(loc, map, num2);
        }

        private void GenerateInsectCocoonPOI(IntVec3 loc, Map map)
        {
            IEnumerable<ThingDef> cocoonsToSpawn = new List<ThingDef>() { InternalDefOf.VGE_ExowormCocoon };
            int nextCocoonGroupID = Find.UniqueIDsManager.GetNextCocoonGroupID();
            foreach (ThingDef item in cocoonsToSpawn)
            {
                Thing thing = ThingMaker.MakeThing(item);
                thing.SetFaction(Faction.OfInsects);
                thing.TryGetComp<CompWakeUpDormant>().groupID = nextCocoonGroupID;
                GenPlace.TryPlaceThing(thing, loc, map, ThingPlaceMode.Radius, null, null, null, 5);
            }
        }



        private void TryPlaceSludge(IntVec3 cell, Map map)
        {
            if (GenConstruct.CanBuildOnTerrain(InternalDefOf.VGE_Subcreep, cell, map, Rot4.North))
            {
                map.terrainGrid.SetTerrain(cell, InternalDefOf.VGE_Subcreep);
               
                if (Rand.Chance(0.2f))
                {
                    GenSpawn.Spawn(ThingDefOf.Filth_Slime, cell, map);
                }
            }
        }

        public static Hive SpawnExoHive(IntVec3 spawnCell, Map map, WipeMode wipeMode = WipeMode.VanishOrMoveAside, bool spawnInsectsImmediately = false, bool canSpawnHives = true, bool canSpawnInsects = true, bool dormant = false, bool aggressive = true, bool spawnJellyImmediately = false, bool spawnSludge = true)
        {
            Hive hive = (Hive)GenSpawn.Spawn(ThingMaker.MakeThing(InternalDefOf.VGE_ExoHive_Building), spawnCell, map, wipeMode);
            hive.SetFaction(Faction.OfInsects);
            hive.PawnSpawner.aggressive = aggressive;
            if (dormant)
            {
                hive.CompDormant.ToSleep();
            }

            if (spawnInsectsImmediately)
            {
                hive.PawnSpawner.SpawnPawnsUntilPoints(Rand.Range(200f, 500f));
                hive.CompDormant.WakeUp();
            }

            if (spawnJellyImmediately)
            {
                foreach (CompSpawner comp in hive.GetComps<CompSpawner>())
                {
                    if (comp.PropsSpawner.thingToSpawn == ThingDefOf.InsectJelly)
                    {
                        comp.TryDoSpawn();
                        break;
                    }
                }
            }

            hive.PawnSpawner.canSpawnPawns = canSpawnInsects;
            hive.GetComp<CompSpawnerHives>().canSpawnHives = canSpawnHives;
            //SpawnExtras(hive, map, wipeMode, spawnSludge);
            return hive;
        }
    }
}