using RimWorld;

namespace VanillaGravshipExpanded2;

public class CompProperties_ApparelVerbOwnerOxygen : CompProperties_ApparelVerbOwnerCharged
{
    public float chargePerUse;
    public int cooldownTicks;

    public CompProperties_ApparelVerbOwnerOxygen()
    {
        compClass = typeof(CompApparelVerbOwner_Oxygen);
    }
}