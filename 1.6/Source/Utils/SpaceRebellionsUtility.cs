using System.Collections.Generic;
using System.Linq;
using System.Text;
using LudeonTK;
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

        if (GetAllUsableEscapePods(pawn.Map).Any())
            return true;

        var closest = GetClosestTargetTransportPodTile(pawn.Map);
        var distance = Find.WorldGrid.TraversalDistanceBetween(pawn.Map.Tile, closest, true, int.MaxValue, true);

        return GetAllUsableDropPods(pawn.Map, closest.Layer, distance).Any();
    }

    public static void GetAllValidPods(Map map, List<Thing> escapePods, List<Thing> dropPods)
    {
        if (map == null)
            return;

        if (escapePods != null)
        {
            escapePods.Clear();
            escapePods.AddRange(GetAllUsableEscapePods(map).Select(x => x.parent));
        }

        if (dropPods != null)
        {
            // Only clear if the 2 lists aren't the same one
            if (dropPods != escapePods)
                dropPods.Clear();

            var closest = GetClosestTargetTransportPodTile(map);
            var distance = Find.WorldGrid.TraversalDistanceBetween(map.Tile, closest, true, int.MaxValue, true);
            dropPods.AddRange(GetAllUsableDropPods(map, closest.Layer, distance).Select(x => x.parent));
        }
    }

    private static IEnumerable<CompEscapePod> GetAllUsableEscapePods(Map map)
    {
        for (var i = 0; i < ValidEscapePods.Count; i++)
        {
            var list = map.listerThings.ThingsOfDef(ValidEscapePods[i]);
            if (list.Count > 0 && list[0].TryGetComp<CompEscapePod>() is { } pod && IsEscapePodUsable(pod))
                yield return pod;
        }
    }

    private static IEnumerable<CompLaunchable> GetAllUsableDropPods(Map map, PlanetLayer targetLayer, int distance)
    {
        for (var i = 0; i < ValidTransportPods.Count; i++)
        {
            var list = map.listerThings.ThingsOfDef(ValidTransportPods[i]);
            for (var j = 0; j < list.Count; j++)
            {
                if (list[j].TryGetComp<CompLaunchable>() is {} pod && IsTransportPodUsable(pod, targetLayer, distance))
                    yield return pod;
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

    [DebugAction(DebugActionCategories.Pawns, actionType = DebugActionType.ToolMapForPawns, allowedGameStates = AllowedGameStates.PlayingOnMap, displayPriority = -1000)]
    private static void LogSpaceRebellionData(Pawn p)
    {
        if (p?.Map == null)
            return;

        var builder = new StringBuilder($"Testing space prison escape for {p}.");
        builder.AppendLine();
        builder.Append("Pawn is");
        if (!p.IsPrisoner)
            builder.Append(" not");
        builder.AppendLine(" a prisoner.");

        builder.Append("Pawn is");
        if (p.Map.Biome?.inVacuum != true)
            builder.Append(" not");
        builder.AppendLine(" in vacuum biome");

        builder.AppendLine($"There's {GetAllUsableEscapePods(p.Map).Count()} usable escape pods.");

        var closest = GetClosestTargetTransportPodTile(p.Map);
        var distance = Find.WorldGrid.TraversalDistanceBetween(p.Map.Tile, closest, true, int.MaxValue, true);

        builder.AppendLine($"There's {GetAllUsableDropPods(p.Map, closest.Layer, distance).Count()} usable drop pods.");

        Log.Message(builder.ToString());
    }
}