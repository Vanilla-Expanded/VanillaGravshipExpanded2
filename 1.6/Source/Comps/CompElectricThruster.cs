using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2;

public class CompElectricThruster : CompGravshipThruster//, IGravshipFuelProvider
{
    protected CompPower_InputOnlyBattery battery;

    public override bool CanBeActive => base.CanBeActive && battery.IsFull;

    public new CompProperties_ElectricThruster Props => (CompProperties_ElectricThruster)props;

    public bool IsActive(Building_GravEngine engine) => battery.IsFull;

    public float ScaledFuelAmount(Building_GravEngine engine)
    {
        // If IsActive is false this method won't be called, so we don't have to bother returning early
        return ScaledFuelCapacity(engine) * battery.StoredEnergyPct;
    }

    public float ScaledFuelCapacity(Building_GravEngine engine)
    {
        if (Props.rangeCapacityOverride > 0f)
            return Props.rangeCapacityOverride;

        // Perhaps consider caching the value?
        var range = Props.statOffsets.GetStatValueFromList(StatDefOf.GravshipRange, Props.rangeCapacityOverride);
        if (range <= 0f)
            return 0f;
        return range;
    }

    public void ConsumeFuel(Building_GravEngine engine, float fuelConsumedRatio)
    {
        battery.SetStoredEnergyPct(1 - fuelConsumedRatio);
    }

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
}