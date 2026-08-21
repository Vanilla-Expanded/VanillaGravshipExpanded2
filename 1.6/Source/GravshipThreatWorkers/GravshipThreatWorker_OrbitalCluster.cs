using System.Linq;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class GravshipThreatWorker_OrbitalCluster : GravshipThreatWorker
    {
        public override Faction EnemyFaction => Faction.OfMechanoids;

        public override TaggedString GetLetterDesc(CompSignalJammer jammer)
        {
            var desc = (TaggedString)def.letterDesc;
            if (jammer != null)
            {
                desc += "\n\n" + "VGE_JammerScrambledCluster".Translate(def.jammerExtensionHours / 24);
            }
            return desc;
        }

        public override bool ShouldDefeat(Map map)
        {
            var engineExists = map.listerThings.ThingsOfDef(InternalDefOf.VGE_MechanoidGravTether).Any(x => x.Destroyed is false);
            var artilleryExists = map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial)
                .Any(x => x.Faction == EnemyFaction && x is VanillaGravshipExpanded.Building_GravshipTurret && x.Destroyed is false);

            return engineExists is false && artilleryExists is false;
        }

        public override void OnEarlyEscape(Map map)
        {
            if (Rand.Chance(0.8f))
            {
                Messages.Message("VGE_OrbitalClusterRelayed".Translate(), MessageTypeDefOf.NeutralEvent, false);
                var comp = WorldComponent_GravshipCombat.Instance;
                comp.warplatformTick = Find.TickManager.TicksGame + def.baseCountdownHours.RandomInRange * GenDate.TicksPerHour;
                comp.incomingWarplatform = true;
            }
            else
            {
                base.OnEarlyEscape(map);
            }
        }

        public override void OnEscape(MapParent_WarPlatform warplatform)
        {
            if (Rand.Chance(0.8f))
            {
                Messages.Message("VGE_OrbitalClusterRelayed".Translate(), MessageTypeDefOf.NeutralEvent, false);
                var comp = WorldComponent_GravshipCombat.Instance;
                comp.warplatformTick = Find.TickManager.TicksGame + def.baseCountdownHours.RandomInRange * GenDate.TicksPerHour;
                comp.incomingWarplatform = true;
            }
            else
            {
                base.OnEscape(warplatform);
            }
        }
    }
}
