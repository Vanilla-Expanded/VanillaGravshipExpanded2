using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class Alert_ActiveWarplatform : Alert_Warplatform
    {
        protected override bool ShouldReport(MapParent_WarPlatform wp) => !wp.defeated;
        public override string GetLabel() => FindWarplatform(ShouldReport).threatDef.label;
        public override TaggedString GetExplanation()
        {
            var engine = GravEngineTracker.GetPlayerGravEngine();
            var shipName = engine != null ? engine.RenamableLabel : (string)"VGE_GravshipGeneric".Translate();
            return FindWarplatform(ShouldReport).threatDef.alertExplanation.Formatted(shipName);
        }
    }
}
