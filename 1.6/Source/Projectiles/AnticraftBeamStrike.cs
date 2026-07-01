using RimWorld;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class AnticraftBeamStrike : OrbitalStrike
    {
        public override void StartStrike()
        {
            base.StartStrike();
            MoteMaker.MakePowerBeamMote(Position, Map);
        }

        public override void Tick()
        {
            base.Tick();
            if (Destroyed)
            {
                return;
            }

            for (var i = 0; i < 4; i++)
            {
                var c = Position + GenRadial.RadialPattern[Rand.Range(0, GenRadial.NumCellsInRadius(15f))];
                if (c.InBounds(Map))
                {
                    FireUtility.TryStartFireIn(c, Map, Rand.Range(0.1f, 0.925f), instigator);
                    GenExplosion.DoExplosion(c, Map, 0.9f, DamageDefOf.Flame, instigator, Rand.Range(65, 100), -1f, null, weaponDef, def);
                    DamageWorker_ExplosionDamageTerrain_Patch.DamageTerrain(c, Map);
                }
            }
        }
    }
}
