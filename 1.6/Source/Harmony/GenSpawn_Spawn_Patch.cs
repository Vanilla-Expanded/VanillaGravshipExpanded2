using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(GenSpawn), nameof(GenSpawn.Spawn), new[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(Rot4), typeof(WipeMode), typeof(bool), typeof(bool) })]
    public static class GenSpawn_Spawn_Patch
    {
        public static void Prefix(Thing newThing, Map map, bool respawningAfterLoad)
        {
            if (!respawningAfterLoad && map.Biome.inVacuum && newThing is Pawn pawn && pawn.RaceProps.Humanlike && pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer))
            {
                var actions = new List<(float weight, Action action)>
                {
                    (1f, () => GiveApparel(pawn, InternalDefOf.VGE_Apparel_Astrorig, Rand.Range(0.07f, 0.36f))),
                    (14f, () => GiveApparel(pawn, InternalDefOf.VGE_Apparel_DisposableOxygenPack, 1f))
                };
                actions.RandomElementByWeight(x => x.weight).action();
            }
        }

        private static void GiveApparel(Pawn pawn, ThingDef apparelDef, float fuelPct)
        {
            var conflicting = pawn.apparel.WornApparel
                .Where(x => !ApparelUtility.CanWearTogether(apparelDef, x.def, pawn.RaceProps.body))
                .ToList();
            if (conflicting.Any(x => x.GetComp<CompApparelOxygenProvider>() is not null))
            {
                return;
            }
            
            foreach (var c in conflicting)
            {
                pawn.apparel.Remove(c);
                c.Destroy();
            }

            var apparel = (Apparel)ThingMaker.MakeThing(apparelDef);
            PawnGenerator.PostProcessGeneratedGear(apparel, pawn);
            pawn.apparel.Wear(apparel, false);

            var comp = apparel.GetComp<CompApparelOxygenProvider>();
            comp.RemainingChargesExact = comp.MaxCharges * fuelPct;
        }
    }
}
