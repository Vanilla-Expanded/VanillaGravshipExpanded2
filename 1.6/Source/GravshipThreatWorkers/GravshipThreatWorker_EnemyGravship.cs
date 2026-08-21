using System.Linq;
using RimWorld;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class GravshipThreatWorker_EnemyGravship : GravshipThreatWorker
    {
        public override Faction EnemyFaction => Faction.OfSalvagers;

        public override void Fire(Building_GravEngine engine)
        {
            WorldComponent_GravshipCombat.Instance.enemyGravshipName = NameGenerator.GenerateName(InternalDefOf.VGE_NamerEnemyGravship);
            base.Fire(engine);
        }

        public override TaggedString GetLetterDesc(CompSignalJammer jammer)
        {
            var redName = WorldComponent_GravshipCombat.Instance.enemyGravshipName.Colorize(ColorLibrary.RedReadable);
            var desc = def.letterDesc.Formatted(redName);
            if (jammer != null)
            {
                desc += "\n\n" + "VGE_JammerScrambledGravship".Translate(redName);
            }
            return desc;
        }

        public override bool ShouldDefeat(Map map)
        {
            if (map.listerThings.ThingsOfDef(InternalDefOf.VGE_LandingStructure_EnemyGravship).Any()) return false;
            var engineDestroyed = base.ShouldDefeat(map);
            var artilleryDestroyed = !map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial).Any(x => x.Faction == EnemyFaction && x is Building_GravshipTurret);
            return engineDestroyed && artilleryDestroyed;
        }

        public override void OnEarlyEscape(Map map)
        {
            if (Rand.Chance(0.40f))
            {
                var comp = WorldComponent_GravshipCombat.Instance;
                var redName = comp.enemyGravshipName.Colorize(ColorLibrary.RedReadable);
                var letterDesc = "VGE_GravshipAttacksDesc".Translate(redName);
                Find.LetterStack.ReceiveLetter("VGE_GravshipAttacks".Translate(), letterDesc, LetterDefOf.ThreatBig);
                comp.incomingWarplatform = false;
                comp.tributeDemandTick = -1;
                def.Worker.SpawnThreat(suppressArrivalLetter: true);
            }
            else base.OnEarlyEscape(map);
        }

        public override void OnEngineDestroyed(MapParent_WarPlatform warplatform)
        {
            var comp = WorldComponent_GravshipCombat.Instance;
            var redName = comp.enemyGravshipName.Colorize(ColorLibrary.RedReadable);
            Find.LetterStack.ReceiveLetter(def.disengagesLetter, def.disengagesLetterDesc.Formatted(redName), LetterDefOf.NegativeEvent);
        }
    }
}
