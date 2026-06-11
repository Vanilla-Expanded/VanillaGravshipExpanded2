using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2;

public class Building_Airlock : Building_SupportedDoor
{
    private CompPowerTrader powerTrader;
    private CompBreakdownable breakdownable;
    private VacuumComponent vacuum;
    private bool wasBrokenDown = false;

    private bool Active => (powerTrader == null || powerTrader.PowerOn) && breakdownable is not { BrokenDown: true };

    public override bool ExchangeVacuum => base.ExchangeVacuum && !Active;

    public override float TempEqualizeRate => Active ? 0f : base.TempEqualizeRate;

    public override void Tick()
    {
        base.Tick();

        // No "Breakdown fixed" signal, we need to watch for state change ourselves.
        // If breakdownable is null or not broken down, mark vacuum dirty.
        if (wasBrokenDown && breakdownable is not { BrokenDown: true })
        {
            vacuum?.Dirty();
            wasBrokenDown = false;
        }
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);

        vacuum = map.GetComponent<VacuumComponent>();
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        base.DeSpawn(mode);

        // Map changed
        vacuum = null;
    }

    public override void PostMake()
    {
        base.PostMake();

        SetupComps();
    }

    public override void ExposeData()
    {
        base.ExposeData();

        if (Scribe.mode == LoadSaveMode.LoadingVars)
            SetupComps();
    }

    public override void ReceiveCompSignal(string signal)
    {
        switch (signal)
        {
            case CompPowerTrader.PowerTurnedOnSignal or CompPowerTrader.PowerTurnedOffSignal:
                vacuum?.Dirty();
                break;
            case CompBreakdownable.BreakdownSignal:
                vacuum?.Dirty();
                wasBrokenDown = true;
                break;
        }
    }

    private void SetupComps()
    {
        powerTrader = GetComp<CompPowerTrader>();
        breakdownable = GetComp<CompBreakdownable>();

        if (breakdownable is { BrokenDown: true })
            wasBrokenDown = true;
    }
}