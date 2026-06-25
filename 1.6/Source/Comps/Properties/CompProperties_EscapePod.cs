using System.Collections.Generic;
using RimWorld;
using UnityEngine;
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
    public SoundDef evacuationSustainerSound;

    [NoTranslate] public string autoRebuildGizmoIconPath = null;
    [Unsaved] private Texture2D autoRebuildGizmo;

    [NoTranslate] public string evacuationGizmoIconPath = null;
    [Unsaved] private Texture2D evacuationGizmo;

    public Texture2D AutoRebuildGizmo
    {
        get
        {
            if (autoRebuildGizmo == null)
            {
                if (!autoRebuildGizmoIconPath.NullOrEmpty())
                    autoRebuildGizmo = ContentFinder<Texture2D>.Get(autoRebuildGizmoIconPath);
                if (autoRebuildGizmo == null)
                    autoRebuildGizmo = BaseContent.BadTex;
            }

            return autoRebuildGizmo;
        }
    }

    public Texture2D EvacuationGizmo
    {
        get
        {
            if (evacuationGizmo == null)
            {
                if (!evacuationGizmoIconPath.NullOrEmpty())
                    evacuationGizmo = ContentFinder<Texture2D>.Get(evacuationGizmoIconPath);
                if (evacuationGizmo == null)
                    evacuationGizmo = BaseContent.BadTex;
            }

            return evacuationGizmo;
        }
    }

    public CompProperties_EscapePod() => compClass = typeof(CompEscapePod);
}