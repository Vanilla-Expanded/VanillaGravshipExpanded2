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
                ProjectileUtil.LaunchWarpod(trans, InternalDefOf.VGE_WarpodIncoming, launchingFaction, map, edgeCell, cell);
            }
        }
    }
}
