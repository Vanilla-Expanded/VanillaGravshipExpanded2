using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HotSwappable]
    public class GravshipThreatWorker
    {
        public GravshipThreatDef def;
        public virtual Faction EnemyFaction => Faction.OfAncientsHostile;
        public virtual bool CanFire(Building_GravEngine engine)
        {
            if (def.allowedEngineDefs != null && !def.allowedEngineDefs.Contains(engine.def))
            {
                return false;
            }
            return true;
        }

        public virtual bool ShouldDefeat(Map map)
        {
            if (def.defeatBuildings == null) return false;
            foreach (var building in def.defeatBuildings)
            {
                if (map.listerThings.ThingsOfDef(building).Any(x => x.Faction != Faction.OfPlayer))
                {
                    return false;
                }
            }
            return true;
        }

        public virtual void Fire(Building_GravEngine engine)
        {
            var comp = WorldComponent_GravshipCombat.Instance;
            var map = comp.GetPlayerTargetMap();
            comp.lastKnownShipMap = engine?.Map ?? map;
            comp.lastKnownShipTile = engine?.Tile ?? map.Tile;
            comp.activeThreatDef = def;
            comp.incomingWarplatform = true;
            comp.warplatformTick = Find.TickManager.TicksGame + def.baseCountdownHours.RandomInRange * GenDate.TicksPerHour;

            var jammer = TryApplyJammer(engine);
            var desc = GetLetterDesc(jammer);
            Find.LetterStack.ReceiveLetter(def.letterLabel, desc, LetterDefOf.ThreatBig);
        }

        public CompSignalJammer TryApplyJammer(Building_GravEngine engine)
        {
            if (engine == null) return null;
            var jammer = engine.AffectedByFacilities.LinkedFacilitiesListForReading
                .OfType<ThingWithComps>()
                .Where(t => t.def == InternalDefOf.SignalJammer)
                .Select(t => t.GetComp<CompSignalJammer>())
                .FirstOrDefault(c => c != null && !c.OnCooldown);
            if (jammer != null)
            {
                jammer.StartCooldown();
                WorldComponent_GravshipCombat.Instance.warplatformTick += def.jammerExtensionHours * GenDate.TicksPerHour;
            }
            return jammer;
        }

        public virtual TaggedString GetLetterDesc(CompSignalJammer jammer)
        {
            var desc = (TaggedString)def.letterDesc;
            if (jammer != null)
            {
                desc += "\n\n" + "VGE_JammerScrambledSignal".Translate(def.jammerExtensionHours);
            }
            return desc;
        }

        public virtual void SpawnThreat(bool suppressArrivalLetter = false)
        {
            var map = WorldComponent_GravshipCombat.Instance.GetPlayerTargetMap();
            var activeThreatDef = WorldComponent_GravshipCombat.Instance.activeThreatDef;

            var warplatform = (MapParent_WarPlatform)WorldObjectMaker.MakeWorldObject(activeThreatDef.worldObjectDef);
            warplatform.threatDef = activeThreatDef;

            var engineTile = map.Tile;
            if (!Find.WorldGrid.TryGetFirstAdjacentLayerOfDef(engineTile, PlanetLayerDefOf.Orbit, out var orbitLayer))
            {
                return;
            }
            var orbitTile = engineTile.LayerDef == PlanetLayerDefOf.Orbit ? engineTile : orbitLayer.GetClosestTile_NewTemp(engineTile);
            var validTiles = new List<PlanetTile>();
            foreach (var tile in orbitLayer.Tiles)
            {
                var distance = DistanceUtil.GetDistanceInOrbitTiles(tile.tile, orbitTile);
                if (distance < def.escapeDistance && def.tileSpawnDistanceRange.Includes(distance))
                {
                    if (!def.tileSpawnDistanceRange.Includes(GravshipHelper.GetDistance(tile.tile, engineTile)))
                    {
                        continue;
                    }
                    var worldObjects = Find.WorldObjects.ObjectsAt(tile.tile);
                    if (worldObjects.Any(x => x is Settlement || x is Site))
                    {
                        continue;
                    }
                    validTiles.Add(tile.tile);
                }
            }
            if (validTiles.TryRandomElement(out var result))
            {
                var worldObjects = Find.WorldObjects.ObjectsAt(result).ToList();
                foreach (var worldObject in worldObjects)
                {
                    Find.WorldObjects.Remove(worldObject);
                }
                SpawnWarplatform(map, warplatform, result, suppressArrivalLetter);
            }
            else
            {
                Log.Error("Failed to create warplatform due to finding no good orbital tile.");
            }
        }

        private void SpawnWarplatform(Map playerMap, MapParent_WarPlatform warplatform, PlanetTile tile, bool suppressArrivalLetter)
        {
            warplatform.Tile = tile;
            warplatform.SetFaction(EnemyFaction);
            Find.WorldObjects.Add(warplatform);
            LongEventHandler.QueueLongEvent(delegate
            {
                MapGenerator.GenerateMap(new IntVec3(def.mapSize, 1, def.mapSize), warplatform, warplatform.MapGeneratorDef);
                if (!suppressArrivalLetter)
                {
                    var engine = GravEngineTracker.GetPlayerGravEngine();
                    var shipName = engine != null ? engine.RenamableLabel : (string)"VGE_GravshipGeneric".Translate();
                    Find.LetterStack.ReceiveLetter(def.arrivalLetterLabel, def.arrivalLetterDesc.Formatted(shipName), LetterDefOf.ThreatBig, new LookTargets(warplatform));
                }
                CameraJumper.TryJump(new GlobalTargetInfo(warplatform.Map.Center, warplatform.Map));
                Find.CameraDriver.SetRootPosAndSize(warplatform.Map.Center.ToVector3(), 60f);
            }, "GeneratingMap", doAsynchronously: true, GameAndMapInitExceptionHandlers.ErrorWhileGeneratingMap);
        }

        public virtual void OnDefeat(Map map)
        {
            foreach (var thing in map.listerBuildings.allBuildingsNonColonist.OfType<Building_GravshipTurret>()) thing.DisablePermanently();
        }

        public virtual void OnEscape(MapParent_WarPlatform warplatform)
        {
            Messages.Message(def.escapedMessage, MessageTypeDefOf.PositiveEvent);
            WorldComponent_GravshipCombat.Instance.visibility = Mathf.Max(0, WorldComponent_GravshipCombat.Instance.visibility - def.escapeMidBattleVisibilityLoss);
        }

        public virtual void OnEarlyEscape(Map map)
        {
            var comp = WorldComponent_GravshipCombat.Instance;
            comp.visibility = Mathf.Max(0, comp.visibility - def.earlyEscapeVisibilityLoss);
            Messages.Message(def.earlyEscapeMessage, MessageTypeDefOf.PositiveEvent);
        }

        public virtual void OnEngineDestroyed(MapParent_WarPlatform warplatform)
        {
            Find.LetterStack.ReceiveLetter(def.disengagesLetter, def.disengagesLetterDesc, LetterDefOf.NegativeEvent);
        }
    }
}
