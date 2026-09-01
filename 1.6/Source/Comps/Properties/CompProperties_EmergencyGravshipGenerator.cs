using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2;

public class CompProperties_EmergencyGravshipGenerator : CompProperties_Power
{
    public float maxPowerIncreasePerRareTick;
    public float baseDailyMaintenanceLoss;
    public float extraDailyMaintenanceLossPerMillionWatts;
    public float extraDailyMaintenanceLossHourlyIncrease;

    public float cooldownDaysAfterBreakdown;

    public float fireChanceAfterBreakdown;
    public FloatRange fireSizeAfterBreakdown = FloatRange.One;
    public float astrofireChanceAfterBreakdown;
    public FloatRange astrofireSizeAfterBreakdown = FloatRange.One;

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