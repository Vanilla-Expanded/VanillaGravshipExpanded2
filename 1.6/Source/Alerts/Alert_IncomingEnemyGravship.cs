using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class Alert_IncomingEnemyGravship : Alert_Critical
    {
        public override string GetLabel() => "VGE_IncomingEnemyGravship".Translate();

        public override TaggedString GetExplanation()
        {
            var part = ScenPart_TheGravship.Active;
            return "VGE_IncomingEnemyGravshipDesc".Translate((part.enemyArrivalTick - Find.TickManager.TicksGame).ToStringTicksToPeriod());
        }

        public override AlertReport GetReport()
        {
            var part = ScenPart_TheGravship.Active;
            if (part != null && !part.enemyArrived && part.enemyArrivalTick > Find.TickManager.TicksGame) return AlertReport.Active;
            return AlertReport.Inactive;
        }
    }
}
