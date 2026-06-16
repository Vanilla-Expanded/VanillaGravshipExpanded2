using RimWorld;
using VanillaGravshipExpanded;

namespace VanillaGravshipExpanded2;

public class CompProperties_GravshipShieldGeneratorWithHeat : CompProperties_ProjectileInterceptor
{
    public float maxHeat = 10f;
    public float passiveHeatGeneration = 1f;
    public float heatPerDamage = 0.1f;
    public float heatDissipation = 1f;

    public float PassiveHeatGeneration => passiveHeatGeneration * CompHeatManager.BaseHeatsinkCapacityMultiplier;
    public float HeatPerDamage => heatPerDamage * CompHeatManager.BaseHeatsinkCapacityMultiplier;
    public float HeatDissipation => heatDissipation * CompHeatManager.HeatMultiplier * CompHeatManager.HeatsinkCapacityMultiplier;

    public CompProperties_GravshipShieldGeneratorWithHeat() => compClass = typeof(CompGravshipShieldGeneratorWithHeat);
}