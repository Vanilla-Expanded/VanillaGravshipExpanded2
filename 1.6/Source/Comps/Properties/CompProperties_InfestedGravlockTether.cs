using Verse;

namespace VanillaGravshipExpanded2
{
    public class CompProperties_InfestedGravlockTether : CompProperties
    {
        public int spawnIntervalTicks = 1200;

        public CompProperties_InfestedGravlockTether()
        {
            compClass = typeof(CompInfestedGravlockTether);
        }
    }
}
