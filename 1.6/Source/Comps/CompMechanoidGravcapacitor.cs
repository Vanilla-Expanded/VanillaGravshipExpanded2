using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class CompProperties_MechanoidGravcapacitor : CompProperties
    {
        public CompProperties_MechanoidGravcapacitor() => compClass = typeof(CompMechanoidGravcapacitor);
    }

    public class CompMechanoidGravcapacitor : ThingComp
    {
        public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
        {
            base.PostPostApplyDamage(dinfo, totalDamageDealt);
            if (totalDamageDealt >= 20f)
            {
                var map = parent.MapHeld;
                if (map != null)
                {
                    GenExplosion.DoExplosion(parent.Position, map, 9.9f, DamageDefOf.EMP, parent, 50);
                }
            }
        }
    }
}
