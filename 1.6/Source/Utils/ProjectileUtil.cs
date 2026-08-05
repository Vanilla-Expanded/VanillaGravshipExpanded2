using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    public static class ProjectileUtil
    {
        public static Thing CreateDummyLauncher(ThingDef def, Faction faction)
        {
            var dummy = ThingMaker.MakeThing(def);
            dummy.SetFaction(faction);
            return dummy;
        }

        public static Projectile_Warpod LaunchWarpod(ActiveTransporterInfo trans, ThingDef incomingDef, Faction faction, Map map, IntVec3 edgeCell, IntVec3 target)
        {
            var projectile = (Projectile_Warpod)ThingMaker.MakeThing(incomingDef);
            projectile.warpodDef = trans.sentTransporterDef;
            projectile.launchingFaction = faction;
            projectile.innerContainer.TryAddRangeOrTransfer(trans.innerContainer, true, true);
            var dummyLauncher = CreateDummyLauncher(projectile.warpodDef, faction);
            GenSpawn.Spawn(projectile, edgeCell, map);
            projectile.Launch(dummyLauncher, edgeCell.ToVector3Shifted(), target, target, ProjectileHitFlags.All);
            return projectile;
        }

        public static Projectile LaunchProjectile(Thing launcher, ThingDef rocketDef, Map map, IntVec3 origin, IntVec3 target)
        {
            var proj = (Projectile)GenSpawn.Spawn(rocketDef, origin, map);
            proj.Launch(launcher, origin.ToVector3Shifted(), target, target, ProjectileHitFlags.IntendedTarget);
            return proj;
        }
    }
}
