using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2;

public class CompPowerEmergencyGravshipGenerator : CompPowerPlant
{
    public CompGravMaintainable maintainable;
    protected bool isActive = false;
    protected float currentPowerOutput;
    protected float currentMaintenanceLoss;

    public new CompProperties_EmergencyGravshipGenerator Props => (CompProperties_EmergencyGravshipGenerator)props;

    public override float DesiredPowerOutput
    {
        get
        {
            if (isActive)
                return -currentPowerOutput;
            return 0;
        }
    }

    public bool IsActive => isActive;

    protected float TargetEnergyGainRatePerDay
    {
        get
        {
            var output = 0f;

            for (var i = 0; i < PowerNet.powerComps.Count; i++)
            {
                if (PowerNet.powerComps[i].PowerOn || (FlickUtility.WantsToBeOn(PowerNet.powerComps[i].parent) && !PowerNet.powerComps[i].parent.IsBrokenDown()))
                    output += PowerNet.powerComps[i].EnergyOutputPerTick;
            }

            return output / WattsToWattDaysPerTick;
        }
    }

    public override void CompTickInterval(int delta)
    {
        base.CompTickInterval(delta);

        if (!isActive || !parent.Spawned)
            return;

        if (parent.Map.GameConditionManager.ElectricityDisabled(parent.Map))
        {
            ResetPowerOutputIfNeeded(true);
            return;
        }

        var powerNet = PowerNet;
        if (powerNet == null)
        {
            ResetPowerOutputIfNeeded(true);
            return;
        }

        // Rare tick check
        if (parent.IsHashIntervalTick(GenTicks.TickRareInterval, delta))
        {
            // Hourly check (nested)
            if (parent.IsHashIntervalTick(GenDate.TicksPerHour, GenTicks.TickRareInterval))
                currentMaintenanceLoss += Props.extraDailyMaintenanceLossHourlyIncrease;

            var energyGainRate = TargetEnergyGainRatePerDay + Props.basePowerConsumption;
            if (energyGainRate < 0)
            {
                var powerChange = Mathf.Max(energyGainRate, -Props.maxPowerIncreasePerRareTick);
                currentPowerOutput += powerChange;
                currentMaintenanceLoss -= Props.extraDailyMaintenanceLossPerMillionWatts / 1000000 * powerChange;
            }
        }

        // Cooldown starts once the building breaks down
        WorldComponent_GravshipCombat.Instance.emergencyGravshipGeneratorCooldownTicks += delta;

        maintainable.maintenance -= currentMaintenanceLoss / GenDate.TicksPerDay * delta;
        switch (maintainable.maintenance)
        {
            case <= 0f:
                ResetPowerOutputIfNeeded(true);
                break;
            case > 1f:
                maintainable.maintenance = 1f;
                break;
        }
    }

    public override void PostPostMake()
    {
        base.PostPostMake();

        currentPowerOutput = Props.basePowerConsumption;
        InitComps();
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        base.PostDeSpawn(map, mode);

        if (mode is DestroyMode.Deconstruct or DestroyMode.KillFinalize)
            ResetPowerOutputIfNeeded(true, map);
    }

    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_Values.Look(ref currentPowerOutput, nameof(currentPowerOutput));
        Scribe_Values.Look(ref isActive, nameof(isActive));

        switch (Scribe.mode)
        {
            case LoadSaveMode.LoadingVars:
                InitComps();
                break;
            case LoadSaveMode.PostLoadInit:
                ResetPowerOutputIfNeeded();
                break;
        }
    }

    public override void ReceiveCompSignal(string signal)
    {
        base.ReceiveCompSignal(signal);

        if (signal is CompBreakdownable.BreakdownSignal && isActive)
            ResetPowerOutputIfNeeded();
    }

    private void InitComps()
    {
        maintainable = parent.GetComp<CompGravMaintainable>();
        // CompPowerPlant should set up the comp... but only in spawn setup. Not post make or expose data.
        breakdownableComp ??= parent.GetComp<CompBreakdownable>();
    }

    protected void ResetPowerOutputIfNeeded(bool forceDisable = false, Map mapOverride = null, bool disableWithoutDownsides = false)
    {
        var wasEnabled = isActive;
        if (forceDisable || breakdownableComp.BrokenDown || maintainable == null || maintainable.maintenance <= 0f)
            isActive = false;
        else if (isActive)
            return;

        currentPowerOutput = Props.PowerConsumption;

        if (wasEnabled)
        {
            if (WorldComponent_GravshipCombat.Instance.activeEmergencyGravshipGenerator == parent)
                WorldComponent_GravshipCombat.Instance.activeEmergencyGravshipGenerator = null;

            if (!disableWithoutDownsides)
            {
                if (maintainable != null)
                    maintainable.maintenance = CompGravMaintainable.MaintenanceAfterBreakdown;
                if (!parent.DestroyedOrNull())
                    breakdownableComp?.DoBreakdown();

                var map = mapOverride ?? parent.MapHeld;
                if (map != null)
                {
                    var position = parent.PositionHeld;

                    var fire = Rand.Chance(Props.fireChanceAfterBreakdown);
                    var astrofire = Rand.Chance(Props.astrofireChanceAfterBreakdown);
                    if (fire || astrofire)
                    {
                        foreach (var pos in GenAdj.OccupiedRect(position, parent.Rotation, parent.def.size))
                        {
                            if (fire)
                            {
                                var size = Props.fireSizeAfterBreakdown.RandomInRange;
                                if (size > 0)
                                    FireUtility.TryStartFireIn(pos, map, size, null);
                            }
                            if (astrofire)
                            {
                                var size = Props.astrofireSizeAfterBreakdown.RandomInRange;
                                if (size > 0)
                                    FireUtility.TryStartFireIn(pos, map, size, null);
                            }
                        }
                    }

                    if (Props.breakdownExplosionDamageDef != null)
                    {
                        GenExplosion.DoExplosion(
                            position,
                            map,
                            Props.breakdownExplosionRadius,
                            Props.breakdownExplosionDamageDef,
                            parent,
                            damAmount: Props.breakdownExplosionDamage,
                            armorPenetration: Props.breakdownExplosionArmorPenetration,
                            explosionSound: Props.breakdownExplosionSoundDef,
                            damageFalloff: Props.breakdownExplosionDamageFalloff,
                            chanceToStartFire: Props.breakdownExplosionChanceToStartFire,
                            flammabilityChanceCurve: Props.breakdownExplosionFlammabilityChanceCurve
                        );
                    }
                }
            }
        }
    }

    public virtual void Activate(Pawn caster)
    {
        isActive = true;
        currentMaintenanceLoss = Props.baseDailyMaintenanceLoss;
        WorldComponent_GravshipCombat.Instance.emergencyGravshipGeneratorCooldownTicks = Mathf.RoundToInt(Props.cooldownDaysAfterBreakdown * GenDate.TicksPerDay);
        WorldComponent_GravshipCombat.Instance.activeEmergencyGravshipGenerator = parent;
    }

    public override string CompInspectStringExtra()
    {
        var builder = new StringBuilder(base.CompInspectStringExtra());

        if (isActive)
        {
            builder.AppendInNewLine("VGE_EmergencyGenerator_DailyLoss".Translate(currentMaintenanceLoss).CapitalizeFirst());
        }
        else
        {
            builder.AppendInNewLine("VGE_EmergencyGenerator_Inactive".Translate().CapitalizeFirst());
        }

        return builder.ToString();
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (var gizmo in base.CompGetGizmosExtra())
            yield return gizmo;

        if (DebugSettings.ShowDevGizmos)
        {
            var resetGizmo = new Command_Action
            {
                defaultLabel = "DEV: Actually reset cooldown",
                action = () =>
                {
                    if (WorldComponent_GravshipCombat.Instance.activeEmergencyGravshipGenerator == null)
                        WorldComponent_GravshipCombat.Instance.emergencyGravshipGeneratorCooldownTicks = 0;
                },
            };
            var breakdownGizmo = new Command_Action
            {
                defaultLabel = "DEV: Breakdown active generator",
                action = () =>
                {
                    if (WorldComponent_GravshipCombat.Instance.activeEmergencyGravshipGenerator != null)
                    {
                        var comp = WorldComponent_GravshipCombat.Instance.activeEmergencyGravshipGenerator.TryGetComp<CompPowerEmergencyGravshipGenerator>();
                        comp?.ResetPowerOutputIfNeeded(true);
                        WorldComponent_GravshipCombat.Instance.activeEmergencyGravshipGenerator = null;
                    }
                }
            };
            var disableGizmo = new Command_Action
            {
                defaultLabel = "DEV: Disable active generator",
                action = () =>
                {
                    if (WorldComponent_GravshipCombat.Instance.activeEmergencyGravshipGenerator != null)
                    {
                        var comp = WorldComponent_GravshipCombat.Instance.activeEmergencyGravshipGenerator.TryGetComp<CompPowerEmergencyGravshipGenerator>();
                        comp?.ResetPowerOutputIfNeeded(true, disableWithoutDownsides: true);
                        WorldComponent_GravshipCombat.Instance.activeEmergencyGravshipGenerator = null;
                    }
                }
            };

            if (WorldComponent_GravshipCombat.Instance.activeEmergencyGravshipGenerator != null)
            {
                resetGizmo.Disable("A generator is active");
            }
            else
            {
                breakdownGizmo.Disable("No generator active");
                disableGizmo.Disable("No generator active");
            }

            yield return resetGizmo;
            yield return breakdownGizmo;
            yield return disableGizmo;
        }
    }
}