using System.Linq;
using RimWorld;
using UnityEngine;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class GravshipThreatWorker_SalvagerStation : GravshipThreatWorker
    {
        public override Faction EnemyFaction => Faction.OfSalvagers;

        public override bool ShouldDefeat(Map map)
        {
            var enemiesExist = map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial)
                .Any(x => x.Faction == Faction.OfSalvagers && (x is Building_GravshipTurret || x.def == InternalDefOf.VGE_EnemyGravlockTether));
            return !enemiesExist;
        }

        public override void Fire(Building_GravEngine engine)
        {
            var comp = WorldComponent_GravshipCombat.Instance;
            comp.activeThreatDef = def;
            comp.incomingWarplatform = true;

            var delayDays = 5;
            if (engine.def == InternalDefOf.VGE_GravjumperEngine) delayDays = 10;
            else if (engine.def == InternalDefOf.VGE_GravhulkEngine) delayDays = 3;

            var tribute = (int)Mathf.Max(2500f, Find.Maps.Sum(m => m.wealthWatcher.WealthTotal) * 0.025f);
            var stationName = NameGenerator.GenerateName(InternalDefOf.VGE_NamerPirateOrbitalStation);

            var kind = Faction.OfSalvagers.def.pawnGroupMakers.SelectMany(gm => gm.options).RandomElementByWeight(opt => opt.selectionWeight).kind;
            var leader = PawnGenerator.GeneratePawn(kind, Faction.OfSalvagers);
            comp.salvagerLeader = leader;
            comp.salvagerTributeAmount = tribute;
            comp.salvagerDelayDays = delayDays;
            comp.salvagerStationName = stationName;
            comp.tributeDemandTick = Find.TickManager.TicksGame + def.baseCountdownHours * GenDate.TicksPerHour;

            var jammerDesc = "";
            var jammer = engine.AffectedByFacilities.LinkedFacilitiesListForReading.OfType<Building_SignalJammer>().FirstOrDefault(x => !x.OnCooldown);
            if (jammer != null) { jammer.StartCooldown(); comp.tributeDemandTick += def.jammerExtensionHours * GenDate.TicksPerHour; jammerDesc = "\n\n" + "VGE_JammerScrambledSalvager".Translate(); }

            var letterText = def.letterDesc.Formatted(
                stationName.Colorize(ColoredText.FactionColor_Hostile),
                leader.Name.ToStringFull.Colorize(ColoredText.NameColor),
                engine.RenamableLabel.Colorize(ColoredText.NameColor),
                tribute.ToString().Colorize(ColoredText.CurrencyColor),
                delayDays) + jammerDesc;
            Find.LetterStack.ReceiveLetter(def.letterLabel, letterText, LetterDefOf.ThreatBig);
        }

        public override void OnDefeat(Map map)
        {
            base.OnDefeat(map);
            WorldComponent_GravshipCombat.Instance.engineLockedRemotely = false;
        }

        public override void OnEscape(MapParent_WarPlatform warplatform)
        {
            base.OnEscape(warplatform);
            WorldComponent_GravshipCombat.Instance.salvagerDropshipTick = Find.TickManager.TicksGame + Rand.RangeInclusive(1, 3) * GenDate.TicksPerDay;
        }
    }
}
