using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2;

[StaticConstructorOnStartup]
public class VGE2LaunchInfo : ExtendedLaunchInfoComp
{
    public float launchVisibilityFactor = 1f;
    public float launchVisibilityOffset = 0;
    public float launchVisibilityOffsetNoFactor = 0;

    static VGE2LaunchInfo() => ExtendedLaunchInfo.onInit += (_, extendedInfo) => extendedInfo.vge2Data = new VGE2LaunchInfo();

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref launchVisibilityOffset, nameof(launchVisibilityOffset), 0);
        Scribe_Values.Look(ref launchVisibilityFactor, nameof(launchVisibilityFactor), 1f);
        Scribe_Values.Look(ref launchVisibilityOffsetNoFactor, nameof(launchVisibilityOffsetNoFactor), 1f);
    }

    public override void PostLandingEnded(Gravship gravship)
    {
        try
        {
            var comp = WorldComponent_GravshipCombat.Instance;
            var engine = gravship.Engine;
            comp.lastKnownShipMap = engine.Map;
            comp.lastKnownShipTile = engine.Tile;
            if (comp.incomingWarplatform)
            {
                ApplyEarlyEscape(comp, gravship);
            }
            else
            {
                ApplyVisibilityGain(gravship);
            }

            ApplySignalJammerEffect(gravship);
        }
        catch (Exception arg)
        {
            Log.Error($"[VGE2] Exception in LandingEnded: {arg}");
        }
    }

    private void ApplySignalJammerEffect(Gravship gravship)
    {
        var jammer = gravship.Engine.GravshipComponents
            .Where(c => c.parent.def == InternalDefOf.SignalJammer)
            .Select(c => c.parent.GetComp<CompSignalJammer>())
            .FirstOrDefault(x => x != null && !x.OnCooldown);
        if (jammer is null) return;

        var map = gravship.Engine.Map;
        var enemyArtillery = new List<Building_GravshipTurret>();
        foreach (var b in map.listerBuildings.allBuildingsNonColonist)
        {
            if (b is Building_GravshipTurret turret && turret.HostileTo(Faction.OfPlayer))
            {
                enemyArtillery.Add(turret);
            }
        }

        if (enemyArtillery.Any())
        {
            jammer.StartCooldown();
            foreach (var art in enemyArtillery)
            {
                art.GetComp<CompStunnable>()?.StunHandler?.StunFor(30000, jammer.parent, true, true);
                FleckMaker.ThrowMicroSparks(art.DrawPos, map);
                for (int i = 0; i < 3; i++)
                {
                    FleckMaker.Static(art.OccupiedRect().RandomCell.ToVector3Shifted(), map, InternalDefOf.BlastEMP, 1f);
                }
            }

            Messages.Message("VGE_SignalJammerStunnedArtillery".Translate(), MessageTypeDefOf.PositiveEvent, false);
        }
    }

    private void ApplyVisibilityGain(Gravship gravship)
    {
        var engine = gravship?.Engine;
        var extendedInfo = engine?.launchInfo.ExtendedInfo(false);
        if (extendedInfo == null || !extendedInfo.launchSourceTile.Valid) return;
        var distance = GravshipHelper.GetDistance(extendedInfo.launchSourceTile, engine.Tile);
        var size = engine.ValidSubstructure.Count;
        WorldComponent_GravshipCombat.Instance.AddVisibility(size * distance * launchVisibilityFactor + launchVisibilityOffset, true);
        if (launchVisibilityOffsetNoFactor != 0)
            WorldComponent_GravshipCombat.Instance.AddVisibility(launchVisibilityOffsetNoFactor, applyFactors: false);
    }

    private void ApplyEarlyEscape(WorldComponent_GravshipCombat comp, Gravship gravship)
    {
        if (comp.incomingWarplatform)
        {
            var threatDef = comp.activeThreatDef;
            comp.incomingWarplatform = false;
            threatDef.Worker.OnEarlyEscape(gravship.Engine.Map);
        }
    }
}