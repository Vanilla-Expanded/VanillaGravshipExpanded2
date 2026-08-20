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

    public FuelUsageData ConsumeFuelRatio(Building_GravEngine engine, float fuelConsumedRatio, List<CompGravshipThruster> activeThrusters, List<IGravshipFuelProvider> otherProviders, bool consumeFuel)
    {
        if (!IsActive(engine, activeThrusters, otherProviders))
            return null;

        var report = new FuelUsageData();

        var maxCharge = battery.Props.storedEnergyMax;
        var range = RangeProvidedByThrusters(null, false);
        report.fuelData[this] = report.totalAmount = battery.StoredEnergy * fuelConsumedRatio;

        otherProviders?.RemoveAll(x =>
        {
            if (x is not CompElectricThruster other)
                return false;

            var charge = other.battery.StoredEnergy * fuelConsumedRatio;
            maxCharge += other.battery.Props.storedEnergyMax;
            range += other.RangeProvidedByThrusters(null, false);
            report.fuelData[this] = charge;
            report.totalAmount += charge;
            return true;
        });

        report.sortingOrder = range * fuelConsumedRatio;
        if (report.totalAmount > 0)
            report.reportString = $"{(report.totalAmount / maxCharge).ToStringPercent()} {"VGE_UsedCharge".Translate()}";

        return report;
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

    public FuelTabEntry GetFuelTabEntry(Building_GravEngine engine, List<CompGravshipThruster> activeThrusters, List<IGravshipFuelProvider> otherProviders)
    {
        var entry = new SimpleMultiLineTextEntry(engine)
        {
            title = "VGE_FuelTab_ElectricThrusters".Translate().CapitalizeFirst()
        };

        entry.fuelProviders.Add(ParentThing);
        entry.thrusters.Add(ParentThing);
        var maxRange = MaxRangeProvidedByFuel(engine, activeThrusters, null);
        var currentRange = 0f;
        var chargedThrusters = 0;
        if (battery.StoredEnergyPct >= Props.chargePercentRequiredToUse)
            chargedThrusters += 1;
        if (IsActive(engine, activeThrusters, null))
            currentRange += CurrentRangeProvidedByFuel(engine, activeThrusters, null);

        otherProviders?.RemoveAll(x =>
        {
            if (x is not CompElectricThruster other)
                return false;

            entry.fuelProviders.Add(other.ParentThing);
            entry.thrusters.Add(other.ParentThing);
            maxRange += other.MaxRangeProvidedByFuel(engine, activeThrusters, null);
            if (other.IsActive(engine, activeThrusters, null))
                currentRange += other.CurrentRangeProvidedByFuel(engine, activeThrusters, null);
            if (other.battery.StoredEnergyPct >= other.Props.chargePercentRequiredToUse)
                chargedThrusters += 1;
            return true;
        });

        // entry.text.Add($"{"VGE_FuelTab_Thrusters".Translate().CapitalizeFirst()}: {entry.thrusters.Count}");
        entry.text.Add($"{"VGE_FuelTab_ChargedThrusters".Translate().CapitalizeFirst()}: {chargedThrusters}");
        entry.text.Add($"{"VGE_FuelTab_Range".Translate().CapitalizeFirst()}: {currentRange} / {maxRange}");

        return entry;
    }
}