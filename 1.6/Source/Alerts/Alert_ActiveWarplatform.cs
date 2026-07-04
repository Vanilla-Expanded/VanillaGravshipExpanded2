using Verse;

namespace VanillaGravshipExpanded2
{
    public class Alert_ActiveWarplatform : Alert_Warplatform
    {
        protected override bool ShouldReport(MapParent_WarPlatform wp) => !wp.defeated && WorldComponent_GravshipCombat.GetActiveGravEngine != null;
        public override string GetLabel() => FindWarplatform(ShouldReport).threatDef.label;
        public override TaggedString GetExplanation() => FindWarplatform(ShouldReport).threatDef.alertExplanation.Formatted(WorldComponent_GravshipCombat.GetActiveGravEngine.RenamableLabel);
    }
}
