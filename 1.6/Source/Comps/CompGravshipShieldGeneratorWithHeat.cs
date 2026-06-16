using RimWorld;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2;

public class CompGravshipShieldGeneratorWithHeat : CompGravshipShieldGenerator
{
    private CompHeatManager manager;
    private float storedHeat = 0f;

    public new CompProperties_GravshipShieldGeneratorWithHeat Props => (CompProperties_GravshipShieldGeneratorWithHeat)props;

    public float StoredHeat => storedHeat;
    public float EffectiveMaxHeat => Props.maxHeat * CompHeatManager.HeatMultiplier * CompHeatManager.HeatsinkCapacityMultiplier;
    public float ActualStoredHeat => storedHeat / (CompHeatManager.HeatMultiplier * CompHeatManager.HeatsinkCapacityMultiplier);

    public override void CompTickInterval(int delta)
    {
        base.CompTickInterval(delta);

        if (parent.IsHashIntervalTick(GenTicks.TickRareInterval, delta))
        {
            storedHeat -= Props.HeatDissipation;
            if (storedHeat < 0f)
                storedHeat = 0f;
            if (Active)
                TryPushHeat(Props.PassiveHeatGeneration);
        }
    }

    public void TryPushHeat(float heat)
    {
        if (!parent.Spawned)
            return;
        var facility = Facility;
        if (facility?.engine == null)
            return;

        if (manager == null || manager.parent != facility.engine)
        {
            manager = facility.engine.GetComp<CompHeatManager>();
            if (manager == null)
                return;
        }

        storedHeat += manager.AddHeat(heat, true, false, false);
    }

    public override void PostExposeData()
    {
        base.PostExposeData();

        Scribe_Values.Look(ref storedHeat, nameof(storedHeat));
        if (Scribe.mode == LoadSaveMode.PostLoadInit && storedHeat < 0f)
            storedHeat = 0f;
    }

    public override string CompInspectStringExtra()
    {
        return "VGE_HeatsinkHeatStored".Translate(ActualStoredHeat.ToString("F1"), Props.maxHeat.ToString("F1"));
    }
}