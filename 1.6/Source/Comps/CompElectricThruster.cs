using System.Collections.Generic;
using RimWorld;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2;

public class CompElectricThruster : CompGravshipThruster, IGravshipFuelProvider
{
    protected CompPower_InputOnlyBattery battery;

    public Thing ParentThing => parent;

    public new CompProperties_ElectricThruster Props => (CompProperties_ElectricThruster)props;

    public override void PostPostMake()
    {
        base.PostPostMake();
        InitializeComps();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        if (Scribe.mode == LoadSaveMode.LoadingVars)
            InitializeComps();
    }

    private void InitializeComps()
    {
        battery = parent.GetComp<CompPower_InputOnlyBattery>();
    }

    public bool IsActive(Building_GravEngine engine, List<CompGravshipThruster> activeThrusters, List<IGravshipFuelProvider> otherProviders)
    {
        // Remove all inactive thrusters
        otherProviders?.RemoveAll(x => x is CompElectricThruster other && other.battery.StoredEnergyPct < other.Props.chargePercentRequiredToUse);
        return battery.StoredEnergyPct >= Props.chargePercentRequiredToUse;
    }

    public float CurrentFuel(Building_GravEngine engine) => battery.StoredEnergy;

    public float MaxFuel(Building_GravEngine engine) => battery.Props.storedEnergyMax;

    public float CurrentRangeProvidedByFuel(Building_GravEngine engine, List<CompGravshipThruster> activeThrusters, List<IGravshipFuelProvider> otherProviders)
    {
        return RangeProvidedByThrusters(otherProviders, true);
    }

    public float MaxRangeProvidedByFuel(Building_GravEngine engine, List<CompGravshipThruster> activeThrusters, List<IGravshipFuelProvider> otherProviders)
    {
        return RangeProvidedByThrusters(otherProviders, false);
    }

    public float RangeProvidedByThrusters(List<IGravshipFuelProvider> otherProviders, bool scaled)
    {
        var range = GetRange(this);

        // Handle other providers
        otherProviders?.RemoveAll(x =>
        {
            if (x is not CompElectricThruster other)
                return false;
            range += GetRange(other);
            return true;
        });

        return range;

        float GetRange(CompElectricThruster thruster)
        {
            float tempRange;
            if (thruster.Props.rangeCapacityOverride > 0f)
                tempRange = thruster.Props.rangeCapacityOverride;
            else
            {
                // Perhaps consider caching the value?
                tempRange = thruster.Props.statOffsets.GetStatValueFromList(StatDefOf.GravshipRange, thruster.Props.rangeCapacityOverride);
                if (tempRange < 0f)
                    tempRange = 0f;
            }

            if (scaled)
                tempRange *= thruster.battery.StoredEnergyPct;
            return tempRange;
        }
    }

    public float ConsumeFuelAmount(Building_GravEngine engine, float fuelAmount)
    {
        var currentAmount = battery.StoredEnergy;
        if (currentAmount >= fuelAmount)
        {
            battery.DrawPower(fuelAmount);
            return fuelAmount;
        }

        battery.SetStoredEnergyPct(0f);
        return currentAmount;
    }

    public float ConsumeFuelRatio(Building_GravEngine engine, float fuelConsumedRatio, List<CompGravshipThruster> activeThrusters, List<IGravshipFuelProvider> otherProviders)
    {
        var amountToConsume = battery.StoredEnergy * fuelConsumedRatio;
        battery.DrawPower(amountToConsume);
        return amountToConsume;
    }

    public float AddFuelAmount(Building_GravEngine engine, float amount)
    {
        var canAccept = battery.AmountCanAccept;
        if (canAccept >= amount)
        {
            battery.AddEnergy(amount);
            return amount;
        }

        battery.AddEnergy(canAccept);
        return canAccept;
    }
}