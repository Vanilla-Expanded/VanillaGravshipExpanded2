using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded2
{
    public class JobDriver_CallSalvagerStation : JobDriver
    {
        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedOrNull(TargetIndex.A);
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.InteractionCell).FailOn((Toil to) => !((Building_CommsConsole)to.actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).CanUseCommsNow);

            var openComms = ToilMaker.MakeToil("MakeNewToils");
            openComms.initAction = delegate
            {
                var actor = openComms.actor;
                var comp = WorldComponent_GravshipCombat.Instance;
                if (comp.tributeDemandTick > 0 && ((Building_CommsConsole)actor.jobs.curJob.GetTarget(TargetIndex.A).Thing).CanUseCommsNow)
                {
                    var map = actor.Map;
                    var leaderName = comp.salvagerLeader.Name.ToStringFull.Colorize(ColoredText.NameColor);
                    var coloredStation = comp.salvagerStationName.Colorize(ColoredText.FactionColor_Hostile);
                    var coloredTribute = comp.salvagerTributeAmount.ToString().Colorize(ColoredText.CurrencyColor);

                    var node = new DiaNode("VGE_CallSalvagerStationDialog".Translate(leaderName, coloredStation, coloredTribute));
                    node.options.Add(comp.GetPayTributeOption(map));
                    node.options.Add(new DiaOption("Close".Translate())
                    {
                        link = null
                    });
                    Find.WindowStack.Add(new Dialog_NodeTree(node, radioMode: true));
                }
            };
            yield return openComms;
        }
    }
}
