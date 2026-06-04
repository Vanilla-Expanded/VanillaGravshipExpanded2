using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class Building_TargetingConsole : Building_TargetingTerminal
    {
        public override int MaxLinkedTurrets => 3;
        public override float LinkRange => 55f;
        public override string OnlyArtilleryErrorKey => "VGE_TargetingConsoleOnlyGravshipArtillery";
        public override string LinkGizmoDesc => "VGE_LinkWithTurretConsoleDesc".Translate();
        public override string UnlinkGizmoDesc => "VGE_UnlinkWithTurretConsoleDesc".Translate();
        public override string SelectGizmoDesc => "VGE_SelectLinkedTurretConsoleDesc".Translate();
    }
}
