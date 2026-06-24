using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2;

public class CompProperties_EscapePod : CompProperties
{
    public int enterDuration = 120;
    public int launchDuration = 300;

    public List<PlanetLayerDef> layerWhitelist;

    public TargetingParameters insertPawnTargetingParameters = TargetingParameters.ForColonist();

    public ThingDef activeTransporterDef;
    public ThingDef skyfallerLeaving;
    public WorldObjectDef worldObjectDef;
    public SoundDef enterSound;

    public CompProperties_EscapePod() => compClass = typeof(CompEscapePod);
}