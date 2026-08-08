using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace VanillaGravshipExpanded2;

public class LordToil_GoToEscapePods : LordToil
{
    protected static readonly List<Pawn> TempPawnList = [];
    protected static readonly List<Thing> TempDropPodList = [];

    public override bool AllowSatisfyLongNeeds => false;

    public override bool AllowSelfTend => false;

    public LordToilData_GoToEscapePods Data => (LordToilData_GoToEscapePods)data;

    public LordToil_GoToEscapePods(PlanetTile dropPodTile, LocomotionUrgency locomotion = LocomotionUrgency.None, bool canDig = false, bool interruptCurrentJob = false)
    {
        data = new LordToilData_GoToEscapePods
        {
            dropPodTile = dropPodTile,
            locomotion = locomotion,
            canDig = canDig,
            interruptCurrentJob = interruptCurrentJob,
        };
    }

    public override void Init()
    {
        base.Init();
        Data.cachedDropPodDistance = Find.WorldGrid.TraversalDistanceBetween(Map.Tile, Data.dropPodTile, true, int.MaxValue, true);
    }

    public override void Notify_PawnLost(Pawn victim, PawnLostCondition cond)
    {
        base.Notify_PawnLost(victim, cond);

        Data.targetsForPawns.Remove(victim);
    }

    public override void LordToilTick()
    {
        base.LordToilTick();

        if (lord.ownedBuildings.Count > 0 && lord.Map.IsHashIntervalTick(300))
        {
            for (var i = lord.ownedBuildings.Count - 1; i >= 0; i--)
                LaunchDropPodIfNeeded(lord.ownedBuildings[i]);
        }
    }

    public void LaunchDropPodIfNeeded(Building building)
    {
        var launchable = building.GetComp<CompLaunchable>();
        var heldThings = launchable?.Transporter?.GetDirectlyHeldThings();
        if (heldThings == null)
        {
            lord.Notify_BuildingLost(building);
        }
        else if (!SpaceRebellionsUtility.IsTransportPodUsable(launchable, Data.dropPodTile.Layer, Data.cachedDropPodDistance) || !heldThings.Any(x => lord.ownedPawns.Contains(x)))
        {
            lord.Notify_BuildingLost(building);
            launchable.Transporter.CleanUpLoadingVars(Map);
        }
        else if ((float)building.HitPoints / building.MaxHitPoints < 0.75f || !Data.targetsForPawns.Any(x => x.Value == building && x.Key.DestroyedOrNull() || x.Key.DeadOrDowned || !heldThings.Contains(x.Key)))
        {
            lord.Notify_BuildingLost(building);
            LaunchDropPod(launchable);
        }
    }

    public void LaunchDropPod(Building building)
    {
        var transporter = building.GetComp<CompLaunchable>();
        if (transporter == null)
            lord.Notify_BuildingLost(building);
        else
            LaunchDropPod(transporter);
    }

    public void LaunchDropPod(CompLaunchable transporter)
    {
        TransporterUtility.InitiateLoading([transporter.Transporter]);
        transporter.TryLaunch(Data.dropPodTile, new TransportersArrivalAction_DestroyOrPassToWorld());
    }

    public override void UpdateAllDuties()
    {
        TempPawnList.Clear();
        TempPawnList.AddRange(lord.ownedPawns);
        TempPawnList.RemoveAll(x => x.DestroyedOrNull() || !x.Spawned || x.Map != Map);

        if (TempPawnList.Count <= 0)
            return;

        var data = Data;
        data.targetsForPawns.RemoveAll(kvp => kvp.Key.DestroyedOrNull() || !TempPawnList.Contains(kvp.Key) || kvp.Value.DestroyedOrNull());

        SpaceRebellionsUtility.GetAllValidPods(Map, TempDropPodList, TempDropPodList);

        foreach (var (pawn, target) in data.targetsForPawns)
        {
            TempPawnList.Remove(pawn);
            TempDropPodList.Remove(target);
            AssignDuty(pawn, target, false);
        }

        if (TempPawnList.Count <= 0)
        {
            TempDropPodList.Clear();
            return;
        }

        TempDropPodList.SortBy(x => x.Position.DistanceToSquared(lord.ownedPawns[0].Position));
        Thing currentPod = null;
        var acceptsMultiplePawns = false;
        var remainingAcceptedWeight = 0f;

        for (var i = 0; i < TempPawnList.Count; i++)
        {
            var pawn = TempPawnList[i];
            AssignDuty(pawn, GrabNextPod(pawn), true);
        }

        TempDropPodList.Clear();
        TempPawnList.Clear();

        void AssignDuty(Pawn pawn, Thing target, bool updateTargets)
        {
            var pawnDuty = new PawnDuty(InternalDefOf.VGE_SpacePrisonerEscape, target)
            {
                locomotion = data.locomotion,
                canDig = data.canDig
            };
            if (updateTargets)
                data.targetsForPawns[pawn] = target;

            pawn.mindState.duty = pawnDuty;
            if (Data.interruptCurrentJob && pawn.jobs.curJob != null)
                pawn.jobs.EndCurrentJob(JobCondition.InterruptForced);
        }

        Thing GrabNextPod(Pawn pawn)
        {
            while (TempDropPodList.Count > 0)
            {
                if (currentPod == null)
                {
                    currentPod = TempDropPodList[0];
                    TempDropPodList.RemoveAt(0);

                    var escapePod = currentPod.TryGetComp<CompLaunchable>();
                    if (escapePod?.Transporter != null)
                    {
                        acceptsMultiplePawns = true;
                        remainingAcceptedWeight = escapePod.Transporter.MassCapacity;
                    }
                    else
                    {
                        acceptsMultiplePawns = false;
                    }
                }

                if (acceptsMultiplePawns)
                {
                    var pawnMass = pawn.GetStatValue(StatDefOf.Mass, cacheStaleAfterTicks: 1);
                    if (remainingAcceptedWeight >= pawnMass)
                    {
                        remainingAcceptedWeight -= pawnMass;
                        return currentPod;
                    }

                    currentPod = null;
                    remainingAcceptedWeight = 0f;
                }
                else
                {
                    var pod = currentPod;
                    currentPod = null;
                    return pod;
                }
            }

            return null;
        }
    }
}