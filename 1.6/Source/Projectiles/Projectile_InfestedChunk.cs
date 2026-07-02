using System.Collections.Generic;
using RimWorld;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class Projectile_InfestedChunk : Projectile_Space
    {
        private bool impacted;
        public bool isLarge;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref impacted, "impacted");
            Scribe_Values.Look(ref isLarge, "isLarge");
        }

        public override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            impacted = true;
            var map = Map;
            var pos = Position;
            if (!blockedByShield)
            {
                var filthCount = isLarge ? Rand.RangeInclusive(4, 6) : Rand.RangeInclusive(2, 3);
                for (var i = 0; i < filthCount; i++)
                {
                    var c = pos + GenRadial.RadialPattern[Rand.Range(0, 9)];
                    if (c.InBounds(map))
                    {
                        FilthMaker.TryMakeFilth(c, map, ThingDefOf.Filth_Slime);
                    }
                }
                if (isLarge)
                {
                    var hive = ThingMaker.MakeThing(InternalDefOf.VGE_ExoHive_Building);
                    hive.SetFaction(Faction.OfInsects);
                    GenSpawn.Spawn(hive, pos, map);
                }
                else
                {
                    var eggCount = Rand.RangeInclusive(2, 3);
                    for (var i = 0; i < eggCount; i++)
                    {
                        var c = CellFinder.RandomClosewalkCellNear(pos, map, 2);
                        if (c.InBounds(map))
                        {
                            var egg = ThingMaker.MakeThing(InternalDefOf.VGE_EggSac);
                            egg.SetFaction(Faction.OfInsects);
                            GenSpawn.Spawn(egg, c, map);
                        }
                    }
                }
                GenExplosion.DoExplosion(pos, map, isLarge ? 2.9f : 1.9f, DamageDefOf.Bomb, launcher, isLarge ? 30 : 20, 0.2f, null, def);
            }
            SpawnRandomInsect(pos, map);
            base.Impact(hitThing, blockedByShield);
        }

        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
        {
            if (!impacted && Map != null && Position.InBounds(Map))
            {
                SpawnRandomInsect(Position, Map);
            }
            base.Destroy(mode);
        }

        private void SpawnRandomInsect(IntVec3 pos, Map map)
        {
            var options = new List<(PawnKindDef kind, float weight)>
            {
                (InternalDefOf.VGE_Exoworm, 100f),
                (InternalDefOf.VGE_Exopede, 50f),
                (InternalDefOf.VGE_Exoleech, 20f),
                (InternalDefOf.VGE_ExowormKnot, 5f)
            };
            var selected = options.RandomElementByWeight(x => x.weight).kind;
            var insect = PawnGenerator.GeneratePawn(selected, Faction.OfInsects);
            var spawnCell = CellFinder.RandomClosewalkCellNear(pos, map, 2);
            if (!spawnCell.InBounds(map))
            {
                spawnCell = pos;
            }
            GenSpawn.Spawn(insect, spawnCell, map);
            insect.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.Manhunter);
        }
    }
}
