using RimWorld;

namespace VanillaGravshipExpanded2;

public class CompProperties_ElectricThruster : CompProperties_GravshipThruster
{
    public float rangeCapacityOverride = -1f;

    public CompProperties_ElectricThruster() => compClass = typeof(CompElectricThruster);
}