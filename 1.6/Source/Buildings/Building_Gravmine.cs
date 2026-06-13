using System.Collections.Generic;
using RimWorld;
using Verse;
namespace VanillaGravshipExpanded2
{
    public class Building_Gravmine : Building_TrapExplosive
    {
        private HashSet<Thing> thingsInRadius = new HashSet<Thing>();
        private const float DetectionRadius = 5.9f;
        private const int CheckInterval = 15;
        public override void Tick()
        {
            base.Tick();
            if (Spawned && this.IsHashIntervalTick(CheckInterval))
            {
                CheckSpringRadius();
            }
        }
        private void CheckSpringRadius()
        {
            var currentThings = new HashSet<Thing>();
            var targets = Map.attackTargetsCache.TargetsHostileToFaction(Faction);
            foreach (var target in targets)
            {
                var t = target.Thing;
                if (t != null && t.Spawned && t.Position.InHorDistOf(Position, DetectionRadius))
                {
                    currentThings.Add(t);
                    if (!thingsInRadius.Contains(t))
                    {
                        var chance = this.GetStatValue(StatDefOf.TrapSpringChance);
                        if (t is Pawn p)
                        {
                            chance = SpringChance(p);
                        }
                        if (Rand.Chance(chance))
                        {
                            Spring(t as Pawn);
                            if (Destroyed) return;
                        }
                    }
                }
            }
            thingsInRadius = currentThings;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref thingsInRadius, "thingsInRadius", LookMode.Reference);
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                thingsInRadius?.RemoveWhere(x => x == null);
                thingsInRadius ??= new HashSet<Thing>();
            }
        }
    }
}
