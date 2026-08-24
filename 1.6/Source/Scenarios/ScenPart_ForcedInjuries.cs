using System.Linq;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class ScenPart_ForcedInjuries : ScenPart
    {
        private const float InjuryChance = 0.75f;
        private const int MinDamage = 6;
        private const int MaxDamage = 14;

        public override void PostMapGenerate(Map map)
        {
            base.PostMapGenerate(map);
            if (Find.GameInitData == null) return;

            foreach (var pawn in map.mapPawns.FreeColonists)
            {
                if (Rand.Chance(InjuryChance) is false) continue;

                var damage = Rand.RangeInclusive(MinDamage, MaxDamage);
                var hediffDef = Rand.Bool ? InternalDefOf.Shredded : InternalDefOf.Crack;

                var candidateParts = pawn.health.hediffSet.GetNotMissingParts()
                    .Where(p => p.def.tags.Any(t => t.vital) is false && p.coverageAbs > 0f && pawn.health.hediffSet.GetPartHealth(p) > damage && pawn.health.WouldDieAfterAddingHediff(hediffDef, p, damage) is false)
                    .ToList();

                if (candidateParts.TryRandomElement(out var part))
                {
                    var injury = (Hediff_Injury)HediffMaker.MakeHediff(hediffDef, pawn);
                    injury.Part = part;
                    injury.Severity = damage;
                    pawn.health.AddHediff(injury);
                }
            }
        }
    }
}
