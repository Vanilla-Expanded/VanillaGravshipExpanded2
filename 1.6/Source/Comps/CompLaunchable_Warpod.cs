using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class CompLaunchable_Warpod : CompLaunchable
    {
        private static List<CompLaunchable_Warpod> selectedWarpodsToLaunch;

        public CompRefuelable FuelingPortSource => FuelingPortUtility.FuelingPortGiverAtFuelingPortCell(parent.Position, parent.Map).GetComp<CompRefuelable>();
        public bool ConnectedToFuelingPort => FuelingPortUtility.FuelingPortGiverAtFuelingPortCell(parent.Position, parent.Map) is Building;
        public override CompRefuelable Refuelable => FuelingPortSource;
        public override float FuelLevel => FuelingPortSource.Fuel;
        public override float MaxFuelLevel => FuelingPortSource.Props.fuelCapacity;
        public override bool RequiresFuelingPort => true;
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            var command = new Command_Action
            {
                defaultLabel = "VGE_LaunchWarpod".Translate(),
                defaultDesc = "VGE_LaunchWarpodDesc".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Gizmo/LaunchWarpods"),
                action = TryStartChoosingWarpodDestination
            };

            var canLaunch = CanLaunch();
            if (!canLaunch.Accepted)
            {
                command.Disable(canLaunch.Reason);
            }
            yield return command;
        }

        public override AcceptanceReport CanLaunch(float? overrideFuelLevel = null)
        {
            if (!ConnectedToFuelingPort)
                return "CommandLaunchGroupFailNotConnectedToFuelingPort".Translate();

            if (Transporter.Props.massCapacity > 0)
            {
                if (!Transporter.LoadingInProgressOrReadyToLaunch)
                    return "CommandLaunchGroupFailNotLoaded".Translate();
                if (Transporter.OverMassCapacity)
                    return "CommandLaunchGroupFailOverMassCapacity".Translate();
            }
            if (FuelLevel < Props.minFuelCost)
                return "CommandLaunchGroupFailNoFuel".Translate();

            return true;
        }

        private void ResetCachedTile()
        {
            cachedClosest = PlanetTile.Invalid;
            cachedOrigin = PlanetTile.Invalid;
            cachedLayer = null;
        }

        private void TryStartChoosingWarpodDestination()
        {
            if (Transporter.Props.massCapacity > 0 && Transporter.AnyInGroupHasAnythingLeftToLoad)
            {
                var text = "ConfirmSendNotCompletelyLoadedLaunchable".Translate() + ":\n";
                text += Transporter.leftToLoad.Select((TransferableOneWay x) => x.AnyThing.LabelCap).ToLineList(" -");
                text += "\n\n" + "ConfirmSendLaunchAnyway".Translate();
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(text, StartChoosingWarpodDestination, true));
            }
            else
            {
                StartChoosingWarpodDestination();
            }
        }

        private void StartChoosingWarpodDestination()
        {
            ResetCachedTile();
            selectedWarpodsToLaunch = Find.Selector.SelectedObjects
                .OfType<Thing>()
                .Select(t => t.TryGetComp<CompLaunchable_Warpod>())
                .Where(c => c != null && c.CanLaunch().Accepted)
                .ToList();
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(new GlobalTargetInfo(parent.Map.Tile)));
            Find.WorldSelector.ClearSelection();
            Find.WorldTargeter.BeginTargeting(ChoseWorldTarget, true, null, false, () =>
            {
                var tile = parent.Map.Tile;
                PlanetTile planetTile;
                if (cachedLayer != Find.WorldSelector.SelectedLayer || cachedOrigin != tile)
                {
                    cachedLayer = Find.WorldSelector.SelectedLayer;
                    cachedOrigin = tile;
                    planetTile = cachedClosest = Find.WorldSelector.SelectedLayer.GetClosestTile_NewTemp(tile);
                }
                else
                {
                    planetTile = cachedClosest;
                }
                var maxLaunchDistance = MaxLaunchDistanceAtFuelLevel(FuelLevel, PlanetLayer.Selected);
                GenDraw.DrawWorldRadiusRing(planetTile, maxLaunchDistance, CompPilotConsole.GetFuelRadiusMat(planetTile));
            });
        }

        private bool ChoseWorldTarget(GlobalTargetInfo target)
        {
            if (!target.IsValid) return false;

            var dist = Find.WorldGrid.TraversalDistanceBetween(parent.Map.Tile, target.Tile, true, int.MaxValue, true);
            if (dist > MaxLaunchDistanceAtFuelLevel(FuelLevel, target.Tile.Layer))
            {
                Messages.Message("TransportPodDestinationBeyondMaximumRange".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }

            if (!(target.WorldObject is MapParent mp) || !mp.HasMap)
            {
                Messages.Message("VGE_WarpodLocationNeedRevealed".Translate(), MessageTypeDefOf.RejectInput, false);
                return false;
            }
            CameraJumper.TryJump(mp.Map.Center, mp.Map);

            var explosiveProps = parent.def.GetCompProperties<CompProperties_Explosive>();
            var radius = explosiveProps.explosiveRadius;
            var sourceMap = parent.Map;
            var sourcePos = parent.Position;

            Find.Targeter.BeginTargeting(new TargetingParameters
            {
                canTargetLocations = true,
                canTargetBuildings = true,
                canTargetPawns = true
            }, (LocalTargetInfo localTarget) =>
            {
                LaunchWarpodTo(mp.Map, mp.Tile, localTarget.Cell);
            }, (LocalTargetInfo targetCell) =>
            {
                GenDraw.DrawTargetHighlight(targetCell);
                if (radius > 0f)
                {
                    GenDraw.DrawRadiusRing(targetCell.Cell, radius);
                }
            }, null, null, () =>
            {
                CameraJumper.TryJump(new GlobalTargetInfo(sourcePos, sourceMap));
            }, null, true, null, null);

            return true;
        }

        private void LaunchWarpodTo(Map destMap, PlanetTile destTile, IntVec3 destCell)
        {
            var finalCell = destCell;
            if (Designator_MoveGravship_IsValidCell_Patch.IsCellJammed(finalCell, destMap))
            {
                Messages.Message("VGE_DestinationJammedWarpod".Translate(), MessageTypeDefOf.RejectInput, false);
                return;
            }
            foreach (var warpod in selectedWarpodsToLaunch)
            {
                var traversalDistance = Find.WorldGrid.TraversalDistanceBetween(warpod.parent.Map.Tile, destTile, true, int.MaxValue, true);
                var amount = Mathf.Max(warpod.FuelNeededToLaunchAtDist(traversalDistance, destMap.TileInfo.Layer), 5f);
                warpod.Refuelable.ConsumeFuel(amount);

                var activeTransporter = (ActiveTransporter)ThingMaker.MakeThing(ThingDefOf.ActiveDropPod);
                activeTransporter.Contents = new ActiveTransporterInfo();
                activeTransporter.Contents.innerContainer.TryAddRangeOrTransfer(warpod.Transporter.GetDirectlyHeldThings(), true, true);
                activeTransporter.Contents.sentTransporterDef = warpod.parent.def;
                activeTransporter.Rotation = warpod.parent.Rotation;

                var leaving = (WarpodLeaving)SkyfallerMaker.MakeSkyfaller(InternalDefOf.VGE_WarpodLeaving, activeTransporter);
                leaving.destinationTile = destTile;
                leaving.worldObjectDef = InternalDefOf.VGE_TravellingWarpod;
                leaving.arrivalAction = new TransportersArrivalAction_WarpodStrike(destMap.Parent, finalCell, warpod.parent.Faction);

                var sourceMap = warpod.parent.Map;
                var sourcePos = warpod.parent.Position;
                warpod.Transporter.CleanUpLoadingVars(sourceMap);
                warpod.parent.Destroy();
                GenSpawn.Spawn(leaving, sourcePos, sourceMap);
            }
        }
    }
}
