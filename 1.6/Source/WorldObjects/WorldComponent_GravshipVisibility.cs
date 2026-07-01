using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HotSwappable]
    public class WorldComponent_GravshipVisibility : WorldComponent
    {
        public float visibility;
        private bool warned400k;
        private bool warned600k;
        private bool detectionImminent;
        private int ticksToDetection = -1;

        public static WorldComponent_GravshipVisibility Instance;

        public WorldComponent_GravshipVisibility(World world) : base(world)
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
            Scribe_Values.Look(ref ticksToDetection, "ticksToDetection", -1);
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            if (visibility > 0)
            {
                visibility -= 112f / 2500f;
                if (visibility < 0f) visibility = 0f;

                if (visibility < 400000f) warned400k = false;
                if (visibility < 600000f) warned600k = false;
            }

            if (detectionImminent)
            {
                ticksToDetection--;
                if (ticksToDetection <= 0)
                {
                    TriggerEncounter();
                }
            }
        }

        public void AddVisibility(float baseAmount)
        {
            var engine = Current.Game.Gravship?.Engine;
            engine ??= Find.Maps.SelectMany(m => m.listerBuildings.allBuildingsColonist).FirstOrDefault(x => x is Building_GravEngine) as Building_GravEngine;
            if (engine == null) return;
            var factor = engine.GetStatValue(InternalDefOf.VGE_GravshipVisibilityFactor);
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
                var jammerDelayed = false;
                var jammer = engine.AffectedByFacilities.LinkedFacilitiesListForReading.OfType<Building_SignalJammer>().FirstOrDefault(x => !x.OnCooldown);

                if (jammer != null)
                {
                    jammer.StartCooldown();
                    jammerDelayed = true;
                }

                detectionImminent = true;
                ticksToDetection = jammerDelayed ? 60000 : 0;
            }
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

        private void TriggerEncounter()
        {
            detectionImminent = false;
            visibility = 0f;
            warned400k = false;
            warned600k = false;
            // todo: we need to trigger some orbital encounters, leaving it as a stub
        }
    }
}
