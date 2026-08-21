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
            return enemiesExist is false;
        }

        public override void Fire(Building_GravEngine engine)
        {
            var comp = WorldComponent_GravshipCombat.Instance;

            var delayDays = 5;
            if (engine?.def == InternalDefOf.VGE_GravjumperEngine) delayDays = 10;
            else if (engine?.def == InternalDefOf.VGE_GravhulkEngine) delayDays = 3;

            var tribute = (int)Mathf.Max(2500f, Find.Maps.Sum(m => m.wealthWatcher.WealthTotal) * 0.025f);
            var stationName = NameGenerator.GenerateName(InternalDefOf.VGE_NamerPirateOrbitalStation);

            var kind = Faction.OfSalvagers.def.pawnGroupMakers.SelectMany(gm => gm.options).RandomElementByWeight(opt => opt.selectionWeight).kind;
            var leader = PawnGenerator.GeneratePawn(kind, Faction.OfSalvagers);
            comp.salvagerLeader = leader;
            comp.salvagerTributeAmount = tribute;
            comp.salvagerDelayDays = delayDays;
            comp.salvagerStationName = stationName;

            base.Fire(engine);
            comp.tributeDemandTick = comp.warplatformTick;
        }

        public override TaggedString GetLetterDesc(CompSignalJammer jammer)
        {
            var comp = WorldComponent_GravshipCombat.Instance;
            var engine = GravEngineTracker.GetPlayerGravEngine();
            var shipName = engine != null ? engine.RenamableLabel : (string)"VGE_GravshipGeneric".Translate();
            var desc = def.letterDesc.Formatted(
                comp.salvagerStationName.Colorize(ColoredText.FactionColor_Hostile),
                comp.salvagerLeader.Name.ToStringFull.Colorize(ColoredText.NameColor),
                shipName.Colorize(ColoredText.NameColor),
                comp.salvagerTributeAmount.ToString().Colorize(ColoredText.CurrencyColor),
                comp.salvagerDelayDays);
            if (jammer != null)
            {
                desc += "\n\n" + "VGE_JammerScrambledSalvager".Translate();
            }
            return desc;
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

        public override void OnEarlyEscape(Map map)
        {
            base.OnEarlyEscape(map);
            var parms = PawnsArrivalModeWorker_SalvagerDropshipRaid.CreateRaidParms(map);
            Find.Storyteller.incidentQueue.Add(IncidentDefOf.RaidEnemy, Find.TickManager.TicksGame + Rand.RangeInclusive(3, 5) * GenDate.TicksPerDay, parms);
        }
    }
}
