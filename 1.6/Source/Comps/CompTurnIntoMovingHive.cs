using RimWorld;
using RimWorld.Planet;
using System;
using UnityEngine;
using Verse;


namespace VanillaGravshipExpanded2
{
    public class CompTurnIntoMovingHive : ThingComp
    {


        public CompProperties_TurnIntoMovingHive Props
        {
            get
            {
                return (CompProperties_TurnIntoMovingHive)this.props;
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (parent.IsHashIntervalTick(60) && parent.Map!=null)
            {
                if(parent.HitPoints<= parent.MaxHitPoints * Props.healthPercentage)
                {
                    Pawn p = PawnGenerator.GeneratePawn(Props.turnInto, parent.Faction);                  
                    GenSpawn.Spawn(p, parent.Position, parent.Map);
                    parent.Destroy();
                }
            }
        }

      


    }
}
