using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class GravshipThreatDef : Def
    {
        public string incomingLabel;
        public float weight;
        public Type workerClass = typeof(GravshipThreatWorker);
        public List<ThingDef> allowedEngineDefs;
        public string letterLabel;
        public string letterDesc;
        public string arrivalLetterLabel;
        public string arrivalLetterDesc;
        public IntRange baseCountdownHours;
        public int jammerExtensionHours;
        public float escapeMidBattleVisibilityLoss;
        public float earlyEscapeVisibilityLoss;
        public int escapeDistance;
        public int despawnHours;
        public int hoursBombardmentOnEngineDestroyed;
        public WorldObjectDef worldObjectDef;
        public FloatRange tileSpawnDistanceRange;
        public int mapSize;
        public List<ThingDef> defeatBuildings;
        public string escapedMessage;
        public string earlyEscapeMessage;
        public string defeatLetter;
        public string defeatLetterDesc;
        public string disengagesLetter;
        public string disengagesLetterDesc;
        public string alertExplanation;
        private GravshipThreatWorker workerInt;

        public GravshipThreatWorker Worker
        {
            get
            {
                if (workerInt == null)
                {
                    workerInt = (GravshipThreatWorker)Activator.CreateInstance(workerClass);
                    workerInt.def = this;
                }
                return workerInt;
            }
        }
    }
}
