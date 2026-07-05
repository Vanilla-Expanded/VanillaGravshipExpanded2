using RimWorld;
using VEF.Buildings;
using Verse;

namespace VanillaGravshipExpanded2;

public class CompProperties_InputOnlyBattery : CompProperties_Power
{
    public float storedEnergyMax;
    public CustomFillableBarGaugeData powerGauge;

    public CompProperties_InputOnlyBattery() => compClass = typeof(CompPower_InputOnlyBattery);

    public override void ResolveReferences(ThingDef parentDef)
    {
        base.ResolveReferences(parentDef);
        powerGauge?.ResolveReferences();
    }
}