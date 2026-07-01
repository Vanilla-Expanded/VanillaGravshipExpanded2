using RimWorld;

namespace VanillaGravshipExpanded2
{
    public class CompProperties_SignalJammer : CompProperties_GravshipFacility
    {
        public CompProperties_SignalJammer()
        {
            compClass = typeof(CompSignalJammer);
        }
    }

    public class CompSignalJammer : CompGravshipFacility
    {
        public override bool CanBeActive => base.CanBeActive && !((Building_SignalJammer)parent).OnCooldown;
    }
}
