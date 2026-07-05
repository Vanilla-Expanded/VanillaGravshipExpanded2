using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2;

public class CompPower_InputOnlyBattery : CompPower
{
    private float storedEnergy;
    private CompStunnable stunnableComp;

    public float AmountCanAccept => parent.IsBrokenDown() || StunnedByEMP ? 0f : Props.storedEnergyMax - storedEnergy;

    public bool IsFull => storedEnergy >= Props.storedEnergyMax;
    
    public float StoredEnergy => storedEnergy;

    public float StoredEnergyPct => storedEnergy / Props.storedEnergyMax;

    public bool StunnedByEMP => stunnableComp != null && stunnableComp.StunHandler.Stunned && stunnableComp.StunHandler.StunFromEMP;

    public new CompProperties_InputOnlyBattery Props => (CompProperties_InputOnlyBattery)props;

    public void AddEnergy(float amount)
    {
        if (amount < 0f)
        {
            Log.Error($"Cannot add negative energy {amount}");
            return;
        }
        if (StunnedByEMP)
            return;
        if (amount > AmountCanAccept)
            amount = AmountCanAccept;

        storedEnergy += amount;
    }

    public void DrawPower(float amount)
    {
        storedEnergy -= amount;
        if (storedEnergy < 0f)
        {
            Log.Error($"Drawing power we don't have from {parent}");
            storedEnergy = 0f;
        }
    }

    public void SetStoredEnergyPct(float pct)
    {
        storedEnergy = Props.storedEnergyMax * Mathf.Clamp01(pct);
    }

    public override void ReceiveCompSignal(string signal)
    {
        if (signal == CompBreakdownable.BreakdownSignal)
            DrawPower(StoredEnergy);
    }

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        base.DrawAt(drawLoc, flip);
        Props.powerGauge?.DrawGauge(parent, StoredEnergyPct);
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);

        parent.Map.GetComponent<VGE2_MapComponent>()?.batteries.Add(this);
    }

    public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
    {
        base.PostDeSpawn(map, mode);

        map.GetComponent<VGE2_MapComponent>()?.batteries.Remove(this);
    }

    public override void PostPostMake()
    {
        base.PostPostMake();
        InitializeComps();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref storedEnergy, nameof(storedEnergy));
        if (storedEnergy > Props.storedEnergyMax)
            storedEnergy = Props.storedEnergyMax;

        if (Scribe.mode == LoadSaveMode.LoadingVars)
           InitializeComps();
    }

    private void InitializeComps()
    {
        stunnableComp = parent.GetComp<CompStunnable>();
    }

    public override string CompInspectStringExtra()
    {
        return $"{"PowerBatteryStored".Translate()}: {storedEnergy:F0} / {Props.storedEnergyMax:F0} Wd\n{base.CompInspectStringExtra()}";
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (var gizmo in base.CompGetGizmosExtra())
            yield return gizmo;

        if (DebugSettings.ShowDevGizmos)
        {
            yield return new Command_Action
            {
                defaultLabel = "DEV: Fill",
                action = () => SetStoredEnergyPct(1f),
            };
            yield return new Command_Action
            {
                defaultLabel = "DEV: Empty",
                action = () => SetStoredEnergyPct(0f),
            };
        }
    }
}