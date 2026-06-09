using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class Projectile_Warpod : Projectile, IThingHolder
    {
        public ThingDef warpodDef;
        public ThingOwner innerContainer;
        public Faction launchingFaction;

        public Projectile_Warpod()
        {
            innerContainer = new ThingOwner<Thing>(this);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref warpodDef, "warpodDef");
            Scribe_Deep.Look(ref innerContainer, "innerContainer", this);
            Scribe_References.Look(ref launchingFaction, "launchingFaction");
            if (Scribe.mode == LoadSaveMode.PostLoadInit && launcher == null)
            {
                launcher = CreateDummyLauncher();
            }
        }

        public Thing CreateDummyLauncher()
        {
            var dummy = ThingMaker.MakeThing(warpodDef);
            dummy.SetFaction(launchingFaction);
            return dummy;
        }

        public ThingOwner GetDirectlyHeldThings() => innerContainer;

        public void GetChildHolders(List<IThingHolder> outChildren)
        {
            ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
        }

        public override void Tick()
        {
            base.Tick();
            if (Spawned)
            {
                var smokePos = DrawPos;
                var exhaustOffset = ExactRotation * Vector3.back * 0.8f;
                FleckMaker.ThrowSmoke(smokePos + exhaustOffset, Map, 1.2f);
                if (this.IsHashIntervalTick(4))
                {
                    FleckMaker.ThrowHeatGlow((smokePos + exhaustOffset).ToIntVec3(), Map, 1.2f);
                }
            }
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            var num = def.projectile.arcHeightFactor * GenMath.InverseParabola(DistanceCoveredFractionArc);
            var vector = drawLoc + new Vector3(0f, 0f, 1f) * num;
            var graphic = warpodDef.graphic;
            graphic.Draw(vector, flip ? Rotation.Opposite : Rotation, this, ExactRotation.eulerAngles.y);
            Comps_PostDraw();
        }

        public override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            var things = innerContainer.ToList();
            var explosiveProps = warpodDef.GetCompProperties<CompProperties_Explosive>();
            GenExplosion.DoExplosion(Position, Map, explosiveProps.explosiveRadius, explosiveProps.explosiveDamageType, launcher, explosiveProps.damageAmountBase, explosiveProps.armorPenetrationBase, explosiveProps.explosionSound, warpodDef, null, null, explosiveProps.postExplosionSpawnThingDef, explosiveProps.postExplosionSpawnChance, explosiveProps.postExplosionSpawnThingCount, explosiveProps.postExplosionGasType, null, 255, explosiveProps.applyDamageToExplosionCellsNeighbors, explosiveProps.preExplosionSpawnThingDef, explosiveProps.preExplosionSpawnChance, explosiveProps.preExplosionSpawnThingCount, explosiveProps.chanceToStartFire, explosiveProps.damageFalloff, null, things);
            if (warpodDef.GetCompProperties<CompProperties_Transporter>().massCapacity > 0)
            {
                innerContainer.TryDropAll(Position, Map, ThingPlaceMode.Near);
                GenSpawn.Spawn(ThingDefOf.ChunkSlagSteel, Position, Map);
            }

            innerContainer.ClearAndDestroyContents();
            base.Impact(hitThing, blockedByShield);
        }
    }
}
