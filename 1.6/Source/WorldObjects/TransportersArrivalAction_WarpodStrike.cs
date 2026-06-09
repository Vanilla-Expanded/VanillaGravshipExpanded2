using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class TransportersArrivalAction_WarpodStrike : TransportersArrivalAction
    {
        private MapParent mapParent;
        private IntVec3 cell;
        private Faction launchingFaction;

        public override bool GeneratesMap => false;

        public TransportersArrivalAction_WarpodStrike() { }

        public TransportersArrivalAction_WarpodStrike(MapParent mapParent, IntVec3 cell, Faction launchingFaction)
        {
            this.mapParent = mapParent;
            this.cell = cell;
            this.launchingFaction = launchingFaction;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref mapParent, "mapParent");
            Scribe_Values.Look(ref cell, "cell");
            Scribe_References.Look(ref launchingFaction, "launchingFaction");
        }

        public override void Arrived(List<ActiveTransporterInfo> transporters, PlanetTile tile)
        {
            foreach (var trans in transporters)
            {
                var map = mapParent.Map;
                var edgeCell = CellFinder.RandomEdgeCell(map);
                var projectile = (Projectile_Warpod)ThingMaker.MakeThing(InternalDefOf.VGE_WarpodIncoming);
                projectile.warpodDef = trans.sentTransporterDef;
                projectile.launchingFaction = launchingFaction;
                projectile.innerContainer.TryAddRangeOrTransfer(trans.innerContainer, true, true);
                var dummyLauncher = projectile.CreateDummyLauncher();
                GenSpawn.Spawn(projectile, edgeCell, map);
                projectile.Launch(dummyLauncher, edgeCell.ToVector3Shifted(), new LocalTargetInfo(cell), new LocalTargetInfo(cell), ProjectileHitFlags.All);
            }
        }
    }
}
