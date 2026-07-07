using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    public static class WarcomputerHelper
    {
        private static readonly System.Reflection.MethodInfo MemberwiseCloneMethod = typeof(object).GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

        public static bool IsWarcomputerPresent(this Map map)
        {
            if (map == null) return false;
            foreach (var computer in map.listerBuildings.AllBuildingsColonistOfDef(InternalDefOf.VGE_Warcomputer))
            {
                var comp = computer.GetComp<CompGravshipFacility>();
                if (comp.CanBeActive)
                {
                    return true;
                }
            }
            return false;
        }

        public static void ApplyForcedMissRadiusBuffs(this Building_TurretGun turret)
        {
            var hasBuff = IsWarcomputerPresent(turret.Map);
            var verb = turret.AttackVerb;
            if (turret.def == InternalDefOf.VGE_GaussGun || turret.def == InternalDefOf.VGE_GaussHowitzer)
            {
                var originalProps = turret.def.building.turretGunDef.Verbs[0];
                if (verb.verbProps == originalProps)
                {
                    verb.verbProps = (VerbProperties)MemberwiseCloneMethod.Invoke(originalProps, null);
                }

                if (turret.def == InternalDefOf.VGE_GaussGun)
                {
                    verb.verbProps.forcedMissRadius = hasBuff ? System.Math.Max(0f, originalProps.forcedMissRadius - 1f) : originalProps.forcedMissRadius;
                }
                else if (turret.def == InternalDefOf.VGE_GaussHowitzer)
                {
                    verb.verbProps.forcedMissRadius = hasBuff ? System.Math.Max(0f, originalProps.forcedMissRadius - 2f) : originalProps.forcedMissRadius;
                }
            }
        }
    }
}
