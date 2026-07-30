using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HotSwappable]
    public class SalvagerDropshipLeaving : FlyShipLeaving
    {
        public float exactAngle;
        public Building Shuttle => (Building)Contents.GetShuttle();

        private float flightDistance;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref exactAngle, "exactAngle");
            Scribe_Values.Look(ref flightDistance, "flightDistance");
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                exactAngle = Shuttle.Rotation.AsAngle;
                Rotation = Shuttle.Rotation;
                flightDistance = Mathf.Sqrt(map.Size.x * map.Size.x + map.Size.z * map.Size.z);
            }
        }

        public override Vector3 DrawPos
        {
            get
            {
                var startPos = Position.ToVector3Shifted();
                var t = TimeInAnimation;

                var dist = Mathf.Lerp(0f, flightDistance, Mathf.Pow(t, 3f));
                var dir = Quaternion.Euler(0, exactAngle, 0) * Vector3.forward;
                var currentPos = startPos + dir * dist;

                currentPos.y = AltitudeLayer.Skyfaller.AltitudeFor();
                return currentPos;
            }
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            GetDrawPositionAndRotation(ref drawLoc, out var extraRotation);
            var currentVisualAngle = Shuttle.Rotation.AsAngle + extraRotation;

            SalvagerDropshipIncoming.DrawTwoThrusters(drawLoc, currentVisualAngle, Mathf.Lerp(1.5f, 5.5f, TimeInAnimation), Rotation);

            Graphic.Draw(drawLoc, flip ? Rotation.Opposite : Rotation, this, extraRotation);
            DrawDropSpotShadow();
        }

        public override void GetDrawPositionAndRotation(ref Vector3 drawLoc, out float extraRotation)
        {
            var t = TimeInAnimation;
            var yawBlend = t;

            var angleDiff = Mathf.DeltaAngle(Shuttle.Rotation.AsAngle, exactAngle);
            extraRotation = angleDiff * (yawBlend * yawBlend * yawBlend);

            drawLoc.z += def.skyfaller.zPositionCurve.Evaluate(t);
        }

        public override void LeaveMap()
        {
            var drawCell = DrawPos.ToIntVec3();
            if (drawCell.InBounds(Map))
            {
                ticksToImpact--;
                Rotation = Shuttle.Rotation;
                return;
            }
            base.LeaveMap();
        }

        public override float DrawAngle() => 0f;
    }
}
