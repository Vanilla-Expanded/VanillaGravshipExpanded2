using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded2;

public class JobDriver_EscapePod_InsertPawn : JobDriver
{
    public const TargetIndex EscapePodIndex = TargetIndex.A;
    public const TargetIndex CarriedPawnIndex = TargetIndex.B;

    private CompEscapePod EscapePod => job.GetTarget(EscapePodIndex).Thing.TryGetComp<CompEscapePod>();

    private Pawn CarriedPawn => job.GetTarget(CarriedPawnIndex).Pawn;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
        => pawn != job.GetTarget(CarriedPawnIndex) &&
           pawn.Reserve(job.GetTarget(EscapePodIndex), job, errorOnFailed: errorOnFailed) &&
           pawn.Reserve(job.GetTarget(CarriedPawnIndex), job, errorOnFailed: errorOnFailed);

    public override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(EscapePodIndex);
        this.FailOn(() => CarriedPawn.Dead);
        this.FailOnCannotReach(EscapePodIndex, PathEndMode.OnCell);
        this.FailOn(() => EscapePod is not { } pod || pod.GetDirectlyHeldThings().Any);
        this.FailOnAggroMentalState(CarriedPawnIndex);

        var gotoPawn = Toils_Goto.GotoThing(CarriedPawnIndex, PathEndMode.OnCell)
            .FailOnDespawnedNullOrForbidden(CarriedPawnIndex)
            .FailOnCannotReach(CarriedPawnIndex, PathEndMode.ClosestTouch)
            .FailOnSomeonePhysicallyInteracting(CarriedPawnIndex);
        var carryPawn = Toils_Haul.StartCarryThing(CarriedPawnIndex);
        var gotoPod = Toils_Goto.GotoThing(EscapePodIndex, PathEndMode.OnCell);

        // Skip over goto pawn and carry pawn if already carrying the pawn
        yield return Toils_Jump.JumpIf(gotoPod, () => pawn.IsCarryingPawn(CarriedPawn));

        // Goto and carry the pawn
        yield return gotoPawn;
        yield return carryPawn;
        // Goto the escape pod
        yield return gotoPod;

        // Wait while inserting the pawn
        var waitToil = Toils_General.Wait(EscapePod.Props.enterDuration, EscapePodIndex);
        waitToil.FailOnCannotTouch(EscapePodIndex, PathEndMode.OnCell);
        waitToil.WithProgressBarToilDelay(EscapePodIndex);
        waitToil.PlaySustainerOrSound(() => EscapePod.Props.enterSound);
        yield return waitToil;

        // Actually insert the pawn
        var insertPawnToil = ToilMaker.MakeToil();
        insertPawnToil.initAction = () => EscapePod.Enter(CarriedPawn);
        insertPawnToil.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return insertPawnToil;
    }
}