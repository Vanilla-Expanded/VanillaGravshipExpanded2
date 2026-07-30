using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HotSwappable]
    public class SalvagerDropshipIncoming : ShuttleIncoming
    {
        public float exactAngle;
        public IntVec3 startCell = IntVec3.Invalid;

        public Building Shuttle => (Building)innerContainer[0];

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref exactAngle, "exactAngle");
            Scribe_Values.Look(ref startCell, "startCell", IntVec3.Invalid);
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                exactAngle = Shuttle.Rotation.AsAngle;
            }
        }

        public override Vector3 DrawPos
        {
            get
            {
                var targetPos = Position.ToVector3Shifted();
                var t = TimeInAnimation;
                Vector3 currentPos;

                if (startCell.IsValid)
                {
                    var startPos = startCell.ToVector3Shifted();
                    var lerpFactor = 1f - Mathf.Pow(1f - t, 3f);
                    currentPos = Vector3.Lerp(startPos, targetPos, lerpFactor);
                }
                else
                {
                    var dist = Mathf.Lerp(80f, 0f, 1f - Mathf.Pow(1f - t, 3f));
                    var dir = Quaternion.Euler(0, exactAngle, 0) * Vector3.forward;
                    currentPos = targetPos - dir * dist;
                }

                currentPos.y = AltitudeLayer.Skyfaller.AltitudeFor();
                return currentPos;
            }
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            GetDrawPositionAndRotation(ref drawLoc, out var extraRotation);
            var currentVisualAngle = Rotation.AsAngle + extraRotation;

            DrawTwoThrusters(drawLoc, currentVisualAngle, Mathf.Lerp(4.5f, 0.5f, TimeInAnimation), Rotation);

            Graphic.Draw(drawLoc, flip ? Rotation.Opposite : Rotation, this, extraRotation);
            DrawDropSpotShadow();
        }

        public static void DrawTwoThrusters(Vector3 drawLoc, float angleDeg, float flameLength, Rot4 rot)
        {
            (float offset1, float offset2) offsets;
            if (rot == Rot4.North) offsets = (-1.2f, 1.2f);
            else if (rot == Rot4.East) offsets = (-1.7f, 0.5f);
            else if (rot == Rot4.South) offsets = (-1.2f, 1.2f);
            else if (rot == Rot4.West) offsets = (-0.5f, 1.7f);
            else offsets = (-1.7f, 0.5f);
            var nozzleOffset = 3f;
            if (rot == Rot4.North)
            {
                nozzleOffset = 1.8f;
                drawLoc.y += 1f;
            }
            WarpodLeaving.DrawThrusterFlame(drawLoc, angleDeg, flameLength, 1.2f, offsets.offset1, nozzleOffset);
            WarpodLeaving.DrawThrusterFlame(drawLoc, angleDeg, flameLength, 1.2f, offsets.offset2, nozzleOffset);
        }

        public override void GetDrawPositionAndRotation(ref Vector3 drawLoc, out float extraRotation)
        {
            var t = TimeInAnimation;
            var yawBlend = 1f - t;

            var angleDiff = Mathf.DeltaAngle(Shuttle.Rotation.AsAngle, exactAngle);
            extraRotation = angleDiff * (yawBlend * yawBlend * yawBlend);

            drawLoc.z += def.skyfaller.zPositionCurve.Evaluate(t);
        }

        public override float DrawAngle() => 0f;
    }
}
