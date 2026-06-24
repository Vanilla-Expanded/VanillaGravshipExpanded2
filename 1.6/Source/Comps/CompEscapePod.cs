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
                defaultDesc = "CommandAutoRebuild_Building".Translate(),
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
    
	private static bool CanTarget(TargetingParameters parms, TargetInfo targ, ITargetingSource source = null)
	{
		var shouldLog = targ.Thing is Pawn { IsGhoul: true };

		if (parms.validator != null && !parms.validator(targ))
		{
			if (shouldLog)
				Log.Error($"validator");
			return false;
		}
		if (targ.Thing == null)
		{
			if (shouldLog)
				Log.Error($"no thing");
			return parms.canTargetLocations;
		}
		if (parms.neverTargetDoors && targ.Thing.def.IsDoor)
		{
			if (shouldLog)
				Log.Error($"never target doors");
			return false;
		}
		if (parms.onlyTargetDamagedThings && targ.Thing.HitPoints == targ.Thing.MaxHitPoints)
		{
			if (shouldLog)
				Log.Error($"damaged");
			return false;
		}
		if (parms.onlyTargetFlammables && !targ.Thing.FlammableNow)
		{
			if (shouldLog)
				Log.Error($"flammable");
			return false;
		}
		if (parms.mustBeSelectable && !ThingSelectionUtility.SelectableByMapClick(targ.Thing))
		{
			if (shouldLog)
				Log.Error($"selectable");
			return false;
		}
		if (parms.onlyTargetColonistsOrPrisoners && targ.Thing.def.category != ThingCategory.Pawn)
		{
			if (shouldLog)
				Log.Error($"colonist or prisoner");
			return false;
		}
		if (parms.onlyTargetColonistsOrPrisonersOrSlaves && targ.Thing.def.category != ThingCategory.Pawn)
		{
			if (shouldLog)
				Log.Error($"colonist or prisoner or slave");
			return false;
		}
		if (parms.onlyTargetDoors && !targ.Thing.def.IsDoor)
		{
			if (shouldLog)
				Log.Error($"only doors");
			return false;
		}
		Corpse corpse;
		if ((corpse = targ.Thing as Corpse) == null)
		{
			corpse = (targ.Thing is Pawn pawn) ? pawn.Corpse : null;
		}
		Corpse corpse2 = corpse;
		if (parms.canTargetCorpses && corpse2 != null)
		{
			if (shouldLog)
				Log.Error($"Corpse");
			return (parms.canTargetMechs || !corpse2.InnerPawn.RaceProps.IsMechanoid) && (parms.canTargetAnimals || !corpse2.InnerPawn.RaceProps.Animal) && (parms.canTargetHumans || !corpse2.InnerPawn.RaceProps.Humanlike) && (parms.canTargetSubhumans || !corpse2.InnerPawn.IsSubhuman);
		}
		if (parms.onlyTargetCorpses)
		{
			if (shouldLog)
				Log.Error($"Only corpse");
			return false;
		}
		if (parms.targetSpecificThing != null && targ.Thing == parms.targetSpecificThing)
		{
			if (shouldLog)
				Log.Error($"Specific thing");
			return true;
		}
		if (parms.canTargetFires && targ.Thing.def == ThingDefOf.Fire)
		{
			
			if (shouldLog)
				Log.Error($"Fire");
			return true;
		}
		if (parms.canTargetPawns && targ.Thing.def.category == ThingCategory.Pawn)
		{
			Pawn pawn2 = (Pawn)targ.Thing;
			if (pawn2.Downed)
			{
				if (parms.neverTargetIncapacitated)
				{
					if (shouldLog)
						Log.Error($"Not downed");
					return false;
				}
			}
			else if (parms.onlyTargetIncapacitatedPawns)
			{
				
				if (shouldLog)
					Log.Error($"Downed");
				return false;
			}
			if (parms.onlyTargetFactions != null && !parms.onlyTargetFactions.Contains(targ.Thing.Faction))
			{
				
				if (shouldLog)
					Log.Error($"Wrong faction");
				return false;
			}
			if (pawn2.NonHumanlikeOrWildMan())
			{
				if (pawn2.Faction != null && pawn2.RaceProps.IsMechanoid)
				{
					if (!parms.canTargetMechs)
					{
						
						if (shouldLog)
							Log.Error($"Wild mech");
						return false;
					}
					if (parms.onlyRepairableMechs && !MechRepairUtility.CanRepair(pawn2))
					{
						if (shouldLog)
							Log.Error($"Wild mech repair");
						return false;
					}
				}
				else if (!parms.canTargetAnimals)
				{
					if (shouldLog)
						Log.Error($"No animals");
					return false;
				}
			}
			if (!pawn2.NonHumanlikeOrWildMan() && !parms.canTargetHumans)
			{
				
				if (shouldLog)
					Log.Error($"Wildman");
				return false;
			}
			if (!parms.canTargetEntities && pawn2.IsEntity)
			{
				
				if (shouldLog)
					Log.Error($"Entity");
				return false;
			}
			if (!parms.canTargetSubhumans && pawn2.IsSubhuman)
			{
				
				if (shouldLog)
					Log.Error($"Subbhuman");
				return false;
			}
			if (parms.onlyTargetControlledPawns && !pawn2.IsColonistPlayerControlled)
			{
				
				if (shouldLog)
					Log.Error($"Not controlled");
				return false;
			}
			if (parms.onlyTargetColonists && (!pawn2.IsColonist || pawn2.HostFaction != null))
			{
				if (shouldLog)
					Log.Error($"Not colonist");
				return false;
			}
			if (parms.onlyTargetPrisonersOfColony && !pawn2.IsPrisonerOfColony)
			{
				if (shouldLog)
					Log.Error($"Not prisoner");
				return false;
			}
			if (parms.onlyTargetColonistsOrPrisoners && !pawn2.IsColonistPlayerControlled && !pawn2.IsPrisonerOfColony)
			{
				
				if (shouldLog)
					Log.Error($"Not colonist or prisoner");
				return false;
			}
			if (parms.onlyTargetColonistsOrPrisonersOrSlaves && !pawn2.IsColonistPlayerControlled && !pawn2.IsPrisonerOfColony && !pawn2.IsSlaveOfColony)
			{
				
				if (shouldLog)
					Log.Error($"Not colonist prisoner or slave");
				return false;
			}
			if (parms.onlyTargetColonistsOrPrisonersOrSlavesAllowMinorMentalBreaks)
			{
				if (!pawn2.IsPrisonerOfColony && !pawn2.IsSlaveOfColony && (!pawn2.IsColonist || (pawn2.HostFaction != null && !pawn2.IsSlave)))
				{
					return false;
				}
				MentalStateDef mentalStateDef = pawn2.MentalStateDef;
				if (mentalStateDef != null && mentalStateDef.IsAggro)
				{
					return false;
				}
			}
			if (parms.onlyTargetPsychicSensitive && pawn2.GetStatValue(StatDefOf.PsychicSensitivity, true, -1) <= 0f)
			{
				return false;
			}
			if (parms.neverTargetHostileFaction && !pawn2.IsPrisonerOfColony && !pawn2.IsSlaveOfColony)
			{
				Faction homeFaction = pawn2.HomeFaction;
				if (homeFaction != null && homeFaction.HostileTo(Faction.OfPlayer))
				{
					return false;
				}
			}
			if (parms.onlyTargetSameIdeo)
			{
				if (source == null)
				{
					Log.Error("Source passed in is null but targeting parameters have onlyTargetSameIdeo set.");
				}
				else
				{
					Verb verb = source as Verb;
					if (verb != null && verb.CasterPawn != null)
					{
						Pawn pawn3 = targ.Thing as Pawn;
						Ideo ideo = ((pawn3 != null) ? pawn3.Ideo : null);
						if (verb.CasterPawn.Ideo != ideo)
						{
							return false;
						}
					}
					else
					{
						Log.Error("Source passed in is incompatible type but targeting parameters have onlyTargetSameIdeo set.");
					}
				}
			}
			return parms.canTargetBloodfeeders || !ModsConfig.BiotechActive || !pawn2.IsBloodfeeder();
		}
		else
		{
			if (parms.canTargetBuildings && targ.Thing.def.category == ThingCategory.Building)
			{
				return (!parms.mapObjectTargetsMustBeAutoAttackable || targ.Thing.def.building.isTargetable) && (!parms.onlyTargetThingsAffectingRegions || targ.Thing.def.AffectsRegions) && (parms.onlyTargetFactions == null || parms.onlyTargetFactions.Contains(targ.Thing.Faction));
			}
			if (parms.canTargetPlants && targ.Thing.def.category == ThingCategory.Plant)
			{
				return !ModsConfig.RoyaltyActive || !parms.onlyTargetAnimaTrees || targ.Thing.def == ThingDefOf.Plant_TreeAnima;
			}
			if (parms.canTargetItems)
			{
				if (parms.mapObjectTargetsMustBeAutoAttackable && !targ.Thing.def.isAutoAttackableMapObject)
				{
					return false;
				}
				if (parms.thingCategory == ThingCategory.None || parms.thingCategory == targ.Thing.def.category)
				{
					return true;
				}
			}
			return false;
		}
	}
}