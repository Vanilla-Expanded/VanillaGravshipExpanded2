using Verse;
using System.Collections.Generic;


namespace VanillaGravshipExpanded2
{
    public class CompProperties_TurnIntoMovingHive : CompProperties
    {

        public float healthPercentage;
        public PawnKindDef turnInto;

        public CompProperties_TurnIntoMovingHive()
        {
            this.compClass = typeof(CompTurnIntoMovingHive);
        }


    }
}
