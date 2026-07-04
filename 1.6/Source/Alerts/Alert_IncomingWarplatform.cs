using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class Alert_IncomingWarplatform : Alert_Critical
    {
        public override string GetLabel() => WorldComponent_GravshipCombat.Instance.activeThreatDef.label;
        public override TaggedString GetExplanation()
        {
            var comp = WorldComponent_GravshipCombat.Instance;
            return comp.activeThreatDef.description.Formatted((comp.warplatformTick - Find.TickManager.TicksGame).ToStringTicksToPeriod());
        }
        public override AlertReport GetReport()
        {
            if (WorldComponent_GravshipCombat.Instance.incomingWarplatform) return AlertReport.Active;
            return AlertReport.Inactive;
        }
    }
}
