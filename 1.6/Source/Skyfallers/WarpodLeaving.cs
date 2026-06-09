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

        public override Vector3 DrawPos
        {
            get
            {
                var pos = Position.ToVector3Shifted();
                var progress = (float)ticksToImpact / LeaveMapAfterTicks;
                if (progress > 0f && progress < 1f)
                {
                    pos.y = Altitudes.AltitudeFor(AltitudeLayer.Skyfaller);

                    pos.x += -50f * progress * progress;
                    pos.z += 100f * progress - 50f * progress * progress;
                }
                return pos;
            }
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            var angle = 0f;
            var progress = (float)ticksToImpact / LeaveMapAfterTicks;
            if (progress > 0f && progress < 1f)
            {

                var dx = -100f * progress;
                var dz = 100f - 100f * progress;
                angle = Mathf.Atan2(dx, dz) * Mathf.Rad2Deg;
            }

            Graphic.Draw(drawLoc, flip ? Rotation.Opposite : Rotation, this, angle);

            if (progress > 0f && progress < 1f)
            {
                var flameScale = new Vector3(1.2f, 1f, 2.2f * progress);
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
            if (Spawned && ticksToImpact > 0 && ticksToImpact < LeaveMapAfterTicks)
            {
                var progress = (float)ticksToImpact / LeaveMapAfterTicks;

                var dx = -100f * progress;
                var dz = 100f - 100f * progress;
                var angle = Mathf.Atan2(dx, dz) * Mathf.Rad2Deg;

                var pos = DrawPos;
                var exhaustOffset = Quaternion.Euler(0f, angle, 0f) * Vector3.back * 0.8f;

                FleckMaker.ThrowSmoke(pos + exhaustOffset, Map, 1.5f);
                if (this.IsHashIntervalTick(4))
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
