using System.Collections.Generic;
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
        var maxHeat = EffectiveMaxHeat;
        if (storedHeat > maxHeat)
        {
            storedHeat = maxHeat;
            currentHitPoints = 0;
            startedChargingTick = Find.TickManager.TicksGame;
            activatedTick = Find.TickManager.TicksGame - Props.activeDuration;
            Messages.Message("VGE_ShieldOverheated".Translate(), MessageTypeDefOf.CautionInput, false);
            if (Props.explosionRadius > 0f && Props.explosionDamageDef != null)
                GenExplosion.DoExplosion(parent.Position, parent.Map, Props.explosionRadius, Props.explosionDamageDef, parent, Props.explosionDamage);

            var breakdownComp = parent.GetComp<CompBreakdownable>();
            if (breakdownComp == null)
                startedChargingTick = Find.TickManager.TicksGame + Props.chargeDurationTicks;
            else
                breakdownComp.DoBreakdown();
        }
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
        return "VGE_ShieldInternalHeat".Translate(ActualStoredHeat.ToString("F1"), Props.maxHeat.ToString("F1"));
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        yield return new Gizmo_ProjectileInterceptorHitPointsWithHeat
        {
            interceptor = this,
        };

        foreach (var gizmo in base.CompGetGizmosExtra())
            yield return gizmo;

        if (DebugSettings.ShowDevGizmos && Active)
        {
            yield return new Command_Action
            {
                defaultLabel = "DEV: Remove heat",
                action = () => storedHeat = 0f,
            };
            yield return new Command_Action
            {
                defaultLabel = "DEV: Add 1 heat",
                action = () => TryPushHeat(CompHeatManager.BaseHeatsinkCapacityMultiplier),
            };
            yield return new Command_Action
            {
                defaultLabel = "DEV: Overheat",
                action = () => TryPushHeat(EffectiveMaxHeat + CompHeatManager.BaseHeatsinkCapacityMultiplier),
            };
        }
    }
}