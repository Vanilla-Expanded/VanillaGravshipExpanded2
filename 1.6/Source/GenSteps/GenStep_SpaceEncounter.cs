using Verse;

namespace VanillaGravshipExpanded2
{
    public abstract class GenStep_SpaceEncounter : GenStep
    {
        public override void Generate(Map map, GenStepParams parms)
        {
            Rand.PushState(Find.TickManager.TicksGame ^ map.uniqueID ^ SeedPart);
            map.regionAndRoomUpdater.Enabled = true;
            try
            {
                GenerateSpaceMap(map, parms);
            }
            finally
            {
                Rand.PopState();
            }
        }

        protected abstract void GenerateSpaceMap(Map map, GenStepParams parms);
    }
}
