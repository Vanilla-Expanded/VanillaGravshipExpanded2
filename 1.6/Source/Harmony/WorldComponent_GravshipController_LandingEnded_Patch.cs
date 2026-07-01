using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(WorldComponent_GravshipController), "LandingEnded")]
    public static class WorldComponent_GravshipController_LandingEnded_Patch
    {
        public static void Postfix(WorldComponent_GravshipController __instance)
        {
            var map = __instance.map;
            var hasEnemyArtillery = false;
            var enemyArtillery = new List<Building_GravshipTurret>();
            foreach (var b in map.listerBuildings.allBuildingsNonColonist)
            {
                if (b is Building_GravshipTurret turret && turret.HostileTo(Faction.OfPlayer))
                {
                    hasEnemyArtillery = true;
                    enemyArtillery.Add(turret);
                }
            }

            if (hasEnemyArtillery)
            {
                var gravship = __instance.gravship;
                var jammer = gravship.Engine.GravshipComponents.Select(x => x.parent).OfType<Building_SignalJammer>().FirstOrDefault(x => x.OnCooldown is false);
                if (jammer != null)
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
        }
    }
}
