using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.Noise;

namespace VanillaGravshipExpanded2
{
    public class Building_TradingContainer : Building
    {

        public override void Destroy(DestroyMode mode)
        {
            TraderKindDef traderKind = DefDatabase<TraderKindDef>.AllDefs.Where(x => x.orbital)?.RandomElementByWeight(x => x.CalculatedCommonality);
            if (traderKind != null)
            {
                TradeShip tradeShip = new TradeShip(traderKind);
                ThingSetMakerParams parms = default;
                parms.traderDef = tradeShip.TraderKind;
                parms.makingFaction = tradeShip.Faction;
                parms.tile = Map.Tile;
                parms.minSingleItemMarketValuePct = 0.15f;

                float targetMarketValue = 4000;
                List<Thing> pool = ThingSetMakerDefOf.TraderStock.root.Generate(parms).ToList();

                pool.RemoveAll(t => t is Pawn || t is MinifiedThing);

                pool.Shuffle();

                List<Thing> wares = new List<Thing>();

                float currentValue = 0f;
                int maxItems = 6;

                foreach (Thing t in pool)
                {
                    if (wares.Count >= maxItems)
                        break;

                    float value = t.MarketValue * t.stackCount;

                    if (currentValue + value > targetMarketValue * 1.15f)
                        continue;

                    wares.Add(t);
                    currentValue += value;

                    if (currentValue >= targetMarketValue)
                        break;
                }

                if (wares.Count == 0 && pool.Count > 0)
                {
                    Thing fallback = pool.MinBy(t => t.MarketValue * t.stackCount);
                    wares.Add(fallback);
                }

                foreach (Thing thing in wares)
                {
                    //thing.SetForbidden(true);
                    GenPlace.TryPlaceThing(thing, Position, Map, ThingPlaceMode.Near);
                }
               
            }

            base.Destroy(mode);
        }

    }
}
