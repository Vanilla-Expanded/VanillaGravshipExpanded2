using RimWorld;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2;

public class CompProperties_GravshipShieldGeneratorWithHeat : CompProperties_ProjectileInterceptor
{
    public float maxHeat = 10f;
    public float passiveHeatGeneration = 1f;
    public float heatPerDamage = 0.1f;
    public float heatDissipationPerHour = 1f;
    public float explosionRadius = 0f;
    public int explosionDamage = -1;
    public DamageDef explosionDamageDef = null;

    public float PassiveHeatGeneration => passiveHeatGeneration * CompHeatManager.BaseHeatsinkCapacityMultiplier;
    public float HeatPerDamage => heatPerDamage * CompHeatManager.BaseHeatsinkCapacityMultiplier;
    public float HeatDissipation => heatDissipationPerHour * CompHeatManager.HeatMultiplier * CompHeatManager.HeatsinkCapacityMultiplier;

    public CompProperties_GravshipShieldGeneratorWithHeat() => compClass = typeof(CompGravshipShieldGeneratorWithHeat);
}