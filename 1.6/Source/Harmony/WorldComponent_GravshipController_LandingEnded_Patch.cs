using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(WorldComponent_GravshipController), "LandingEnded")]
    public static class WorldComponent_GravshipController_LandingEnded_Patch
    {
        public static void Prefix(WorldComponent_GravshipController __instance)
        {
            try
            {
                var comp = WorldComponent_GravshipCombat.Instance;
                if (comp.incomingWarplatform)
                {
                    ApplyEarlyEscape(comp, __instance);
                }
                else
                {
                    ApplyVisibilityGain(__instance);
                }
                ApplySignalJammerEffect(__instance);
            }
            catch (Exception arg)
            {
                Log.Error($"[VGE2] Exception in LandingEnded Prefix: {arg}");
            }
        }

        private static void ApplySignalJammerEffect(WorldComponent_GravshipController __instance)
        {
            var gravship = __instance.gravship;
            var jammer = gravship.Engine.GravshipComponents.Select(x => x.parent).OfType<Building_SignalJammer>().FirstOrDefault(x => x.OnCooldown is false);
            if (jammer is null) return;

            var map = __instance.map;
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
                    art.GetComp<CompStunnable>()?.StunHandler?.StunFor(30000, jammer, true, true);
                    FleckMaker.ThrowMicroSparks(art.DrawPos, map);
                    for (int i = 0; i < 3; i++)
                    {
                        FleckMaker.Static(art.OccupiedRect().RandomCell.ToVector3Shifted(), map, InternalDefOf.BlastEMP, 1f);
                    }
                }
                Messages.Message("VGE_SignalJammerStunnedArtillery".Translate(), MessageTypeDefOf.PositiveEvent, false);
            }
        }

        private static void ApplyVisibilityGain(WorldComponent_GravshipController __instance)
        {
            var engine = __instance.gravship?.Engine;
            if (engine == null) return;
            var launchInfo = engine.launchInfo;
            if (launchInfo == null) return;
            if (LaunchInfo_ExposeData_Patch.launchSourceTiles.TryGetValue(launchInfo, out var sourceTile))
            {
                var distance = GravshipHelper.GetDistance(sourceTile, engine.Tile);
                var size = engine.ValidSubstructure.Count;
                WorldComponent_GravshipCombat.Instance.AddVisibility(size * distance, true);
            }
        }

        private static void ApplyEarlyEscape(WorldComponent_GravshipCombat comp, WorldComponent_GravshipController __instance)
        {
            if (comp.incomingWarplatform)
            {
                var threatDef = comp.activeThreatDef;
                comp.incomingWarplatform = false;
                comp.visibility = Mathf.Max(0, comp.visibility - threatDef.earlyEscapeVisibilityLoss);
                Messages.Message(threatDef.earlyEscapeMessage, MessageTypeDefOf.PositiveEvent);
                if (threatDef == InternalDefOf.VGE_SalvagerStation)
                {
                    var parms = new IncidentParms
                    {
                        target = __instance.map,
                        faction = Faction.OfSalvagers,
                        points = StorytellerUtility.DefaultThreatPointsNow(__instance.map),
                        raidArrivalMode = InternalDefOf.VGE_SalvagerDropshipRaid
                    };
                    Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidEnemy, Find.TickManager.TicksGame + Rand.RangeInclusive(3, 5) * GenDate.TicksPerDay, parms);
                }
            }
        }
    }
}
