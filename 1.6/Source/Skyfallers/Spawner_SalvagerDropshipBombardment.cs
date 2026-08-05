using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class Spawner_SalvagerDropshipBombardment : Thing, IThingHolder
    {
        public ThingOwner innerContainer;
        public IntVec3 targetCell;
        public int totalRockets;
        public int rocketsFired;
        public Projectile lastRocket;
        public Rot4 shuttleRotation;

        public Spawner_SalvagerDropshipBombardment()
        {
            innerContainer = new ThingOwner<Thing>(this);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                totalRockets = Rand.RangeInclusive(5, 12);
                var dir = (targetCell - Position).AngleFlat;
                shuttleRotation = Rot4.FromAngleFlat(dir);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_Values.Look(ref targetCell, "targetCell");
            Scribe_Values.Look(ref totalRockets, "totalRockets");
            Scribe_Values.Look(ref rocketsFired, "rocketsFired");
            Scribe_References.Look(ref lastRocket, "lastRocket");
            Scribe_Values.Look(ref shuttleRotation, "shuttleRotation");
        }

        public override void Tick()
        {
            base.Tick();
            if (rocketsFired < totalRockets)
            {
                if (this.IsHashIntervalTick(7))
                {
                    var c = targetCell + GenRadial.RadialPattern[Rand.Range(0, GenRadial.NumCellsInRadius(12.9f))];
                    if (!c.InBounds(Map)) c = targetCell;
                    var proj = ProjectileUtil.LaunchProjectile(ProjectileUtil.CreateDummyLauncher(InternalDefOf.VGE_SalvagerDropship, Faction.OfSalvagers), InternalDefOf.Proj_Rocket, Map, Position, c);
                    rocketsFired++;
                    lastRocket = proj;
                }
            }
            else if (lastRocket.DestroyedOrNull())
            {
                SpawnShuttle();
                Destroy();
            }
        }

        private void SpawnShuttle()
        {
            var angleFlat = (targetCell - Position).AngleFlat;
            var rot = Rot4.FromAngleFlat(angleFlat);

            if (TryFindLandingSpotNear(targetCell, Map, out var clearSpot, InternalDefOf.VGE_SalvagerDropship, rot))
            {
                targetCell = clearSpot;
                angleFlat = (targetCell - Position).AngleFlat;
                rot = Rot4.FromAngleFlat(angleFlat);
            }

            var ship = TransportShipMaker.MakeTransportShip(InternalDefOf.VGE_Ship_SalvagerDropship, null);
            ship.shipThing.Rotation = rot;
            ship.TransporterComp.innerContainer.TryAddRangeOrTransfer(innerContainer.ToList(), true, true);
            ship.ArriveAt(targetCell, Map.Parent);

            var incoming = (SalvagerDropshipIncoming)targetCell.GetFirstThing<Skyfaller>(Map);
            incoming.Rotation = rot;
            incoming.exactAngle = angleFlat;
            incoming.startCell = Position;

            var unload = (ShipJob_Unload)ShipJobMaker.MakeShipJob(ShipJobDefOf.Unload);
            unload.dropMode = TransportShipDropMode.All;
            ship.AddJob(unload);
            ship.AddJob(ShipJobDefOf.FlyAway);
        }

        public ThingOwner GetDirectlyHeldThings() => innerContainer;

        public void GetChildHolders(List<IThingHolder> outChildren) => ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());

        public static bool TryFindLandingSpotNear(IntVec3 center, Map map, out IntVec3 result, ThingDef dropshipDef, Rot4 rot)
        {
            foreach (var cell in GenRadial.RadialCellsAround(center, 35f, false))
            {
                if (CanLandAt(cell, map, dropshipDef, rot, strictPlants: true))
                {
                    result = cell;
                    return true;
                }
            }

            foreach (var cell in GenRadial.RadialCellsAround(center, 50f, false))
            {
                if (CanLandAt(cell, map, dropshipDef, rot, strictPlants: false))
                {
                    result = cell;
                    return true;
                }
            }

            result = center;
            return false;
        }

        private static bool CanLandAt(IntVec3 cell, Map map, ThingDef dropshipDef, Rot4 rot, bool strictPlants)
        {
            var occupiedRect = GenAdj.OccupiedRect(cell, rot, dropshipDef.size);

            foreach (var c in occupiedRect)
            {
                if (!c.InBounds(map))
                {
                    return false;
                }

                if (!c.Standable(map))
                {
                    return false;
                }

                if (c.Roofed(map))
                {
                    return false;
                }

                if (c.GetTerrain(map).IsWater)
                {
                    return false;
                }

                var thingList = c.GetThingList(map);
                for (int i = 0; i < thingList.Count; i++)
                {
                    var t = thingList[i];

                    if (t.def.category == ThingCategory.Building)
                    {
                        return false;
                    }

                    if (t.def.category == ThingCategory.Item)
                    {
                        return false;
                    }

                    if (strictPlants && t.def.category == ThingCategory.Plant)
                    {
                        return false;
                    }

                    if (t is Skyfaller || t is IActiveTransporter)
                    {
                        return false;
                    }
                }
            }

            if (ModsConfig.RoyaltyActive)
            {
                var list = map.listerThings.ThingsOfDef(ThingDefOf.ActivatorProximity);
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j].Faction != null && list[j].Faction.HostileTo(Faction.OfSalvagers))
                    {
                        var compSendSignalOnMotion = list[j].TryGetComp<CompSendSignalOnMotion>();
                        if (compSendSignalOnMotion != null)
                        {
                            var radius = compSendSignalOnMotion.Props.radius + 10f;
                            foreach (var c in occupiedRect)
                            {
                                if (c.InHorDistOf(list[j].Position, radius))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}
