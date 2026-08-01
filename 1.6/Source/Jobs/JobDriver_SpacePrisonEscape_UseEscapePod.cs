using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace VanillaGravshipExpanded2;

public class JobDriver_SpacePrisonEscape_UseEscapePod : JobDriver
{
    public const TargetIndex EscapePodIndex = TargetIndex.A;

    public Thing Building => job.GetTarget(EscapePodIndex).Thing;
    public CompEscapePod EscapePod => Building.TryGetComp<CompEscapePod>();
    public CompLaunchable TransportPod => Building.TryGetComp<CompLaunchable>();

    public override bool TryMakePreToilReservations(bool errorOnFailed) => true;

    public override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(EscapePodIndex);
        this.FailOnCannotReach(EscapePodIndex, PathEndMode.OnCell);
        this.FailOn(() => pawn.GetLord()?.CurLordToil is not LordToil_GoToEscapePods);
        this.FailOn(() =>
        {
            var escapePod = EscapePod;
            if (escapePod != null)
                return escapePod.GetDirectlyHeldThings().Any;

            var transportPod = TransportPod;
            var lordJob = (LordToil_GoToEscapePods)pawn.GetLord().CurLordToil;
            return transportPod?.Transporter == null || !SpaceRebellionsUtility.IsTransportPodUsable(TransportPod, lordJob.Data.dropPodTile.Layer, lordJob.Data.cachedDropPodDistance);
        });

        yield return Toils_Goto.GotoThing(EscapePodIndex, PathEndMode.OnCell);

        var waitToil = Toils_General.Wait(EscapePod?.Props.enterDuration ?? 120, EscapePodIndex);
        waitToil.FailOnCannotTouch(EscapePodIndex, PathEndMode.OnCell);
        waitToil.WithProgressBarToilDelay(EscapePodIndex);
        waitToil.PlaySustainerOrSound(() => EscapePod?.Props.enterSound);
        yield return waitToil;

        var enterToil = ToilMaker.MakeToil();
        enterToil.initAction = () =>
        {
            var escapePod = EscapePod;
            if (escapePod != null)
            {
                escapePod.Enter(pawn);
                escapePod.isCurrentlyStolen = true;
                escapePod.parent.SetFaction(null);
                return;
            }

            // If EscapePod is null, then CompLaunchable and CompTransporter can't be (job fail condition would have triggered otherwise)
            var transportPod = TransportPod;
            var lord = pawn.GetLord();

            if (!transportPod.Transporter.GetDirectlyHeldThings().ContainsAny(x => x is Pawn otherPawn && otherPawn.GetLord() == lord))
            {
                // Cancel load
                if (transportPod.Transporter.LoadingInProgressOrReadyToLaunch)
                    transportPod.Transporter.CancelLoad();
                // Just in case something is up, eject all items and cleanup
                else
                    transportPod.Transporter.CleanUpLoadingVars(transportPod.parent.Map);
            }

            transportPod.parent.SetFaction(null);
            var building = (Building)transportPod.parent;
            if (!lord.ownedBuildings.Contains(building))
                lord.AddBuilding(building);
            var selected = pawn.DeSpawnOrDeselect();
            transportPod.Transporter.GetDirectlyHeldThings().TryAdd(pawn);
            if (selected)
                Find.Selector.Select(pawn);
        };
        enterToil.defaultCompleteMode = ToilCompleteMode.Instant;
        yield return enterToil;
    }
}