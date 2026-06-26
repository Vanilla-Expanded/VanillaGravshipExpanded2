using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded2;

public class TransportersArrivalAction_EscapePod : TransportersArrivalAction
{
    protected Faction faction;

    public override bool GeneratesMap => false;

    public TransportersArrivalAction_EscapePod()
    {
    }

    public TransportersArrivalAction_EscapePod(Faction faction) : this()
    {
        this.faction = faction;
    }

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_References.Look(ref faction, nameof(faction));
    }

    public override void Arrived(List<ActiveTransporterInfo> pods, PlanetTile tile)
    {
        TryDropCaravanOrExistingMap(pods, tile, true);
    }

    public override FloatMenuAcceptanceReport StillValid(IEnumerable<IThingHolder> pods, PlanetTile tile)
    {
        var podsList = pods as List<ActiveTransporterInfo> ?? pods.OfType<ActiveTransporterInfo>().ToList();
        return TryDropCaravanOrExistingMap(podsList, tile, false);
    }

    protected bool TryDropCaravanOrExistingMap(List<ActiveTransporterInfo> pods, PlanetTile tile, bool arrive)
    {
        foreach (var map in Find.Maps)
        {
            if (TryDropInRandomCell(pods, tile, map, faction, arrive))
                return true;
        }

        foreach (var caravan in Find.WorldObjects.Caravans)
        {
            if (TryJoinCaravan(pods, tile, caravan, arrive))
                return true;
        }

        return TryFormCaravan(pods, tile, arrive);
    }

    protected static bool TryDropInRandomCell(List<ActiveTransporterInfo> transporters, PlanetTile targetTile, Map map, Faction faction, bool arrive)
    {
        if (map.Tile != targetTile)
            return false;

        if (!DropCellFinder.FindSafeLandingSpot(out var cell, faction, map, 25, 5, 5))
            cell = DropCellFinder.RandomDropSpot(map);
        if (!cell.IsValid)
            return false;

        var arrival = new TransportersArrivalAction_LandInSpecificCell(map.Parent, cell);
        if (!arrival.StillValid(transporters, targetTile))
            return false;

        if (arrive)
            arrival.Arrived(transporters, targetTile);
        return true;
    }

    protected static bool TryJoinCaravan(List<ActiveTransporterInfo> transporters, PlanetTile targetTile, Caravan caravan, bool arrive)
    {
        if (caravan.Tile != targetTile)
            return false;

        var action = new TransportersArrivalAction_GiveToCaravan(caravan);
        if (!action.StillValid(transporters, targetTile))
            return false;

        if (arrive)
            action.Arrived(transporters, targetTile);
        return true;
    }

    protected static bool TryFormCaravan(List<ActiveTransporterInfo> transporters, PlanetTile targetTile, bool arrive)
    {
        var action = new TransportersArrivalAction_FormCaravan();
        if (!action.StillValid(transporters, targetTile))
            return false;

        if (arrive)
            action.Arrived(transporters, targetTile);
        return true;
    }
}