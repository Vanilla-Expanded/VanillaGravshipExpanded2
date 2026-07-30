using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HotSwappable]
    public class WorldComponent_GravshipCombat : WorldComponent
    {
        public float visibility;
        private bool warned400k;
        private bool warned600k;
        private bool detectionImminent;
        private int detectionTick = -1;
        public bool incomingWarplatform;
        public int warplatformTick = -1;
        public GravshipThreatDef activeThreatDef;
        public int salvagerTributeAmount;
        public int salvagerDelayDays;
        public int tributeDemandTick = -1;
        public bool engineLockedRemotely;
        public string salvagerStationName;
        public Pawn salvagerLeader;
        public int salvagerDropshipTick = -1;

        public static WorldComponent_GravshipCombat Instance;

        public static Building_GravEngine GetActiveGravEngine
        {
            get
            {
                foreach (var map in Current.Game.Maps)
                {
                    var engine = GravshipUtility.GetPlayerGravEngine_NewTemp(map);
                    if (engine != null)
                    {
                        return engine;
                    }
                }
                return Current.Game.Gravship?.Engine;
            }
        }

        public WorldComponent_GravshipCombat(World world) : base(world)
        {
            Instance = this;
        }

        public override void ExposeData()
        {
            Instance = this;
            base.ExposeData();
            Scribe_Values.Look(ref visibility, "visibility", 0f);
            Scribe_Values.Look(ref warned400k, "warned400k", false);
            Scribe_Values.Look(ref warned600k, "warned600k", false);
            Scribe_Values.Look(ref detectionImminent, "detectionImminent", false);
            Scribe_Values.Look(ref detectionTick, "detectionTick", -1);
            Scribe_Values.Look(ref incomingWarplatform, "incomingWarplatform", false);
            Scribe_Values.Look(ref warplatformTick, "warplatformTick", -1);
            Scribe_Defs.Look(ref activeThreatDef, "activeThreatDef");
            Scribe_Values.Look(ref salvagerTributeAmount, "salvagerTributeAmount", 0);
            Scribe_Values.Look(ref salvagerDelayDays, "salvagerDelayDays", 0);
            Scribe_Values.Look(ref tributeDemandTick, "tributeDemandTick", -1);
            Scribe_Values.Look(ref engineLockedRemotely, "engineLockedRemotely", false);
            Scribe_Values.Look(ref salvagerStationName, "salvagerStationName");
            Scribe_Deep.Look(ref salvagerLeader, "salvagerLeader");
            Scribe_Values.Look(ref salvagerDropshipTick, "salvagerDropshipTick", -1);
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();
            if (visibility > 0)
            {
                RemoveVisibility(112f / 2500f);
            }

            if (incomingWarplatform)
            {
                if (Find.TickManager.TicksGame >= warplatformTick)
                {
                    if (activeThreatDef != InternalDefOf.VGE_SalvagerStation) SpawnActiveWarplatform();
                }
            }

            if (tributeDemandTick > 0 && Find.TickManager.TicksGame >= tributeDemandTick)
            {
                ShowTributeDemandDialog(engineLockedRemotely);
                tributeDemandTick = -1;
            }

            if (salvagerDropshipTick > 0 && Find.TickManager.TicksGame >= salvagerDropshipTick)
            {
                var engine = GetActiveGravEngine;
                if (engine?.Map != null)
                {
                    salvagerDropshipTick = -1;
                    var parms = new IncidentParms
                    {
                        target = engine.Map,
                        faction = Faction.OfSalvagers,
                        points = StorytellerUtility.DefaultThreatPointsNow(engine.Map),
                        raidArrivalMode = InternalDefOf.VGE_SalvagerDropshipRaid
                    };
                    IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
                }
            }
        }

        public void PayTribute(Map map)
        {
            TradeUtility.LaunchSilver(map, salvagerTributeAmount);
            RemoveVisibility(visibility);
            incomingWarplatform = false;
            engineLockedRemotely = false;
            tributeDemandTick = -1;
            salvagerLeader.Destroy();
            salvagerLeader = null;
        }

        public DiaOption GetPayTributeOption(Map map)
        {
            var leaderName = salvagerLeader.Name.ToStringFull.Colorize(ColoredText.NameColor);
            var coloredTribute = salvagerTributeAmount.ToString().Colorize(ColoredText.CurrencyColor);
            var payOpt = new DiaOption("VGE_PaySilver".Translate(coloredTribute))
            {
                link = new DiaNode("VGE_TributePaidReply".Translate(leaderName))
                {
                    options =
                    {
                        DiaOption.DefaultOK
                    }
                },
                action = () => PayTribute(map)
            };
            if (!TradeUtility.ColonyHasEnoughSilver(map, salvagerTributeAmount))
            {
                payOpt.Disable("NeedSilverLaunchable".Translate(salvagerTributeAmount));
            }
            return payOpt;
        }

        private void ShowTributeDemandDialog(bool postponed)
        {
            var engine = GetActiveGravEngine;
            if (engine == null) return;
            var map = engine.Map;

            var leaderName = salvagerLeader.Name.ToStringFull.Colorize(ColoredText.NameColor);
            var coloredStation = salvagerStationName.Colorize(ColoredText.FactionColor_Hostile);
            var coloredTribute = salvagerTributeAmount.ToString().Colorize(ColoredText.CurrencyColor);
            var node = new DiaNode(postponed ? "VGE_PostponedTributeDemandPopup".Translate(leaderName, coloredStation, coloredTribute) : "VGE_TributeDemandPopup".Translate(leaderName, coloredStation, coloredTribute, salvagerDelayDays));

            node.options.Add(GetPayTributeOption(map));

            if (!postponed)
            {
                node.options.Add(new DiaOption("VGE_PostponeTribute".Translate(salvagerDelayDays))
                {
                    link = new DiaNode("VGE_PostponeReply".Translate(leaderName, salvagerDelayDays))
                    {
                        options =
                        {
                            DiaOption.DefaultOK
                        }
                    },
                    action = () =>
                    {
                        engineLockedRemotely = true;
                        if (engine.cooldownCompleteTick < Find.TickManager.TicksGame) engine.cooldownCompleteTick = Find.TickManager.TicksGame + salvagerDelayDays * GenDate.TicksPerDay;
                        tributeDemandTick = Find.TickManager.TicksGame + salvagerDelayDays * GenDate.TicksPerDay;
                    }
                });
            }

            node.options.Add(new DiaOption("VGE_RefuseTribute".Translate())
            {
                link = new DiaNode("VGE_RefuseReply".Translate(leaderName, coloredStation))
                {
                    options =
                    {
                        DiaOption.DefaultOK
                    }
                },
                action = () => SpawnActiveWarplatform()
            });

            Find.WindowStack.Add(new Dialog_NodeTree(node, true));
        }

        public void AddVisibility(float baseAmount, bool isLaunch = false)
        {
            var engine = GetActiveGravEngine;
            if (engine == null) return;
            var factor = engine.GetStatValue(InternalDefOf.VGE_GravshipVisibilityFactor);
            if (isLaunch)
                factor += engine.GetStatValue(InternalDefOf.VGE_GravshipLaunchVisibilityOffset);

            visibility += baseAmount * factor;
            if (visibility >= 400000f && !warned400k)
            {
                warned400k = true;
                Messages.Message("VGE_Visibility400k".Translate(), MessageTypeDefOf.CautionInput, false);
            }

            if (visibility >= 600000f && !warned600k)
            {
                warned600k = true;
                Send600kLetter(engine);
            }
            if (visibility >= 800000f && !detectionImminent)
            {
                detectionImminent = true;
                TriggerEncounter();
            }
        }

        public void RemoveVisibility(float amount)
        {
            visibility -= amount;
            if (visibility < 0f) visibility = 0f;
            if (visibility < 400000f) warned400k = false;
            if (visibility < 600000f) warned600k = false;
            if (visibility < 800000f) detectionImminent = false;
        }

        private void Send600kLetter(Building_GravEngine engine)
        {
            var text = "VGE_Visibility600kDesc".Translate(engine.RenamableLabel, GetArmamentsList());
            Find.LetterStack.ReceiveLetter("VGE_Visibility600k".Translate(), text, LetterDefOf.ThreatSmall);
        }

        private string GetArmamentsList()
        {
            var counts = new Dictionary<string, int>();

            void AddCount(string label)
            {
                if (counts.ContainsKey(label)) counts[label]++;
                else counts[label] = 1;
            }

            var allThings = new List<Thing>();
            if (Current.Game.Gravship != null)
            {
                allThings.AddRange(Current.Game.Gravship.Things);
            }
            foreach (var map in Find.Maps)
            {
                allThings.AddRange(map.listerBuildings.allBuildingsColonist);
            }

            foreach (var t in allThings.Distinct())
            {
                if (t.def.HasModExtension<VisibilityGainExtension>())
                {
                    AddCount(t.def.label);
                }
                else if (t.TryGetComp<CompTransporter_Warpod>() != null)
                {
                    AddCount("VGE_Warpods".Translate());
                }
            }

            if (counts.Count == 0) return "VGE_NoArmamentsPresent".Translate();

            return counts.Select(kvp => $"{kvp.Value}x {kvp.Key}").ToLineList();
        }

        public void TriggerEncounter()
        {
            var engine = GetActiveGravEngine;
            if (engine is null) return;
            var validThreats = DefDatabase<GravshipThreatDef>.AllDefsListForReading.Where(x => x.Worker.CanFire(engine));
            if (validThreats.TryRandomElementByWeight(x => x.weight, out var selected))
            {
                activeThreatDef = selected;
                selected.Worker.Fire(engine);
            }
        }

        public void SpawnActiveWarplatform()
        {
            incomingWarplatform = false;
            tributeDemandTick = -1;
            activeThreatDef.Worker.SpawnThreat();
        }
    }
}
