using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;

namespace VanillaGravshipExpanded2;

public class TransportersArrivalAction_DestroyOrPassToWorld : TransportersArrivalAction
{
    public override bool GeneratesMap => false;

    public override void Arrived(List<ActiveTransporterInfo> transporters, PlanetTile tile)
    {
        foreach (var transporter in transporters)
            transporter.GetDirectlyHeldThings().ClearAndDestroyContentsOrPassToWorld();
    }
}