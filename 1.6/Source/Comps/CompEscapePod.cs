using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded2;

public class CompEscapePod : ThingComp, IThingHolder, ISearchableContents
{
    protected int triggerOnTick = -999999;
    protected ThingOwner<Pawn> heldPawn;
    protected bool autoRebuild;

    [Unsaved]
    private Effecter progressBarEffecter;

    public ThingOwner SearchableContents => heldPawn;

    public Pawn Occupant => heldPawn.Any ? heldPawn[0] : null;

    private bool CanSetAutoRebuild => parent.Faction == Faction.OfPlayer && parent.def.blueprintDef != null && parent.def.IsResearchFinished;

    public CompProperties_EscapePod Props => (CompProperties_EscapePod)props;

    public CompEscapePod()
    {
        heldPawn = new ThingOwner<Pawn>(this);
    }

    public override void CompTickInterval(int delta)
    {
        base.CompTickInterval(delta);

        if (heldPawn.Any && parent.Spawned)
        {
            if (Find.TickManager.TicksGame >= triggerOnTick)
            {
                Trigger();

                if (progressBarEffecter != null)
                {
                    progressBarEffecter.Cleanup();
                    progressBarEffecter = null;
                }
            }
            else
            {
                progressBarEffecter ??= EffecterDefOf.ProgressBar.Spawn();
                progressBarEffecter.EffectTick(parent, TargetInfo.Invalid);
                var mote = ((SubEffecter_ProgressBar)progressBarEffecter.children[0]).mote;
                mote.progress = 1f - (float)(triggerOnTick - Find.TickManager.TicksGame) / Props.launchDuration;
                mote.offsetZ = -0.8f;
            }
        }
        else if (progressBarEffecter != null)
        {
            progressBarEffecter.Cleanup();
            progressBarEffecter = null;
        }
    }

    public void GetChildHolders(List<IThingHolder> outChildren) => ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());

    public ThingOwner GetDirectlyHeldThings() => heldPawn;

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (var gizmo in base.CompGetGizmosExtra())
            yield return gizmo;

        if (CanSetAutoRebuild)
        {
            yield return new Command_Toggle
            {
                defaultLabel = "CommandAutoRebuild_Building".Translate(),
                defaultDesc = "VGE_CommandAutoRebuild_EscapePod".Translate(parent.Named("BUILDING")),
                hotKey = KeyBindingDefOf.Misc3,
                icon = Props.AutoRebuildGizmo,
                isActive = () => autoRebuild,
                toggleAction = () => autoRebuild = !autoRebuild,
            };
        }
    }

    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
    {
        foreach (var option in base.CompFloatMenuOptions(selPawn))
            yield return option;

        var map = parent.Map;
        if (map?.Tile.Layer == null)
            yield break;

        var targetTile = FindClosestValidTile();
        var enter = new FloatMenuOption("VGE_EscapePod_Enter".Translate(), () => selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(InternalDefOf.VGE_EscapePod_Enter, parent)));
        var insertPawn = new FloatMenuOption("VGE_EscapePod_InsertPawn".Translate(), () =>
        {
            Find.Targeter.BeginTargeting(Props.insertPawnTargetingParameters, target =>
            {
                if (target.Thing == null)
                    return;
                // If targeting a thing, only allow player faction things or factionless things (no hostile things)
                if (target.Thing.Faction != null && target.Thing.Faction != Faction.OfPlayer)
	                return;
                // Don't target pawns from other factions, unless prisoner or slave
                if (target.Pawn != null && target.Pawn.Faction != Faction.OfPlayer && !target.Pawn.IsPrisonerOfColony && !target.Pawn.IsSlaveOfColony)
	                return;
                if (selPawn == target.Pawn)
                    selPawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(InternalDefOf.VGE_EscapePod_Enter, parent));
                else
                {
                    var job = JobMaker.MakeJob(InternalDefOf.VGE_EscapePod_InsertPawn, parent, target);
                    job.count = 1;
                    selPawn.jobs.TryTakeOrderedJob(job);
                }
            });
        });
        FloatMenuOption insertCarriedPawn;
        if (selPawn.carryTracker?.CarriedThing is Pawn carriedPawn)
        {
            insertCarriedPawn = new FloatMenuOption("VGE_EscapePod_InsertCarriedPawn".Translate(carriedPawn.Named("PAWN")), () =>
            {
                var job = JobMaker.MakeJob(InternalDefOf.VGE_EscapePod_InsertPawnDrafted, parent, carriedPawn);
                job.count = 1;
                selPawn.jobs.TryTakeOrderedJob(job);
            });
        }
        else insertCarriedPawn = null;

        if (IsFloatMenuEnabled(selPawn, targetTile) is { Accepted: false } reason)
        {
            enter.Disabled = true;
            enter.Label += $": {reason.Reason}";

            insertPawn.Disabled = true;
            insertPawn.Label += $": {reason.Reason}";

            if (insertCarriedPawn != null)
            {
                insertCarriedPawn.Disabled = true;
                insertCarriedPawn.Label += $": {reason.Reason}";
            }
        }
        else
        {
            enter = FloatMenuUtility.DecoratePrioritizedTask(enter, selPawn, parent);
            insertPawn = FloatMenuUtility.DecoratePrioritizedTask(insertPawn, selPawn, parent);
            if (insertCarriedPawn != null)
                insertCarriedPawn = FloatMenuUtility.DecoratePrioritizedTask(insertCarriedPawn, selPawn, parent);
        }

        yield return enter;
        yield return insertPawn;
        if (insertCarriedPawn != null)
            yield return insertCarriedPawn;
    }

    protected virtual AcceptanceReport IsFloatMenuEnabled(Pawn pawn, PlanetTile tile)
    {
        if (GetDirectlyHeldThings().Any)
            return "VGE_EscapePod_Occupied".Translate();

        if (!tile.Valid)
            return "CannotPerformPlanetLayer".Translate(parent.Map.Tile.LayerDef.gerundLabel.Named("GERUND"), parent.Map.Tile.LayerDef.label.Named("LAYER"));
        
        if (!pawn.CanReach(parent, PathEndMode.OnCell, Danger.Deadly))
            return "NoPath".Translate().CapitalizeFirst();

        return true;
    }

    protected virtual void Trigger()
    {
        var map = parent.Map;
        var pos = parent.Position;

        var tile = FindClosestValidTile();
        if (!tile.Valid)
        {
            heldPawn.TryDropAll(pos, map, ThingPlaceMode.Near);
            return;
        }

        var occupant = Occupant;

        var directlyHeldThings = GetDirectlyHeldThings();
        var activeTransporter = (ActiveTransporter)ThingMaker.MakeThing(Props.activeTransporterDef ?? ThingDefOf.ActiveDropPod);
        activeTransporter.Contents = new ActiveTransporterInfo();
        activeTransporter.Contents.innerContainer.TryAddRangeOrTransfer(directlyHeldThings, true, true);
        activeTransporter.Contents.sentTransporterDef = parent.def;
        activeTransporter.Rotation = parent.Rotation;
        var flyShipLeaving = (FlyShipLeaving)SkyfallerMaker.MakeSkyfaller(Props.skyfallerLeaving ?? ThingDefOf.DropPodLeaving, activeTransporter);
        flyShipLeaving.groupID = Find.UniqueIDsManager.GetNextTransporterGroupID();
        flyShipLeaving.destinationTile = tile;

        var mapParent = Find.WorldObjects.MapParentAt(tile);
        if (TransportersArrivalAction_LandInSpecificCell.CanLandInSpecificCell([this], mapParent))
        {
            if (!DropCellFinder.FindSafeLandingSpot(out var landingSpot, occupant.Faction, mapParent.Map, 25, 5, 5))
                landingSpot = DropCellFinder.RandomDropSpot(mapParent.Map);

            if (landingSpot.IsValid)
                flyShipLeaving.arrivalAction = new TransportersArrivalAction_LandInSpecificCell(mapParent, landingSpot);
            else
                flyShipLeaving.arrivalAction = new TransportersArrivalAction_FormCaravan();
        }
        else
            flyShipLeaving.arrivalAction = new TransportersArrivalAction_FormCaravan();

        flyShipLeaving.worldObjectDef = Props.worldObjectDef ?? WorldObjectDefOf.TravellingTransporters;
        parent.Destroy();
        GenSpawn.Spawn(flyShipLeaving, pos, map);
        CheckAutoRebuild(map);
    }

    public virtual void Enter(Pawn pawn)
    {
        triggerOnTick = Find.TickManager.TicksGame + Props.launchDuration;

        var selected = pawn.DeSpawnOrDeselect();
        if (pawn.holdingOwner != null)
            pawn.holdingOwner.TryTransferToContainer(pawn, heldPawn);
        else
            heldPawn.TryAdd(pawn);

        if (selected)
            Find.Selector.Select(pawn);
    }

    protected virtual PlanetTile FindClosestValidTile()
    {
        var map = parent.Map;
        if (map?.Tile.Layer?.Def == null)
            return PlanetTile.Invalid;
        if (Props.layerWhitelist == null)
            return PlanetTile.Invalid;
        if (!Props.layerWhitelist.Contains(map.Tile.LayerDef))
            return PlanetTile.Invalid;

        TileFinder.TryFindPassableTileWithTraversalDistance(Find.WorldGrid.Surface.GetClosestTile_NewTemp(map.Tile), 0, int.MaxValue, out var closest, IsPlanetTileValid, true, TileFinderMode.Near, true, true);
        return closest;
    }

    protected virtual bool IsPlanetTileValid(PlanetTile tile) => true;

    public override void PostSwapMap()
    {
        base.PostSwapMap();
        heldPawn.TryDropAll(parent.Position, parent.Map, ThingPlaceMode.Near);
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (!respawningAfterLoad && !parent.BeingTransportedOnGravship)
            autoRebuild = CanSetAutoRebuild && parent.Map.areaManager.Home[parent.Position];
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        base.PostDeSpawn(map, mode);

        if (mode != DestroyMode.WillReplace)
            heldPawn.TryDropAll(parent.Position, map, ThingPlaceMode.Near);

        if (progressBarEffecter != null)
        {
            progressBarEffecter.Cleanup();
            progressBarEffecter = null;
        }
    }

    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_Deep.Look(ref heldPawn, nameof(heldPawn), this);
        Scribe_Values.Look(ref triggerOnTick, nameof(triggerOnTick));
    }

    protected virtual void CheckAutoRebuild(Map map)
    {
        if (autoRebuild && CanSetAutoRebuild && map != null && GenConstruct.CanPlaceBlueprintAt(parent.def, parent.Position, parent.Rotation, map, stuffDef: parent.Stuff))
            GenConstruct.PlaceBlueprintForBuild(parent.def, parent.Position, map, parent.Rotation, Faction.OfPlayer, parent.Stuff);
    }
}