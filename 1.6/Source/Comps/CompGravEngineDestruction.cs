using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class CompGravEngineDestruction : ThingComp
    {
        public CompProperties_GravEngineDestruction Props => (CompProperties_GravEngineDestruction)props;

        private bool imploding;
        private int implosionTicksLeft;
        private const int ImplosionDuration = 30;

        public void StartImplosion()
        {
            if (imploding)
            {
                return;
            }
            imploding = true;
            implosionTicksLeft = ImplosionDuration;

            var data = new FleckCreationData();
            data.def = Props.implosionFleck;
            data.spawnPosition = parent.TrueCenter();
            data.scale = Props.implosionFleck.graphicData.drawSize.x;
            data.ageTicksOverride = -1;
            parent.Map.flecks.CreateFleck(data);
        }

        public override void CompTick()
        {
            base.CompTick();
            if (imploding)
            {
                implosionTicksLeft--;
                if (implosionTicksLeft <= 0)
                {
                    Explode();
                }
            }
        }

        private void Explode()
        {
            var map = parent.Map;
            if (map == null) return;
            var pos = parent.Position;
            var center = parent.TrueCenter();

            if (Props.substructureDamageRadius > 0f)
            {
                foreach (var c in GenRadial.RadialCellsAround(pos, Props.substructureDamageRadius, true))
                {
                    if (c.InBounds(map))
                    {
                        DamageWorker_ExplosionDamageTerrain_Patch.DamageTerrain(c, map);
                    }
                }
            }

            var shockwaveData = new FleckCreationData();
            shockwaveData.def = Props.shockwaveFleck;
            shockwaveData.spawnPosition = center;
            shockwaveData.scale = 1f;
            shockwaveData.ageTicksOverride = -1;
            map.flecks.CreateFleck(shockwaveData);

            var ext = parent.def.GetModExtension<WreckedBuildingReplacementExtension>();
            Thing wreck = null;
            var ignored = new List<Thing>();
            if (ext?.replacementBuilding != null)
            {
                var rot = parent.Rotation;
                var faction = parent.Faction;
                parent.Destroy(DestroyMode.Vanish);
                wreck = GenSpawn.Spawn(ext.replacementBuilding, pos, map, rot);
                if (wreck.def.CanHaveFaction)
                {
                    wreck.SetFaction(faction);
                }
                ignored.Add(wreck);
            }
            else
            {
                parent.Destroy(DestroyMode.Vanish);
            }
            GenExplosion.DoExplosion(center: pos, map: map, radius: Props.explosionRadius, damType: InternalDefOf.BombSuper, instigator: null, damAmount: -1, armorPenetration: -1f, weapon: null, ignoredThings: ignored);
            var count = Props.gravJunkCountRange.RandomInRange;
            for (int i = 0; i < count; i++)
            {
                var targetCell = GetRandomJunkTarget(pos, map);
                var proj = (Projectile)GenSpawn.Spawn(InternalDefOf.VGE_Projectile_GravJunk, pos, map);
                proj.Launch(parent, center, targetCell, targetCell, ProjectileHitFlags.All);
            }
        }

        private IntVec3 GetRandomJunkTarget(IntVec3 pos, Map map)
        {
            for (int i = 0; i < 20; i++)
            {
                var dist = Rand.Range(6.9f, 21.9f);
                var offset = Quaternion.AngleAxis(Rand.Range(0f, 360f), Vector3.up) * Vector3.forward * dist;
                var c = (pos.ToVector3Shifted() + offset).ToIntVec3();
                if (c.InBounds(map))
                {
                    return c;
                }
            }
            return pos;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref imploding, "imploding");
            Scribe_Values.Look(ref implosionTicksLeft, "implosionTicksLeft");
        }
    }
}
