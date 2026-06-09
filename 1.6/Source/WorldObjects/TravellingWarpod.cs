using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class TravellingWarpod : TravellingTransporters
    {
        public ThingDef WarpodDef => transporters.First().sentTransporterDef;
        public override Material Material => WarpodDef.graphic.MatSingle;
        public override Texture2D ExpandingIcon => WarpodDef.uiIcon;
        public override Color ExpandingIconColor => WarpodDef.graphic.color;
        public override float ExpandingIconRotation
        {
            get
            {
                var start = Find.WorldGrid.GetTileCenter(Tile);
                var end = Find.WorldGrid.GetTileCenter(destinationTile);
                var vector = GenWorldUI.WorldToUIPosition(start);
                var vector2 = GenWorldUI.WorldToUIPosition(end);
                var num = Mathf.Atan2(vector2.y - vector.y, vector2.x - vector.x) * 57.29578f;
                if (num > 180f)
                {
                    num -= 180f;
                }
                return num + 90f;
            }
        }

        public override void TickInterval(int delta)
        {
            traveledPct += TraveledPctStepPerTick * (float)delta;
            base.TickInterval(delta);
        }
    }
}
