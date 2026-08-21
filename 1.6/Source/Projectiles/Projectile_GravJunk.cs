using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class Projectile_GravJunk : Projectile
    {
        public override void Tick()
        {
            base.Tick();
            if (Spawned && this.IsHashIntervalTick(3))
            {
                FleckMaker.ThrowSmoke(DrawPos, Map, 1.5f);
            }
        }

        public override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            var map = Map;
            var pos = Position;
            base.Impact(hitThing, blockedByShield);
            if (blockedByShield is false && pos.InBounds(map))
            {
                var edifice = pos.GetEdifice(map);
                if (edifice != null && edifice.def.destroyable)
                {
                    edifice.Destroy(DestroyMode.KillFinalize);
                }
                GenExplosion.DoExplosion(center: pos, map: map, radius: 2.9f, damType: DamageDefOf.Bomb, instigator: launcher, damAmount: DamageAmount, armorPenetration: ArmorPenetration, weapon: def, postExplosionSpawnSingleThingDef: InternalDefOf.VGE_GravJunk, postExplosionSpawnChance: 1f);
            }
        }
    }
}
