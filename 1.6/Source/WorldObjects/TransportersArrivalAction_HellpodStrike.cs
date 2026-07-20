using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class TransportersArrivalAction_HellpodStrike : TransportersArrivalAction
    {
        private MapParent mapParent;
        private Faction launchingFaction;
        private PawnsArrivalModeDef arrivalMode;

        public override bool GeneratesMap => true;

        public TransportersArrivalAction_HellpodStrike() { }

        public TransportersArrivalAction_HellpodStrike(MapParent mapParent, Faction launchingFaction, PawnsArrivalModeDef arrivalMode)
        {
            this.mapParent = mapParent;
            this.launchingFaction = launchingFaction;
            this.arrivalMode = arrivalMode;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref mapParent, "mapParent");
            Scribe_References.Look(ref launchingFaction, "launchingFaction");
            Scribe_Defs.Look(ref arrivalMode, "arrivalMode");
        }

        public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, PlanetTile destinationTile)
        {
            var report = base.StillValid(pods, destinationTile);
            if (!report)
            {
                return report;
            }
            if (mapParent != null && mapParent.Tile != destinationTile)
            {
                return false;
            }
            return true;
        }

        public override bool ShouldUseLongEvent(List<ActiveTransporterInfo> pods, PlanetTile tile)
        {
            return mapParent == null || !mapParent.HasMap;
        }

        public override void Arrived(List<ActiveTransporterInfo> transporters, PlanetTile tile)
        {
            var size = Find.World.info.initialMapSize;
            if (mapParent.def.overrideMapSize.HasValue)
            {
                size = mapParent.def.overrideMapSize.Value;
            }
            var map = GetOrGenerateMapUtility.GetOrGenerateMap(mapParent.Tile, size, mapParent.def);
            IntVec3 destCell;
            if (arrivalMode == PawnsArrivalModeDefOf.CenterDrop)
            {
                if (!DropCellFinder.TryFindRaidDropCenterClose(out destCell, map))
                {
                    destCell = DropCellFinder.FindRaidDropCenterDistant(map);
                }
            }
            else
            {
                destCell = DropCellFinder.FindRaidDropCenterDistant(map, false, !transporters.IsShuttle());
            }
            foreach (var trans in transporters)
            {
                var edgeCell = CellFinder.RandomEdgeCell(map);
                var projectile = (Projectile_Warpod)ThingMaker.MakeThing(InternalDefOf.VGE_WarpodIncoming);
                projectile.warpodDef = trans.sentTransporterDef;
                projectile.launchingFaction = launchingFaction;
                projectile.innerContainer.TryAddRangeOrTransfer(trans.innerContainer, true, true);
                var dummyLauncher = projectile.CreateDummyLauncher();
                GenSpawn.Spawn(projectile, edgeCell, map);
                projectile.Launch(dummyLauncher, edgeCell.ToVector3Shifted(), new LocalTargetInfo(destCell), new LocalTargetInfo(destCell), ProjectileHitFlags.All);
            }
        }
    }
}
