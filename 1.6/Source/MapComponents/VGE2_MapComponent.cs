using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded2;

public class VGE2_MapComponent : MapComponent
{
    private static readonly HashSet<JobDef> JobDefsWithPriorityOverEvacuation;

    private bool evacuationActive = false;
    public readonly HashSet<CompEscapePod> activeEscapePods = [];
    public readonly HashSet<CompPower_InputOnlyBattery> batteries = [];

    public bool EvacuationActive
    {
        get => evacuationActive;
        set
        {
            if (evacuationActive == value)
                return;

            evacuationActive = value;
            if (value)
                StartEvacuation(true);
            else
                StopEvacuation();
        }
    }

    static VGE2_MapComponent()
    {
        JobDefsWithPriorityOverEvacuation =
        [
            InternalDefOf.VGE_EscapePod_Enter,
            InternalDefOf.VGE_EscapePod_InsertPawn,
            InternalDefOf.VGE_EscapePod_InsertPawnDrafted,
        ];
    }

    public VGE2_MapComponent(Map map) : base(map)
    {
    }

    public override void MapComponentTick()
    {
        base.MapComponentTick();

        if (EvacuationActive)
        {
            // Stop evacuation if there's no escape pods, or they're not player faction escape pods
            if (activeEscapePods.Count == 0 || activeEscapePods.All(pod => pod.parent.Faction is { IsPlayer: false }))
                EvacuationActive = false;
            else if (map.IsHashIntervalTick(GenTicks.TickLongInterval))
                StartEvacuation(false);
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref evacuationActive, nameof(evacuationActive));
    }

    private void StartEvacuation(bool evacuateDrafted)
    {
        foreach (var pawn in map.mapPawns.AllHumanlike)
        {
            // All colonists, slaves have lower priority
            if (pawn.Faction == Faction.OfPlayer && !pawn.IsSlaveOfColony && !TryEvacuate(pawn, evacuateDrafted))
                return;
        }

        if (ModsConfig.IdeologyActive)
        {
            foreach (var pawn in map.mapPawns.AllHumanlike)
            {
                if (pawn.IsSlaveOfColony && !TryEvacuate(pawn, evacuateDrafted))
                    return;
            }
        }
    }

    private void StopEvacuation()
    {
        foreach (var pawn in map.mapPawns.AllHumanlike)
        {
            if (pawn.jobs != null && pawn.RaceProps.Humanlike && pawn.Faction == Faction.OfPlayer)
            {
                pawn.jobs.jobQueue.RemoveAll(pawn, job => job?.def == InternalDefOf.VGE_EscapePod_Enter);
                if (pawn.jobs.curJob?.def == InternalDefOf.VGE_EscapePod_Enter)
                    pawn.jobs.EndCurrentJob(JobCondition.Incompletable);
            }
        }
    }

    // Return true if there's potentially more pods, false if there's none.
    private bool TryEvacuate(Pawn pawn, bool evacuateDrafted)
    {
        if (!evacuateDrafted && pawn.Drafted)
            return true;
        // Basic checks
        if (pawn.jobs == null || !pawn.RaceProps.Humanlike || pawn.Downed)
            return true;
        // Check if evacuation is already started or queued
        if (pawn.CurJob?.def != null && JobDefsWithPriorityOverEvacuation.Contains(pawn.CurJobDef))
            return true;
        if (pawn.jobs.jobQueue.jobs.Any(x => x.job?.def != null && JobDefsWithPriorityOverEvacuation.Contains(x.job.def)))
            return true;

        var anyValidPod = false;
        // Pick the closest pod the pawn can reach
        foreach (var selectedPod in activeEscapePods.Where(pod => pod.parent.Faction == null || pod.parent.Faction.IsPlayer).OrderBy(pod => pod.parent.Position.DistanceToSquared(pawn.Position)))
        {
            anyValidPod = true;
            if (map.reservationManager.CanReserve(pawn, selectedPod.parent) && pawn.CanReach(selectedPod.parent, PathEndMode.OnCell, Danger.Deadly))
            {
                selectedPod.ClaimIfNeeded();
                if (pawn.Drafted)
                    pawn.drafter.Drafted = false;
                pawn.jobs.StartJob(JobMaker.MakeJob(InternalDefOf.VGE_EscapePod_Enter, selectedPod.parent), JobCondition.InterruptForced, resumeCurJobAfterwards: true);
                break;
            }
        }

        return anyValidPod;
    }
}