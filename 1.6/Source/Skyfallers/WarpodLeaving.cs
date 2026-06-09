using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HotSwappable]
    [StaticConstructorOnStartup]
    public class WarpodLeaving : Skyfaller
    {
        public PlanetTile destinationTile;
        public TransportersArrivalAction arrivalAction;
        public WorldObjectDef worldObjectDef;
        private static readonly Graphic FlameGraphic = GraphicDatabase.Get<Graphic_Single>("Things/Mote/SmallThruster_Burn", ShaderDatabase.MoteGlow, Vector2.one, Color.white);

        public ActiveTransporterInfo Contents => ((ActiveTransporter)innerContainer[0]).Contents;

        public override Graphic Graphic => Contents.sentTransporterDef.graphic;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref destinationTile, "destinationTile", -1);
            Scribe_Deep.Look(ref arrivalAction, "arrivalAction");
            Scribe_Defs.Look(ref worldObjectDef, "worldObjectDef");
        }

        private float CurrentU
        {
            get
            {
                var t = (float)ticksToImpact / LeaveMapAfterTicks;
                return 3f * t * t * t * t;
            }
        }

        private Vector3 GetFlightOffset(float u)
        {
            if (u <= 1f)
            {
                return new Vector3(-50f * u * u, 0f, 100f * u - 50f * u * u);
            }
            return new Vector3(-100f * u + 50f, 0f, 50f);
        }

        private float GetFlightAngle(float u)
        {
            var dx = -100f;
            var dz = 0f;
            if (u <= 1f)
            {
                dx = -100f * u;
                dz = 100f - 100f * u;
            }
            return Mathf.Atan2(dx, dz) * Mathf.Rad2Deg;
        }

        public override Vector3 DrawPos
        {
            get
            {
                var pos = Position.ToVector3Shifted();
                if (ticksToImpact > 0)
                {
                    pos.y = Altitudes.AltitudeFor(AltitudeLayer.Skyfaller);
                    pos += GetFlightOffset(CurrentU);
                }
                return pos;
            }
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            var angle = 0f;
            var t = 0f;
            if (ticksToImpact > 0)
            {
                t = (float)ticksToImpact / LeaveMapAfterTicks;
                angle = GetFlightAngle(CurrentU);
            }

            Graphic.Draw(drawLoc, flip ? Rotation.Opposite : Rotation, this, angle);

            if (t > 0f)
            {
                var flameScale = new Vector3(1.2f, 1f, 2.2f * t);
                var nozzleOffsetDist = 0.2f;
                var flameOffset = Quaternion.Euler(0f, angle, 0f) * Vector3.back * (nozzleOffsetDist + (flameScale.z * 0.5f));
                flameOffset.y -= 0.1f;
                var matrix = Matrix4x4.TRS(drawLoc + flameOffset, Quaternion.Euler(0f, angle, 0f), flameScale);
                Graphics.DrawMesh(MeshPool.plane10, matrix, FlameGraphic.MatSingle, 0);
            }
            DrawDropSpotShadow();
        }

        public override void Tick()
        {
            base.Tick();
            if (Spawned)
            {
                if (!new IntVec3(DrawPos).InBounds(Map))
                {
                    LeaveMap();
                    return;
                }
            }
            if (Spawned && ticksToImpact > 0 && ticksToImpact < LeaveMapAfterTicks)
            {
                var pos = DrawPos;
                var angle = GetFlightAngle(CurrentU);
                var exhaustOffset = Quaternion.Euler(0f, angle, 0f) * Vector3.back * 0.8f;

                FleckMaker.ThrowSmoke(pos + exhaustOffset, Map, 1.5f);
                var t = (float)ticksToImpact / LeaveMapAfterTicks;
                if (Rand.Chance(Mathf.Lerp(0.25f, 1f, t)))
                {
                    FleckMaker.ThrowHeatGlow((pos + exhaustOffset).ToIntVec3(), Map, 1.5f);
                }
            }
        }

        public override void LeaveMap()
        {
            var travel = (TravellingTransporters)WorldObjectMaker.MakeWorldObject(worldObjectDef);
            travel.Tile = Map.Tile;
            travel.SetFaction(Faction.OfPlayer);
            travel.destinationTile = destinationTile;
            travel.arrivalAction = arrivalAction;
            Find.WorldObjects.Add(travel);
            var activeTransporter = (ActiveTransporter)innerContainer[0];
            var contents = activeTransporter.Contents;
            activeTransporter.Contents = null;
            travel.AddTransporter(contents, true);
            Destroy();
        }
    }
}
