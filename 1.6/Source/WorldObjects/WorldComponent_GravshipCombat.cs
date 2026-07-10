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
                    SpawnActiveWarplatform();
                }
            }
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
            activeThreatDef.Worker.SpawnThreat();
        }
    }
}
