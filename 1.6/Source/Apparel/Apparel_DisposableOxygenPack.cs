using RimWorld;
using UnityEngine;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class Apparel_DisposableOxygenPack : Apparel
    {
        private CompApparelOxygenProvider cachedComp;

        private CompApparelOxygenProvider CompOxygen => cachedComp ??= GetComp<CompApparelOxygenProvider>();

        public override void Tick()
        {
            base.Tick();
            var comp = CompOxygen;
            if (comp.RemainingChargesExact <= 0.0001f)
            {
                if (!Destroyed)
                {
                    Destroy(DestroyMode.Vanish);
                }
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            var comp = CompOxygen;
            if (comp.RemainingChargesExact > 0.0001f)
            {
                var amount = Mathf.FloorToInt(comp.RemainingChargesExact);
                if (amount > 0)
                {
                    var oxygen = ThingMaker.MakeThing(InternalDefOf.VGE_OxygenCanister);
                    oxygen.stackCount = amount;
                    if (pawn.MapHeld != null)
                    {
                        GenPlace.TryPlaceThing(oxygen, pawn.PositionHeld, pawn.MapHeld, ThingPlaceMode.Near);
                    }
                    else
                    {
                        pawn.inventory.innerContainer.TryAdd(oxygen);
                    }
                }
                comp.RemainingChargesExact = 0f;
            }
        }
    }
}
