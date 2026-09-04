using System.Collections.Generic;
using System.Text;
using RimWorld;
using UnityEngine;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2;

public class CompPowerEmergencyGravshipGenerator : CompPowerPlant
{
    public CompGravshipFacility facility;
    public CompGravMaintainable maintainable;
    protected CompHeatManager manager;
    protected bool isActive = false;
    protected float currentPowerOutput;
    protected float totalWattDaysOutputThisActivation = 0f;

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

    private float RemainingPower => Mathf.Max(Props.maxWattDaysOutput - totalWattDaysOutputThisActivation, 0);

    private int RemainingDurationTicksAtCurrentOutput => (int)(RemainingPower / (-currentPowerOutput * WattsToWattDaysPerTick));

    public override void CompTickInterval(int delta)
    {
        base.CompTickInterval(delta);

        if (!isActive || !parent.Spawned)
            return;

        var powerNet = PowerNet;
        if (powerNet == null || facility is { engine: null } || maintainable is { maintenance: <= 0f } || parent.Map.GameConditionManager.ElectricityDisabled(parent.Map))
        {
            ResetPowerOutputIfNeeded(true);
            return;
        }

        // Rare tick check
        if (parent.IsHashIntervalTick(GenTicks.TickRareInterval, delta))
        {
            var energyGainRate = TargetEnergyGainRatePerDay + Props.basePowerConsumption;
            if (energyGainRate < 0)
            {
                var powerChange = Mathf.Max(energyGainRate, -Props.maxPowerIncreasePerRareTick);
                currentPowerOutput += powerChange;
            }
        }

        // Cooldown starts once the building breaks down
        WorldComponent_GravshipCombat.Instance.emergencyGravshipGeneratorCooldownTicks += delta;
        totalWattDaysOutputThisActivation -= currentPowerOutput * delta * WattsToWattDaysPerTick;

        if (totalWattDaysOutputThisActivation >= Props.maxWattDaysOutput)
        {
            ResetPowerOutputIfNeeded(true);
            return;
        }

        if (Props.heatPerSecond != null)
        {
            // If no engine, once per second push heat
            if (facility?.engine == null || !Props.pushHeatIntoEntireGravship)
            {
                if (parent.IsHashIntervalTick(GenTicks.TicksPerRealSecond, delta))
                    GenTemperature.PushHeat(parent.Position, parent.Map, Props.heatPerSecond.Evaluate(-currentPowerOutput));
            }
            // If connected to an engine, push heat once per 15 seconds (since we push to a lot more places)
            else if (parent.IsHashIntervalTick(GenTicks.TicksPerRealSecond * 15, delta))
            {
                if (manager == null || manager.parent != facility.engine)
                {
                    manager = facility.engine.GetComp<CompHeatManager>();
                    if (manager == null)
                        return;
                }

                var heat = Props.heatPerSecond.Evaluate(-currentPowerOutput) * 10;
                if (!manager.TryApplyHeatToShip(heat))
                    GenTemperature.PushHeat(parent.Position, parent.Map, heat);
            }
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
        Scribe_Values.Look(ref totalWattDaysOutputThisActivation, nameof(totalWattDaysOutputThisActivation));
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
        facility = parent.GetComp<CompGravshipFacility>();
        maintainable = parent.GetComp<CompGravMaintainable>();
        // CompPowerPlant should set up the comp... but only in spawn setup. Not post make or expose data.
        breakdownableComp ??= parent.GetComp<CompBreakdownable>();
    }

    protected void ResetPowerOutputIfNeeded(bool forceDisable = false, Map mapOverride = null, bool disableWithoutDownsides = false)
    {
        var wasEnabled = isActive;
        if (!isActive || forceDisable || breakdownableComp is { BrokenDown: true } || maintainable is { maintenance: <= 0f })
            isActive = false;
        else
            return;

        var outputBefore = -currentPowerOutput;
        currentPowerOutput = Props.PowerConsumption;
        totalWattDaysOutputThisActivation = 0f;

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

                    var fireSize = Props.fireSizeAfterBreakdown?.Evaluate(outputBefore) ?? -1;
                    var astrofireSize = Props.astrofireSizeAfterBreakdown?.Evaluate(outputBefore) ?? -1;
                    var fire = fireSize > 0 && Props.fireChanceAfterBreakdown != null && Rand.Chance(Props.fireChanceAfterBreakdown.Evaluate(outputBefore));
                    var astrofire = astrofireSize > 0 && Props.astrofireChanceAfterBreakdown != null && Rand.Chance(Props.astrofireChanceAfterBreakdown.Evaluate(outputBefore));

                    if (fire || astrofire)
                    {
                        foreach (var pos in GenAdj.OccupiedRect(position, parent.Rotation, parent.def.size))
                        {
                            if (fire)
                                FireUtility.TryStartFireIn(pos, map, fireSize, null);
                            if (astrofire)
                                FireUtility.TryStartFireIn(pos, map, astrofireSize, null);
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
        WorldComponent_GravshipCombat.Instance.emergencyGravshipGeneratorCooldownTicks = Mathf.RoundToInt(Props.cooldownDaysAfterBreakdown * GenDate.TicksPerDay);
        WorldComponent_GravshipCombat.Instance.activeEmergencyGravshipGenerator = parent;
    }

    public override string CompInspectStringExtra()
    {
        var builder = new StringBuilder(base.CompInspectStringExtra());

        if (isActive)
            builder.AppendInNewLine("VGE_EmergencyGenerator_RemainingPowerOutput".Translate(RemainingPower.ToStringDecimalIfSmall(), RemainingDurationTicksAtCurrentOutput.ToStringTicksToPeriod()).CapitalizeFirst());
        else
            builder.AppendInNewLine("VGE_EmergencyGenerator_Inactive".Translate().CapitalizeFirst());

        return builder.ToString();
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (var gizmo in base.CompGetGizmosExtra())
            yield return gizmo;

        if (DebugSettings.ShowDevGizmos)
        {
            if (WorldComponent_GravshipCombat.Instance.activeEmergencyGravshipGenerator != null)
            {
                yield return new Command_Action
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
                yield return new Command_Action
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
            }
            else if (WorldComponent_GravshipCombat.Instance.emergencyGravshipGeneratorCooldownTicks > 0)
            {
                yield return new Command_Action
                {
                    defaultLabel = "DEV: Actually reset cooldown",
                    action = () =>
                    {
                        if (WorldComponent_GravshipCombat.Instance.activeEmergencyGravshipGenerator == null)
                            WorldComponent_GravshipCombat.Instance.emergencyGravshipGeneratorCooldownTicks = 0;
                    },
                };
            }
        }
    }
}