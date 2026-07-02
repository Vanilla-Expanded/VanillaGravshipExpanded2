using Verse;

namespace VanillaGravshipExpanded2
{
    public class Spawner_SpaceInfestation : Thing
    {
        public int ticksToNextSpawn;
        public int totalChunks;
        public int largeChunks;
        public int mediumChunks;
        public int spawnedChunks;
        public IntVec3 targetCenter;
        public IntVec3 spawnCell;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksToNextSpawn, "ticksToNextSpawn");
            Scribe_Values.Look(ref totalChunks, "totalChunks");
            Scribe_Values.Look(ref largeChunks, "largeChunks");
            Scribe_Values.Look(ref mediumChunks, "mediumChunks");
            Scribe_Values.Look(ref spawnedChunks, "spawnedChunks");
            Scribe_Values.Look(ref targetCenter, "targetCenter");
            Scribe_Values.Look(ref spawnCell, "spawnCell");
        }

        public override void Tick()
        {
            if (spawnedChunks >= totalChunks)
            {
                Destroy();
                return;
            }
            ticksToNextSpawn--;
            if (ticksToNextSpawn <= 0)
            {
                SpawnChunk();
                spawnedChunks++;
                ticksToNextSpawn = Rand.RangeInclusive(20, 80);
            }
        }

        private void SpawnChunk()
        {
            var isLarge = false;
            if (largeChunks > 0 && mediumChunks > 0)
            {
                isLarge = Rand.Chance((float)largeChunks / (largeChunks + mediumChunks));
            }
            else if (largeChunks > 0)
            {
                isLarge = true;
            }
            if (isLarge)
            {
                largeChunks--;
            }
            else
            {
                mediumChunks--;
            }
            var chunkDef = isLarge ? InternalDefOf.VGE_Projectile_InfestedChunkLarge : InternalDefOf.VGE_Projectile_InfestedChunkMedium;
            var targetCell = targetCenter + GenRadial.RadialPattern[Rand.Range(0, GenRadial.NumCellsInRadius(10f))];
            var proj = (Projectile_InfestedChunk)GenSpawn.Spawn(chunkDef, spawnCell, Map);
            proj.isLarge = isLarge;
            proj.Launch(null, spawnCell.ToVector3Shifted(), new LocalTargetInfo(targetCell), new LocalTargetInfo(targetCell), ProjectileHitFlags.All);
        }
    }
}
