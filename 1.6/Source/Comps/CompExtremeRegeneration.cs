using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;

namespace VanillaGravshipExpanded2
{
    public class CompExtremeRegeneration : ThingComp
    {
        public CompProperties_ExtremeRegeneration Props => (CompProperties_ExtremeRegeneration)props;

        public bool regenerationActive = true;
        public int regenerationCounter = 0;
        public const int regenerationRecoverTime = 15000; //6 hours

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.regenerationCounter, "regenerationCounter", 0);
            Scribe_Values.Look(ref this.regenerationActive, "regenerationActive", true);

        }

        public override void CompTickInterval(int delta)
        {
            if (parent.IsHashIntervalTick(Props.rateInTicks, delta))
            {
                Pawn pawn = this.parent as Pawn;

                if (regenerationActive)
                {
                    if (pawn.health != null)
                    {
                        if (pawn.IsBurning())
                        {
                            regenerationActive = false;
                            SwapHediff(pawn, false);
                        }
                        else
                        {
                            List<Hediff_Injury> bleedingInjuries = GetInjuries(pawn, true, false);

                            if (bleedingInjuries.Count > 0)
                            {
                                foreach (Hediff_Injury injury in bleedingInjuries)
                                {
                                    if (injury.TendableNow())
                                        injury.Tended(Props.tendMin, Props.tendMax);
                                }
                            }

                            List<Hediff_Injury> allTendedInjuries = GetInjuries(pawn, false, true);

                            if (allTendedInjuries.Count > 1)
                            {
                                List<Hediff_Injury> tworandomTended = allTendedInjuries.Take(2).ToList();
                                for (int i = 0; i < 2; i++)
                                {
                                    tworandomTended[i].Heal(1000);
                                }
                            }
                        }

                    }
                }
                else
                {
                    regenerationCounter++;
                    if(regenerationCounter >= regenerationRecoverTime)
                    {
                        regenerationCounter = 0;
                        regenerationActive = true;
                        SwapHediff(pawn, true);
                    }

                }
                
            }
        }

        public List<Hediff_Injury> GetInjuries(Pawn pawn, bool getOnlyBleeding, bool getTended)
        {
            List<Hediff_Injury> injuries = new List<Hediff_Injury>();
            for (int i = 0; i < pawn.health.hediffSet.hediffs.Count; i++)
            {
                if (pawn.health.hediffSet.hediffs[i] is Hediff_Injury hediff_Injury)
                {
                    if (hediff_Injury.def != InternalDefOf.Burn && (getOnlyBleeding && hediff_Injury.Bleeding)||(getTended && !hediff_Injury.Bleeding))
                    {
                        injuries.Add(hediff_Injury);
                    }
                  
                }
            }
            return injuries;
        }

        public void SwapHediff(Pawn pawn,bool active)
        {
            if (active)
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.inactiveHediff, false);
                if(hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                    pawn.health.AddHediff(Props.activeHediff);
                }

            }
            else
            {
                Hediff hediff = pawn.health.hediffSet.GetFirstHediffOfDef(Props.activeHediff, false);
                if (hediff != null)
                {
                    pawn.health.RemoveHediff(hediff);
                    pawn.health.AddHediff(Props.inactiveHediff);
                }
            }

        }
    }
}