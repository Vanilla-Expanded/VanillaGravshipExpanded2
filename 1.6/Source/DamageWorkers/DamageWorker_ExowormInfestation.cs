using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class DamageWorker_ExowormInfestation : DamageWorker_Bite
    {

        public override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageWorker.DamageResult result)
        {
            base.ApplySpecialEffectsToPart(pawn, totalDamage, dinfo, result);
            if (!pawn.IsGhoul && !pawn.RaceProps.IsMechanoid && pawn.genes?.HasActiveGene(InternalDefOf.PerfectImmunity)!=true
                && Rand.Chance(0.1f))
            {
                List<Hediff> infestationHediffs = new List<Hediff>();
                pawn.health.hediffSet.GetHediffs(ref infestationHediffs, x => x.def == InternalDefOf.VGE_ExowormInfestation);
                List<BodyPartRecord> infestedParts = infestationHediffs.Select(x => x.Part).ToList();

                if (!infestedParts.Contains(dinfo.HitPart))
                {
                    pawn.health.AddHediff(InternalDefOf.VGE_ExowormInfestation, dinfo.HitPart, null, null);
                }

            }

        }
    }
}
