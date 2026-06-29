using Verse;

namespace VanillaGravshipExpanded2;

public class CompProperties_VacuumWarningLight : CompProperties
{
    public bool alwaysAffectsWholeRoom = true;
    public float radius = 9.9f;
    public FloatRange minMaxVacuumRanges = new(0.1f, 0.5f);
    public float baseVacuumLevel = 0.25f;
    public bool evacuatePawnsByDefault = false;
    public bool visualOnly = false;

    public CompProperties_VacuumWarningLight() => compClass = typeof(CompVacuumWarningLight);
}