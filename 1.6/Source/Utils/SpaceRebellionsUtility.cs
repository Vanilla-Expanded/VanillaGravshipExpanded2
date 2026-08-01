using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded2;

public static class SpaceRebellionsUtility
{
    public static readonly List<ThingDef> ValidEscapePods = [];
    public static readonly List<ThingDef> ValidTransportPods = [];

    static SpaceRebellionsUtility()
    {
        foreach (var def in DefDatabase<ThingDef>.AllDefs)
        {
            if (def.building == null)
                continue;

            if (def.HasComp<CompEscapePod>())
                ValidEscapePods.Add(def);
            else if (def.HasComp<CompLaunchable_TransportPod>() && def.HasComp<CompTransporter>())
                ValidTransportPods.Add(def);
        }

        ValidTransportPods.Capacity = ValidTransportPods.Count;
        ValidEscapePods.Capacity = ValidEscapePods.Count;
    }

    public static bool CanInitiateRebellion(Pawn pawn)
    {
        // Shouldn't be the case... but let's be safe, just in case.
        if (pawn?.Map == null)
            return false;

        for (var i = 0; i < ValidEscapePods.Count; i++)
        {
            var list = pawn.Map.listerThings.ThingsOfDef(ValidEscapePods[i]);
            if (list.Count > 0 && IsEscapePodUsable(list[0].TryGetComp<CompEscapePod>()))
                return true;
        }

        TileFinder.TryFindPassableTileWithTraversalDistance(Find.WorldGrid.Surface.GetClosestTile_NewTemp(pawn.Map.Tile), 0, int.MaxValue, out var closest, null, true, TileFinderMode.Near, true, true);
        var distance = Find.WorldGrid.TraversalDistanceBetween(pawn.Map.Tile, closest, true, int.MaxValue, true);

        for (var i = 0; i < ValidTransportPods.Count; i++)
        {
            var list = pawn.Map.listerThings.ThingsOfDef(ValidTransportPods[i]);
            for (var j = 0; j < list.Count; j++)
            {
                if (IsTransportPodUsable(list[j].TryGetComp<CompLaunchable>(), closest.Layer, distance))
                    return true;
            }
        }

        return false;
    }

    public static void GetValidEscapePods(Map map, List<Thing> escapePods, List<Thing> dropPods)
    {
        if (map == null)
            return;

        if (escapePods != null)
        {
            escapePods.Clear();

            for (var i = 0; i < ValidEscapePods.Count; i++)
            {
                var list = map.listerThings.ThingsOfDef(ValidEscapePods[i]);
                for (var j = 0; j < list.Count; j++)
                {
                    var thing = list[j];
                    if (IsEscapePodUsable(thing.TryGetComp<CompEscapePod>()))
                        escapePods.Add(thing);
                }
            }
        }

        if (dropPods != null)
        {
            // Only clear if the 2 lists aren't the same one
            if (dropPods != escapePods)
                dropPods.Clear();

            var closest = GetClosestTargetTransportPodTile(map);
            var distance = Find.WorldGrid.TraversalDistanceBetween(map.Tile, closest, true, int.MaxValue, true);

            for (var i = 0; i < ValidTransportPods.Count; i++)
            {
                var list = map.listerThings.ThingsOfDef(ValidTransportPods[i]);
                for (var j = 0; j < list.Count; j++)
                {
                    var thing = list[j];
                    if (IsTransportPodUsable(thing.TryGetComp<CompLaunchable>(), closest.Layer, distance))
                        dropPods.Add(thing);
                }
            }
        }
    }

    public static bool IsEscapePodUsable(CompEscapePod escapePod)
    {
        return escapePod is { Occupant: null, IsCurrentPlanetLayerSupported: true } && escapePod.FindClosestValidTile().Valid;
    }

    public static bool IsTransportPodUsable(CompLaunchable transportPod, PlanetLayer targetLayer, int distance)
    {
        if (transportPod?.Transporter == null)
            return false;
        if (transportPod.FuelLevel < transportPod.Props.minFuelCost)
            return false;
        if (transportPod.RequiresFuelingPort && !transportPod.Refuelable.HasFuel)
            return false;
        return distance <= transportPod.MaxLaunchDistanceAtFuelLevel(distance, targetLayer);
    }

    public static PlanetTile GetClosestTargetTransportPodTile(Map map)
    {
        TileFinder.TryFindPassableTileWithTraversalDistance(Find.WorldGrid.Surface.GetClosestTile_NewTemp(map.Tile), 0, int.MaxValue, out var closest, null, true, TileFinderMode.Near, true, true);
        return closest;
    }
}