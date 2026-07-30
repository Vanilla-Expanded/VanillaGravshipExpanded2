using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class Alert_TributeDemand : Alert_Critical
    {
        public override string GetLabel() => "VGE_TributeDemand".Translate();
        public override TaggedString GetExplanation()
        {
            var comp = WorldComponent_GravshipCombat.Instance;
            return "VGE_TributeDemandHover".Translate(comp.salvagerTributeAmount.ToString().Colorize(ColoredText.CurrencyColor), (comp.tributeDemandTick - Find.TickManager.TicksGame).ToStringTicksToPeriod());
        }
        public override AlertReport GetReport()
        {
            var comp = WorldComponent_GravshipCombat.Instance;
            if (comp.tributeDemandTick > Find.TickManager.TicksGame)
                return AlertReport.Active;
            return AlertReport.Inactive;
        }
    }
}
