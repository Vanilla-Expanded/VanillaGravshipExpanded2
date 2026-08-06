using RimWorld;
using RimWorld.Planet;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2;

public class LandingOutcomeWorker_Visibility : LandingOutcomeWorker_GravshipBase
{
    private VisibilityLandingOutcomeExtension extension;

    public LandingOutcomeWorker_Visibility(LandingOutcomeDef def) : base(def)
    {
        extension = def.GetModExtension<VisibilityLandingOutcomeExtension>();
        if (extension == null)
            Log.Error($"A {nameof(LandingOutcomeDef)} with {nameof(LandingOutcomeWorker_Visibility)} worker doesn't have an {nameof(VisibilityLandingOutcomeExtension)} extension.");
    }

    public override void ApplyOutcome(Gravship gravship)
    {
        base.ApplyOutcome(gravship);

        var info = gravship.engine?.launchInfo.ExtendedVGE2Info(false);
        if (info != null)
        {
            info.launchVisibilityFactor = extension.visibilityFactor;
            info.launchVisibilityOffset = extension.visibilityOffset;
            info.launchVisibilityOffsetNoFactor = extension.visibilityOffsetNoFactor;
        }
    }

    public override bool CanTrigger(Gravship gravship) => extension != null;
}