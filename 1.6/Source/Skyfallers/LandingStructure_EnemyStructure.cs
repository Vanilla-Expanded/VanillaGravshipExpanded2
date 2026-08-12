using System.Collections;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using VanillaGravshipExpanded;
using Verse;
using Verse.AI.Group;

namespace VanillaGravshipExpanded2
{
    public class LandingStructure_EnemyStructure : LandingStructureBase
    {
        public EnemyStructure structure;

        public override void Impact()
        {
            var map = Map;
            Rand.PushState(randomSeed);
            RestoreStructureToMap(map, Position, structure);
            Rand.PopState();
            var cellRect = GetStructureBounds(structure).MovedBy(Position);
            Refog(map, cellRect);
            Destroy(DestroyMode.Vanish);
        }

        protected override IEnumerator CaptureGravshipCoroutine()
        {
            coroutineStarted = true;
            var originalMap = Current.Game.CurrentMap;
            var mainCamera = Find.Camera;
            var cameraDriver = mainCamera.GetComponent<CameraDriver>();

            var wasCamDriverEnabled = cameraDriver.enabled;
            var wasCamEnabled = mainCamera.enabled;
            cameraDriver.enabled = false;
            mainCamera.enabled = false;
            Current.Game.CurrentMap = null;

            CreateTempMap(new IntVec3(250, 1, 250), Map, out var mapParent, out var tempMap);
            Current.Game.CurrentMap = tempMap;
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            Rand.PushState(randomSeed);
            RestoreStructureToMap(tempMap, tempMap.Center, structure);
            var cellRect = GetStructureBounds(structure).MovedBy(tempMap.Center);

            foreach (var c in tempMap.AllCells)
            {
                if (tempMap.terrainGrid.TerrainAt(c) == TerrainDefOf.Space)
                {
                    tempMap.terrainGrid.SetTerrain(c, VGEDefOf.VGE_FakeTerrain);
                }
            }

            ScanGeneratedLayout(tempMap, cellRect, out var engine);
            landingRotation = Rot4.North;
            RenderAndSaveTexture(tempMap, mainCamera, cellRect, engine);

            foreach (var kvp in structure.Things)
            {
                if (kvp.Key.Spawned)
                {
                    kvp.Key.DeSpawn(DestroyMode.Vanish);
                }
            }
            Rand.PopState();

            Current.Game.CurrentMap = originalMap;
            mainCamera.enabled = wasCamEnabled;
            cameraDriver.enabled = wasCamDriverEnabled;
            Find.WorldObjects.Remove(mapParent);
            Find.Maps.Remove(tempMap);
            coroutineStarted = false;
        }

        private void RestoreStructureToMap(Map map, IntVec3 center, EnemyStructure structure)
        {
            var assaultPawnsByFaction = new Dictionary<Faction, List<Pawn>>();
            foreach (var kvp in structure.Things)
            {
                var thing = kvp.Key;
                var posData = kvp.Value;
                var targetPos = center + posData.position;
                var rot = GetRotationFromRelative(posData.relativeRotation);

                if (thing is Pawn pawn)
                {
                    GenSpawn.Spawn(pawn, targetPos, map);
                    if (pawn.Faction != null && pawn.Faction != Faction.OfPlayer)
                    {
                        if (!assaultPawnsByFaction.TryGetValue(pawn.Faction, out var list))
                        {
                            list = new List<Pawn>();
                            assaultPawnsByFaction[pawn.Faction] = list;
                        }
                        list.Add(pawn);
                    }
                }
                else
                {
                    GenSpawn.Spawn(thing, targetPos, map, rot);
                }

                if (thing is Building_TurretGun turret)
                {
                    var artillery = turret.TryGetComp<CompWorldArtillery>();
                    if (artillery != null)
                    {
                        artillery.Reset();
                        turret.ResetForcedTarget();
                    }
                }
            }

            foreach (var kvp in assaultPawnsByFaction)
            {
                LordMaker.MakeNewLord(kvp.Key, new LordJob_AssaultColony(kvp.Key, false), map, kvp.Value);
            }

            foreach (var kvp in structure.Terrains)
            {
                var pos = center + kvp.Key;
                map.terrainGrid.SetTerrain(pos, kvp.Value);
            }

            foreach (var kvp in structure.Foundations)
            {
                var pos = center + kvp.Key;
                map.terrainGrid.SetFoundation(pos, kvp.Value);
            }

            foreach (var kvp in structure.Roofs)
            {
                var pos = center + kvp.Key;
                map.roofGrid.SetRoof(pos, kvp.Value);
            }
        }

        private static CellRect GetStructureBounds(EnemyStructure structure)
        {
            int minX = int.MaxValue, minZ = int.MaxValue;
            int maxX = int.MinValue, maxZ = int.MinValue;

            foreach (var kvp in structure.Things)
            {
                var pos = kvp.Value.position;
                minX = System.Math.Min(minX, pos.x);
                maxX = System.Math.Max(maxX, pos.x);
                minZ = System.Math.Min(minZ, pos.z);
                maxZ = System.Math.Max(maxZ, pos.z);
            }

            foreach (var pos in structure.Terrains.Keys)
            {
                minX = System.Math.Min(minX, pos.x);
                maxX = System.Math.Max(maxX, pos.x);
                minZ = System.Math.Min(minZ, pos.z);
                maxZ = System.Math.Max(maxZ, pos.z);
            }

            foreach (var pos in structure.Foundations.Keys)
            {
                minX = System.Math.Min(minX, pos.x);
                maxX = System.Math.Max(maxX, pos.x);
                minZ = System.Math.Min(minZ, pos.z);
                maxZ = System.Math.Max(maxZ, pos.z);
            }

            foreach (var pos in structure.Roofs.Keys)
            {
                minX = System.Math.Min(minX, pos.x);
                maxX = System.Math.Max(maxX, pos.x);
                minZ = System.Math.Min(minZ, pos.z);
                maxZ = System.Math.Max(maxZ, pos.z);
            }

            return CellRect.FromLimits(minX, minZ, maxX, maxZ);
        }

        private static Rot4 GetRotationFromRelative(RotationDirection relativeRotation)
        {
            return relativeRotation switch
            {
                RotationDirection.Clockwise => Rot4.East,
                RotationDirection.Opposite => Rot4.South,
                RotationDirection.Counterclockwise => Rot4.West,
                _ => Rot4.North
            };
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

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref structure, "structure");
        }
    }
}
