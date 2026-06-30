using System.Linq;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2;

public class PlaceWorker_VacuumWarningLight : PlaceWorker
{
    private static readonly Color ColorSpotOxygen = new(0.7921569f, 0.6431373f, 0.30980393f, 0.6f);

    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
        var currentMap = Find.CurrentMap;
        var room = center.GetRoom(currentMap);
        var props = def?.GetCompProperties<CompProperties_VacuumWarningLight>();

        if (room == null || props is { alwaysAffectsWholeRoom: false } || room.PsychologicallyOutdoors)
            GenDraw.DrawFieldEdges(GenRadial.RadialCellsAround(center, props?.radius ?? 9.9f, true).ToList(), ColorSpotOxygen);
        else
            GenDraw.DrawFieldEdges(room.Cells.ToList(), ColorSpotOxygen);
    }
}