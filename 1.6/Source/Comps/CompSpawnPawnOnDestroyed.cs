
using RimWorld;
using System;
using Verse.AI.Group;
using Verse;
using VanillaGravshipExpanded2;
using Verse.Sound;
namespace VanillaGravshipExpanded2
{
    public class CompSpawnPawnOnDestroyed : ThingComp
    {
        public CompProperties_SpawnPawnOnDestroyed Props => (CompProperties_SpawnPawnOnDestroyed)props;

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            if (parent.PositionHeld.InBounds(previousMap))
            {
                for (int i = 0; i < Props.filthCountRange.RandomInRange; i++)
                {
                    IntVec3 c;
                    CellFinder.TryFindRandomReachableNearbyCell(parent.PositionHeld, previousMap, 2, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c);
                    FilthMaker.TryMakeFilth(c, previousMap, Props.filthCreated);
                }
                if (Props.sound != null)
                {
                    Props.sound.PlayOneShot(new TargetInfo(parent.PositionHeld, previousMap, false));
                }

                PawnKindDef pawnKind = Props.pawnKind;
              
                Pawn pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(pawnKind, Faction.OfInsects, PawnGenerationContext.NonPlayer, null, fixedBiologicalAge: 2));
                GenSpawn.Spawn(pawn, parent.Position, previousMap, WipeMode.VanishOrMoveAside);
                if (CellFinder.TryFindRandomSpawnCellForPawnNear(parent.Position, previousMap, out var result, 2, (IntVec3 c) => GenSight.LineOfSight(parent.Position, c, previousMap, skipFirstCell: true)))
                {
                    pawn.rotationTracker.FaceCell(result);
                    PawnFlyer pawnFlyer = PawnFlyer.MakeFlyer(ThingDefOf.PawnFlyer_Stun, pawn, result, null, null);
                    if (pawnFlyer != null)
                    {
                        GenSpawn.Spawn(pawnFlyer, result, previousMap);
                    }
                    pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ManhunterPermanent, null, true);

                }

            }
            

        }
    }
}