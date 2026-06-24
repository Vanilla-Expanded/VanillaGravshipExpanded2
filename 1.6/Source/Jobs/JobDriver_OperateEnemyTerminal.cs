using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded2
{
    public class JobDriver_OperateEnemyTerminal : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.InteractionCell);
            var work = ToilMaker.MakeToil("MakeNewToils");
            work.tickAction = delegate
            {
                var actor = work.actor;
                var building = (Building)actor.CurJob.targetA.Thing;
                building.GetComp<CompMannable>().ManForATick(actor);
                actor.rotationTracker.FaceCell(building.Position);
            };
            work.handlingFacing = true;
            work.defaultCompleteMode = ToilCompleteMode.Never;
            work.FailOnCannotTouch(TargetIndex.A, PathEndMode.InteractionCell);
            yield return work;
        }
    }
}
