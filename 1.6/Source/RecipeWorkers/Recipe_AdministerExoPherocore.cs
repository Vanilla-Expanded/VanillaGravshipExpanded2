
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{

    public class Recipe_AdministerExoPherocore : RecipeWorker
    {
        public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn billDoer, List<Thing> ingredients, Bill bill)
        {
            Hediff hediff = HediffMaker.MakeHediff(InternalDefOf.VFEI2_NullipedeSpawn, pawn);
            hediff.Severity = 0.01f;
            pawn.health.AddHediff(hediff);
        }

        public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord part = null)
        {
            Pawn pawn;
            if ((pawn = thing as Pawn) == null)
            {
                return false;
            }

            if (pawn.Map?.listerThings?.ThingsOfDef(InternalDefOf.VFEI2_PherocoreExo)?.Count == 0)
            {
                return false;
            }
            return base.AvailableReport(thing, part);
        }
    }
}

