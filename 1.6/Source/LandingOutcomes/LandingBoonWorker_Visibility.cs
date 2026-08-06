using RimWorld.Planet;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2;

public class LaunchBoonWorker_Visibility : LaunchBoonWorker
{
    private VisibilityLandingOutcomeExtension extension;

    public LaunchBoonWorker_Visibility(LaunchBoonDef def) : base(def)
    {
        extension = def.GetModExtension<VisibilityLandingOutcomeExtension>();
        if (extension == null)
            Log.Error($"A {nameof(LaunchBoonDef)} with {nameof(LaunchBoonWorker_Visibility)} worker doesn't have an {nameof(VisibilityLandingOutcomeExtension)} extension.");
    }

    public override void ApplyBoon(Gravship gravship)
    {
        base.ApplyBoon(gravship);

        var info = gravship.engine?.launchInfo.ExtendedVGE2Info(false);
        if (info != null)
        {
            info.launchVisibilityFactor = extension.visibilityFactor;
            info.launchVisibilityOffset = extension.visibilityOffset;
            info.launchVisibilityOffsetNoFactor = extension.visibilityOffsetNoFactor;
        }
    }

    public override bool CanTrigger(Gravship gravship) => base.CanTrigger(gravship) && extension != null;
}