using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class IncidentWorker_SpaceInfestation : IncidentWorker
    {
        public override bool TryExecuteWorker(IncidentParms parms)
        {
            var map = (Map)parms.target;
            if (!CellFinderLoose.TryFindRandomNotEdgeCellWith(10, c => c.GetTerrain(map) != TerrainDefOf.Space, map, out var targetCenter))
            {
                return false;
            }
            var points = Mathf.RoundToInt(parms.points);
            var totalChunks = Mathf.Clamp(points / 200, 3, 20);
            var largeChunks = Mathf.RoundToInt(totalChunks / 3f);
            var mediumChunks = totalChunks - largeChunks;
            if (!CellFinder.TryFindRandomEdgeCellWith(c => !c.Roofed(map), map, Rot4.Random, CellFinder.EdgeRoadChance_Ignore, out var spawnCell))
            {
                spawnCell = CellFinder.RandomEdgeCell(Rot4.Random, map);
            }
            var spawner = (Spawner_SpaceInfestation)ThingMaker.MakeThing(InternalDefOf.VGE_SpaceInfestationSpawner);
            spawner.targetCenter = targetCenter;
            spawner.spawnCell = spawnCell;
            spawner.largeChunks = largeChunks;
            spawner.mediumChunks = mediumChunks;
            spawner.totalChunks = totalChunks;
            spawner.ticksToNextSpawn = Rand.RangeInclusive(5000, 30000);
            GenSpawn.Spawn(spawner, targetCenter, map);
            SendStandardLetter(parms, null);
            return true;
        }
    }
}
