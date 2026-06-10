using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2;

public class Building_MultiTileAirlock : Building_Airlock
{
    private bool tmpStuckOpen;

    public override bool CanDrawMovers => def.Size.x != 1 || def.size.z != 1;

    public override void Tick()
    {
        base.Tick();
        tmpStuckOpen = StuckOpen;
    }

    public override void DrawAt(Vector3 drawLoc, bool flip = false)
    {
        DoorPreDraw();

        if (!tmpStuckOpen)
        {
            var pos1 = Building_MultiTileDoor.UpperMoverOffsetStart + 0.35000002f * OpenPct;
            DrawMovers(drawLoc, pos1, Graphic, AltitudeLayer.DoorMoveable.AltitudeFor(), Building_MultiTileDoor.MoverDrawScale, Graphic.ShadowGraphic);
            if (def.building.upperMoverGraphic != null)
            {
                var pos2 = Building_MultiTileDoor.UpperMoverOffsetStart + 0.35000002f * Mathf.Clamp01(OpenPct * 2.5f);
                DrawMovers(drawLoc, pos2, def.building.upperMoverGraphic.Graphic, Building_MultiTileDoor.UpperMoverAltitude, Building_MultiTileDoor.MoverDrawScale, null);
            }
        }

        base.DrawAt(drawLoc, flip);
    }

}