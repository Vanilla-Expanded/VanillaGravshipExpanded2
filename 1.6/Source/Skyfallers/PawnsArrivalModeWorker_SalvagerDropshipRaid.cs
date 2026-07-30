using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class PawnsArrivalModeWorker_SalvagerDropshipRaid : PawnsArrivalModeWorker
    {
        public override bool CanUseWith(IncidentParms parms)
        {
            return base.CanUseWith(parms) && parms.faction != null && parms.faction == Faction.OfSalvagers;
        }

        public override void Arrive(List<Pawn> pawns, IncidentParms parms)
        {
            var map = (Map)parms.target;
            var targetCell = parms.spawnCenter;
            if (!CellFinder.TryFindRandomEdgeCellWith(c => c.Standable(map) && !c.Roofed(map), map, CellFinder.EdgeRoadChance_Ignore, out var edgeCell))
            {
                edgeCell = DropCellFinder.RandomDropSpot(map);
            }

            var spawner = (Spawner_SalvagerDropshipBombardment)ThingMaker.MakeThing(InternalDefOf.VGE_SalvagerDropshipBombardment);
            spawner.targetCell = targetCell;

            spawner.innerContainer.TryAddRangeOrTransfer(pawns, canMergeWithExistingStacks: false, destroyLeftover: false);
            GenSpawn.Spawn(spawner, edgeCell, map);
        }

        public override bool TryResolveRaidSpawnCenter(IncidentParms parms)
        {
            var map = (Map)parms.target;
            var homeCells = map.areaManager.Home.ActiveCells.Where(c => !c.Roofed(map) && c.Standable(map)).ToList();
            if (homeCells.Any()) parms.spawnCenter = homeCells.RandomElement();
            else parms.spawnCenter = DropCellFinder.RandomDropSpot(map);
            var rot = Rot4.FromAngleFlat((map.Center - parms.spawnCenter).AngleFlat);
            if (Spawner_SalvagerDropshipBombardment.TryFindLandingSpotNear(parms.spawnCenter, map, out var result, InternalDefOf.VGE_SalvagerDropship, rot))
            {
                parms.spawnCenter = result;
            }
            return true;
        }
    }
}
