using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded2;

public class JobDriver_EscapePod_Enter : JobDriver
{
    public const TargetIndex EscapePodIndex = TargetIndex.A;

    private CompEscapePod EscapePod => job.GetTarget(EscapePodIndex).Thing.TryGetComp<CompEscapePod>();

    public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(job.GetTarget(EscapePodIndex), job, errorOnFailed: errorOnFailed);

    public override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(EscapePodIndex);
        this.FailOnCannotReach(EscapePodIndex, PathEndMode.OnCell);
        this.FailOn(() => EscapePod is not { } pod || pod.GetDirectlyHeldThings().Any);

        yield return Toils_Goto.GotoThing(EscapePodIndex, PathEndMode.OnCell);

        var waitToil = Toils_General.Wait(EscapePod.Props.enterDuration, EscapePodIndex);
        waitToil.FailOnCannotTouch(EscapePodIndex, PathEndMode.OnCell);
        waitToil.WithProgressBarToilDelay(EscapePodIndex);
        waitToil.PlaySustainerOrSound(() => EscapePod.Props.enterSound);
        yield return waitToil;

        var enterToil = ToilMaker.MakeToil();
        enterToil.initAction = () => EscapePod.Enter(pawn);
        enterToil.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return enterToil;
    }
}