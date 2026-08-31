using System.Collections.Generic;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class Building_Siegeworm : Building_GravshipTurret
    {
        public override bool CanFire => permanentlyDisabled is false;
        public override bool CanSetForcedTarget => permanentlyDisabled is false;
        public override float GravshipTargeting => 1f;
        protected override bool ShowNoLinkedTerminalOverlay => false;

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (var gizmo in base.GetGizmos())
            {
                if (gizmo is Command_Action action && (action.defaultLabel == "VGE_LinkWithTerminal".Translate() || action.defaultLabel == "VGE_UnlinkWithTerminal".Translate() || action.defaultLabel == "VGE_SelectLinkedTerminal".Translate()))
                {
                    continue;
                }
                yield return gizmo;
            }
        }
    }
}
