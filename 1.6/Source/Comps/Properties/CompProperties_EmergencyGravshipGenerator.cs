using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2;

public class CompProperties_EmergencyGravshipGenerator : CompProperties_Power
{
    public float maxPowerIncreasePerRareTick;
    public float maxWattDaysOutput = 10000f;

    public bool pushHeatIntoEntireGravship = false;
    public SimpleCurve heatPerSecond;

    public float cooldownDaysAfterBreakdown;

    public SimpleCurve fireChanceAfterBreakdown;
    public SimpleCurve fireSizeAfterBreakdown;
    public SimpleCurve astrofireChanceAfterBreakdown;
    public SimpleCurve astrofireSizeAfterBreakdown;

    public DamageDef breakdownExplosionDamageDef;
    public int breakdownExplosionDamage = -1;
    public float breakdownExplosionRadius;
    public float breakdownExplosionArmorPenetration = -1f;
    public SoundDef breakdownExplosionSoundDef;
    public float breakdownExplosionChanceToStartFire;
    public SimpleCurve breakdownExplosionFlammabilityChanceCurve;
    public bool breakdownExplosionDamageFalloff = false;

    public CompProperties_EmergencyGravshipGenerator() => compClass = typeof(CompPowerEmergencyGravshipGenerator);
}