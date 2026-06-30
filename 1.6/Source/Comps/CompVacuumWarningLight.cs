using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded2;

[StaticConstructorOnStartup]
public class CompVacuumWarningLight : ThingComp, IThingGlower
{
    private static readonly Texture2D VacuumResistanceGizmo = ContentFinder<Texture2D>.Get("UI/Gizmos/SetDesiredVacuumResistance");
    private static readonly HashSet<Pawn> TempPawnsWorkingList = [];

    [Unsaved] private float vacuumLevel;
    private float concerningVacuumLevel;
    private bool evacuatePawns;
    [Unsaved] private CompGlower glower;

    public float VacuumLevel
    {
        get => vacuumLevel;
        private set
        {
            var wasConcern = IsVacuumConcern;
            vacuumLevel = value;
            if (wasConcern != IsVacuumConcern && parent.Spawned)
                glower?.UpdateLit(parent.Map);
        }
    }

    public float ConcerningVacuumLevel
    {
        get => concerningVacuumLevel;
        set
        {
            var val = Props.minMaxVacuumRanges.ClampToRange(value);
            if (concerningVacuumLevel == val)
                return;
            var wasConcern = IsVacuumConcern;
            concerningVacuumLevel = val;
            var isConcern = IsVacuumConcern;
            if (parent.Spawned)
            {
                if (isConcern != wasConcern)
                    glower?.UpdateLit(parent.Map);
                if (!wasConcern && isConcern)
                    ScarePawnsAway();
            }
        }
    }

    public bool EvacuatePawns
    {
        get => evacuatePawns;
        set
        {
            if (value && !evacuatePawns && IsVacuumConcern)
                ScarePawnsAway();
            evacuatePawns = value;
        }
    }

    public bool IsVacuumConcern => vacuumLevel >= ConcerningVacuumLevel;

    public CompProperties_VacuumWarningLight Props => (CompProperties_VacuumWarningLight)props;

    public bool ShouldBeLitNow() => IsVacuumConcern;

    public override void PostPostMake()
    {
        base.PostPostMake();

        concerningVacuumLevel = Props.baseVacuumLevel;
        evacuatePawns = Props.evacuatePawnsByDefault;
        InitComps();
    }

    public override void CompTickInterval(int delta)
    {
        base.CompTickInterval(delta);

        if (IsVacuumConcern && parent.IsHashIntervalTick(GenTicks.TickRareInterval, delta))
            ScarePawnsAway();
    }

    public override void CompTickRare()
    {
        base.CompTickRare();

        if (IsVacuumConcern)
            ScarePawnsAway();
    }

    public override void CompTickLong()
    {
        base.CompTickLong();

        if (IsVacuumConcern)
            ScarePawnsAway();
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);

        VacuumLevel = parent.Position.GetVacuum(parent.Map);
        if (IsVacuumConcern)
            ScarePawnsAway();
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        base.PostDeSpawn(map, mode);

        VacuumLevel = 0f;
    }

    public override void ReceiveCompSignal(string signal)
    {
        base.ReceiveCompSignal(signal);

        if (signal == Room.VacuumChangedSignal && parent.Spawned)
        {
            var wasConcern = IsVacuumConcern;
            VacuumLevel = parent.Position.GetVacuum(parent.Map);
            if (!wasConcern && IsVacuumConcern)
                ScarePawnsAway();
        }
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (var gizmo in base.CompGetGizmosExtra())
            yield return gizmo;

        if (parent.Faction == Faction.OfPlayer)
        {
            yield return new Command_Action
            {
                defaultLabel = "VGE_VacuumEmergencyLight_UnsafeVacuumLevel".Translate(),
                defaultDesc = "VGE_VacuumEmergencyLight_UnsafeVacuumLevelDesc".Translate(),
                icon = VacuumResistanceGizmo,
                action = () => Find.WindowStack.Add(new Dialog_VacuumWarningLight(Props.minMaxVacuumRanges, EvacuatePawns, !Props.visualOnly, ConcerningVacuumLevel)),
            };
        }
    }

    private void ScarePawnsAway()
    {
        if (Props.visualOnly || !EvacuatePawns)
            return;
        if (parent.Faction == null)
            return;

        var room = parent.GetRoom();
        var pos = parent.Position;

        TempPawnsWorkingList.Clear();
        if (room == null || !Props.alwaysAffectsWholeRoom || room.PsychologicallyOutdoors)
        {
            RegionTraverser.BreadthFirstTraverse(pos, parent.Map,
                (_, to) => pos.InHorDistOf(to.extentsClose.ClosestCellTo(pos), Props.radius),
                region =>
                {
                    foreach (var thing in region.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn))
                    {
                        if (thing.Position.InHorDistOf(pos, Props.radius) && thing is Pawn pawn)
                            TempPawnsWorkingList.Add(pawn);
                    }

                    // Don't stop traversing
                    return false;
                });
        }
        else
        {
            RegionTraverser.BreadthFirstTraverse(pos, parent.Map,
                (_, to) => to.Room == room,
                region =>
                {
                    foreach (var thing in region.ListerThings.ThingsInGroup(ThingRequestGroup.Pawn))
                    {
                        if (thing is Pawn pawn)
                            TempPawnsWorkingList.Add(pawn);
                    }

                    // Don't stop traversing
                    return false;
                });
        }

        foreach (var pawn in TempPawnsWorkingList)
        {
            if (!pawn.Spawned)
                continue;
            // if (pawn.Downed) // Crawl?
            //     continue;
            if (!CaresAboutLowVacuum(pawn))
                continue;
            if (pawn.CurJob?.def == JobDefOf.GotoOxygenatedArea)
                continue;

            var cell = ClosestOxygenatedCell(pawn, ConcerningVacuumLevel, TraverseParms.For(pawn));
            if (cell.IsValid)
                pawn.jobs.StartJob(JobMaker.MakeJob(JobDefOf.GotoOxygenatedArea, cell), JobCondition.InterruptForced, resumeCurJobAfterwards: true);
        }

        TempPawnsWorkingList.Clear();
    }

    private bool CaresAboutLowVacuum(Pawn pawn)
    {
        if (parent.Faction != pawn.Faction && parent.Faction != pawn.guest?.HostFaction)
            return false;
        if (pawn.Drafted)
            return false;
        if (pawn.RaceProps.IsMechanoid || pawn.RaceProps.IsDrone)
            return false;
        if (pawn.IsMutant && !pawn.mutant.Def.breathesAir)
            return false;
        if (pawn.GetStatValue(StatDefOf.VacuumResistance, cacheStaleAfterTicks: 60) >= 0.95f)
            return false;
        return true;
    }

    private static IntVec3 ClosestOxygenatedCell(Pawn pawn, float concerningVacuumLevel, TraverseParms traverseParms, RegionType traversableRegionTypes = RegionType.Set_Passable)
    {
        var region = pawn.Position.GetRegion(pawn.Map, traversableRegionTypes);
        if (region == null)
            return IntVec3.Invalid;

        var foundCell = IntVec3.Invalid;
        RegionTraverser.BreadthFirstTraverse(region,
            (_, to) => to.Allows(traverseParms, false),
            reg =>
            {
                // Keep traversing, ignore doorways
                if (reg.IsDoorway)
                    return false;
                if (reg.Room.Vacuum >= concerningVacuumLevel)
                    return false;
                if (!JobGiver_FindOxygen.TryGetAllowedCellInRegion(reg, pawn, out var cell))
                    return false;

                foundCell = cell;
                return true;
            }, traversableRegionTypes: traversableRegionTypes);
        return foundCell;
    }

    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_Values.Look(ref concerningVacuumLevel, nameof(concerningVacuumLevel), 0.25f);
        Scribe_Values.Look(ref evacuatePawns, nameof(evacuatePawns), true);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
            concerningVacuumLevel = Props.minMaxVacuumRanges.ClampToRange(concerningVacuumLevel);
        if (Scribe.mode == LoadSaveMode.LoadingVars)
            InitComps();
    }

    private void InitComps()
    {
        glower = parent.GetComp<CompGlower>();
    }
}