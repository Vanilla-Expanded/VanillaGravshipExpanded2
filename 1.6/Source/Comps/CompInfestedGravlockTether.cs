using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded2
{
    public class CompInfestedGravlockTether : ThingComp
    {
        private int ticksToNextSpawn;

        public CompProperties_InfestedGravlockTether Props => (CompProperties_InfestedGravlockTether)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                ticksToNextSpawn = Props.spawnIntervalTicks;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (!parent.Spawned) return;

            ticksToNextSpawn--;
            if (ticksToNextSpawn <= 0)
            {
                SpawnInsect();
                ticksToNextSpawn = Props.spawnIntervalTicks;
            }
        }

        private void SpawnInsect()
        {
            var map = parent.Map;
            var vents = map.listerThings.ThingsOfDef(InternalDefOf.VGE_InfestedVent);
            var spawnCell = parent.Position;
            if (vents.Count > 0)
            {
                spawnCell = vents.RandomElement().Position;
            }
            var insectKinds = new List<(PawnKindDef kind, float weight)>
            {
                (InternalDefOf.VGE_Exoworm, 3f),
                (InternalDefOf.VGE_Exopede, 2f),
                (InternalDefOf.VGE_Exoleech, 2f),
                (InternalDefOf.VGE_ExowormKnot, 2f)
            };
            var kindToSpawn = insectKinds.RandomElementByWeight(x => x.weight).kind;
            var pawn = PawnGenerator.GeneratePawn(kindToSpawn, Faction.OfInsects);
            GenSpawn.Spawn(pawn, spawnCell, map);
            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter);
            InternalDefOf.Hive_Spawn.PlayOneShot(new TargetInfo(spawnCell, map));
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ticksToNextSpawn, "ticksToNextSpawn", 0);
        }
    }
}
