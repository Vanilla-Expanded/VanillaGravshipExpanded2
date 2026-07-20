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
            comp.activeThreatDef = def;
            comp.incomingWarplatform = true;
            comp.warplatformTick = Find.TickManager.TicksGame + def.baseCountdownHours * GenDate.TicksPerHour;

            var jammer = engine.AffectedByFacilities.LinkedFacilitiesListForReading.OfType<Building_SignalJammer>().FirstOrDefault(x => !x.OnCooldown);
            var desc = def.letterDesc;
            if (jammer != null)
            {
                jammer.StartCooldown();
                comp.warplatformTick += def.jammerExtensionHours * GenDate.TicksPerHour;
                desc += "\n\n" + "VGE_JammerScrambledSignal".Translate(def.jammerExtensionHours);
            }

            Find.LetterStack.ReceiveLetter(def.letterLabel, desc, LetterDefOf.ThreatBig);
        }

        public virtual void SpawnThreat()
        {
            var engine = WorldComponent_GravshipCombat.GetActiveGravEngine;
            if (engine == null) return;
            var activeThreatDef = WorldComponent_GravshipCombat.Instance.activeThreatDef;

            var warplatform = (MapParent_WarPlatform)WorldObjectMaker.MakeWorldObject(activeThreatDef.worldObjectDef);
            warplatform.threatDef = activeThreatDef;

            if (!Find.WorldGrid.TryGetFirstAdjacentLayerOfDef(engine.Tile, PlanetLayerDefOf.Orbit, out var orbitLayer))
            {
                return;
            }
            var orbitTile = engine.Tile.LayerDef == PlanetLayerDefOf.Orbit ? engine.Tile : orbitLayer.GetClosestTile_NewTemp(engine.Tile);
            var validTiles = new List<PlanetTile>();
            foreach (var tile in orbitLayer.Tiles)
            {
                var distance = DistanceUtil.GetDistanceInOrbitTiles(tile.tile, orbitTile);
                if (distance < def.escapeDistance && def.tileSpawnDistanceRange.Includes(distance))
                {
                    if (!def.tileSpawnDistanceRange.Includes(GravshipHelper.GetDistance(tile.tile, engine.Tile)))
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
                SpawnWarplatform(engine, warplatform, result);
            }
            else
            {
                Log.Error("Failed to create warplatform due to finding no good orbital tile.");
            }
        }

        private void SpawnWarplatform(Building_GravEngine engine, MapParent_WarPlatform warplatform, PlanetTile tile)
        {
            warplatform.Tile = tile;
            warplatform.SetFaction(EnemyFaction);
            Find.WorldObjects.Add(warplatform);
            LongEventHandler.QueueLongEvent(delegate
            {
                MapGenerator.GenerateMap(new IntVec3(def.mapSize, 1, def.mapSize), warplatform, warplatform.MapGeneratorDef);
                Find.LetterStack.ReceiveLetter(def.arrivalLetterLabel, def.arrivalLetterDesc.Formatted(engine.RenamableLabel), LetterDefOf.ThreatBig, new LookTargets(warplatform));
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

        public virtual void OnEngineDestroyed(MapParent_WarPlatform warplatform)
        {
            Find.LetterStack.ReceiveLetter(def.disengagesLetter, def.disengagesLetterDesc, LetterDefOf.NegativeEvent);
        }
    }
}
