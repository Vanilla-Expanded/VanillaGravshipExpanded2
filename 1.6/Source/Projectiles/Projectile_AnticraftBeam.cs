using RimWorld;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HotSwappable]
    public class Projectile_AnticraftBeam : Projectile_ArtilleryBeam
    {
        public AnticraftBeamStrike strike;

        public override void SpawnWorldProjectile()
        {
            var targetMap = Find.Maps.FirstOrDefault(m => m.Tile == targetTile);
            var comp = GravshipTurret.TryGetComp<CompWorldArtillery>();
            var targetCell = comp != null && comp.target.IsValid ? comp.target.Cell : target.Cell;
            strike = (AnticraftBeamStrike)GenSpawn.Spawn(InternalDefOf.VGE_EnemyAnticraftBeamStrike, targetCell, targetMap);
            strike.duration = 600;
            strike.instigator = launcher;
            strike.weaponDef = equipmentDef;
            strike.StartStrike();
            var spawnCell = ArtilleryUtility.FindSpawnCell(targetMap, targetTile, Map.Tile, targetCell);
            var originInfo = new TargetInfo(spawnCell, targetMap);
            var destInfo = new TargetInfo(targetCell, targetMap);
            MoteMaker.MakeInteractionOverlay(InternalDefOf.VGE_Mote_AnticraftBeam, originInfo, destInfo);
            var emitter = launcher as Building_EnemyAnticraftEmitter;
            emitter.currentStrike = strike;
        }

        public override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            if (!blockedByShield)
            {
                strike = (AnticraftBeamStrike)GenSpawn.Spawn(InternalDefOf.VGE_EnemyAnticraftBeamStrike, intendedTarget.Cell, Map);
                strike.duration = 600;
                strike.instigator = launcher;
                strike.weaponDef = equipmentDef;
                strike.StartStrike();
                var emitter = launcher as Building_EnemyAnticraftEmitter;
                emitter.currentStrike = strike;
            }
            base.Impact(hitThing, blockedByShield);
        }

        public override void ExposeData()        {
            base.ExposeData();
            Scribe_References.Look(ref strike, "strike");
        }
    }
}
