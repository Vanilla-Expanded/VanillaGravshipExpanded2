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
        public string enemyGravshipName;
        public int gravjumperLandingTick = -1;
        public string gravjumperLandingName;
        public bool gravjumperLandedLocal;

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
            Scribe_Values.Look(ref enemyGravshipName, "enemyGravshipName");
            Scribe_Values.Look(ref gravjumperLandingTick, "gravjumperLandingTick", -1);
            Scribe_Values.Look(ref gravjumperLandingName, "gravjumperLandingName");
            Scribe_Values.Look(ref gravjumperLandedLocal, "gravjumperLandedLocal", false);
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
                    var parms = PawnsArrivalModeWorker_SalvagerDropshipRaid.CreateRaidParms(engine.Map);
                    IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
                }
            }

            if (gravjumperLandingTick > 0 && Find.TickManager.TicksGame >= gravjumperLandingTick)
            {
                gravjumperLandingTick = -1;
                LandGravjumperOnPlayerMap();
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

        public void AddVisibility(float baseAmount, bool isLaunch = false, bool applyFactors = true)
        {
            var engine = GetActiveGravEngine;
            if (engine == null) return;

            var factor = 1f;
            if (applyFactors)
            {
                factor = engine.GetStatValue(InternalDefOf.VGE_GravshipVisibilityFactor);
                if (isLaunch)
                    factor += engine.GetStatValue(InternalDefOf.VGE_GravshipLaunchVisibilityOffset);
            }

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

        private void LandGravjumperOnPlayerMap()
        {
            var map = GetActiveGravEngine.Map;
            var setDef = Rand.Chance(0.2f) ? InternalDefOf.VGE_EnemyGravjumperSet_Complete : InternalDefOf.VGE_EnemyGravjumperSet;
            var standardLayouts = VEF.Storyteller.StructureSetGenerator.SelectStandardLayouts(setDef);
            var engine = GetActiveGravEngine;
            var rotation = GetFacingRotation(map.Center, engine != null ? engine.Position : map.Center);
            var footprint = VEF.Storyteller.StructureSetGenerator.GetFootprint(standardLayouts, rotation);
            var landingCell = FindBestGravjumperLandingSpot(map, footprint);

            var landingStructure = (LandingStructure_StructureSet)ThingMaker.MakeThing(InternalDefOf.VGE_LandingStructure_EnemyGravjumper);
            landingStructure.structureSetDef = setDef;
            landingStructure.selectedDefs = standardLayouts.Select(x => x.def).ToList();
            landingStructure.shipRotation = rotation;
            landingStructure.shipFaction = Faction.OfSalvagers;
            landingStructure.pawnCountRange = new IntRange(5, 8);
            GenSpawn.Spawn(landingStructure, landingCell, map);
            gravjumperLandedLocal = true;
        }

        private static Rot4 GetFacingRotation(IntVec3 from, IntVec3 to)
        {
            return Rot4.FromAngleFlat((to - from).AngleFlat);
        }

        private static IntVec3 FindBestGravjumperLandingSpot(Map map, IntVec2 footprint)
        {
            var minX = 5 + footprint.x / 2;
            var maxX = map.Size.x - 5 - footprint.x / 2;
            var minZ = 5 + footprint.z / 2;
            var maxZ = map.Size.z - 5 - footprint.z / 2;

            var bestCell = map.Center;
            var bestScore = float.MinValue;
            var perfectSpots = new List<IntVec3>();

            for (var x = minX; x <= maxX; x += 3)
            {
                for (var z = minZ; z <= maxZ; z += 3)
                {
                    var center = new IntVec3(x, 0, z);
                    if (center.Fogged(map)) continue;

                    var rect = CellRect.CenteredOn(center, footprint.x, footprint.z);
                    if (!rect.FullyContainedWithin(new CellRect(5, 5, map.Size.x - 10, map.Size.z - 10)))
                        continue;

                    var score = EvaluateLandingSpotScore(rect, map);

                    if (score == 0f)
                    {
                        perfectSpots.Add(center);
                    }
                    else if (perfectSpots.Count == 0 && score > bestScore)
                    {
                        bestScore = score;
                        bestCell = center;
                    }
                }
            }

            if (perfectSpots.Count > 0)
            {
                return perfectSpots.RandomElement();
            }

            return bestCell;
        }

        private static float EvaluateLandingSpotScore(CellRect rect, Map map)
        {
            var score = 0f;
            foreach (var cell in rect)
            {
                if (cell.Fogged(map)) score -= 1000f;
                if (cell.Roofed(map)) score -= 500f;
                if (map.areaManager.Home[cell]) score -= 200f;

                if (!cell.Standable(map)) score -= 100f;

                var edifice = cell.GetEdifice(map);
                if (edifice != null)
                {
                    if (edifice.def.building?.isNaturalRock == true) score -= 80f;
                    else score -= 150f;
                }

                var thingList = cell.GetThingList(map);
                for (var i = 0; i < thingList.Count; i++)
                {
                    var t = thingList[i];
                    if (t is Building && t != edifice) score -= 50f;
                    if (t is Pawn) score -= 30f;
                }
            }
            return score;
        }

        public void CheckLocalGravjumperDefeated(Map map)
        {
            if (!gravjumperLandedLocal) return;

            if (map.listerThings.ThingsOfDef(InternalDefOf.VGE_LandingStructure_EnemyGravjumper).Any()) return;

            var engineExists = map.listerThings.ThingsOfDef(InternalDefOf.VGE_EnemyGravjumperEngine).Any(x => !x.Destroyed);
            var turretsExist = map.listerThings.ThingsInGroup(ThingRequestGroup.BuildingArtificial)
                .Any(x => x.Faction == Faction.OfSalvagers && x is VanillaGravshipExpanded.Building_GravshipTurret && !x.Destroyed);

            if (!engineExists && !turretsExist)
            {
                gravjumperLandedLocal = false;
                Find.LetterStack.ReceiveLetter("VGE_GravjumperDefeated".Translate(), "VGE_GravjumperDefeatedDesc".Translate(), LetterDefOf.PositiveEvent);
            }
        }
    }
}
