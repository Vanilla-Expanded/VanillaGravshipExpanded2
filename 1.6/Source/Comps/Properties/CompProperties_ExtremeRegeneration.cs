using Verse;

namespace VanillaGravshipExpanded2;

public class CompProperties_ExtremeRegeneration : CompProperties
{
    public int rateInTicks;
    public float tendMin;
    public float tendMax;
    public HediffDef activeHediff;
    public HediffDef inactiveHediff;


    public CompProperties_ExtremeRegeneration() => compClass = typeof(CompExtremeRegeneration);
}