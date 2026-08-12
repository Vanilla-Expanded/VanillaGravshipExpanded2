using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class Alert_ActiveWarplatform : Alert_Warplatform
    {
        protected override bool ShouldReport(MapParent_WarPlatform wp) => !wp.defeated && GravEngineTracker.GetPlayerGravEngine() != null;
        public override string GetLabel() => FindWarplatform(ShouldReport).threatDef.label;
        public override TaggedString GetExplanation() => FindWarplatform(ShouldReport).threatDef.alertExplanation.Formatted(GravEngineTracker.GetPlayerGravEngine().RenamableLabel);
    }
}
