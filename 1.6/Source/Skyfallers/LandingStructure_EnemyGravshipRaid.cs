using System.Collections.Generic;
using System.Linq;
using RimWorld;
using VanillaGravshipExpanded;
using Verse;
using Verse.AI.Group;

namespace VanillaGravshipExpanded2
{
    public class LandingStructure_EnemyGravshipRaid : LandingStructure_StructureSet
    {
        public bool spawnedByScenario;

        protected override void OnImpact(Map map, CellRect cellRect, HashSet<Thing> preExisting)
        {
            if (spawnedByScenario)
            {
                foreach (var c in cellRect)
                {
                    if (c.InBounds(map) && c.GetTerrain(map) != TerrainDefOf.Space) map.fogGrid.Unfog(c);
                }
            }
            var center = cellRect.CenterCell;
            foreach (var pair in pawnPositions)
            {
                var pawn = pair.Key;
                EnsureAstrorigEquipped(pawn);
                GenSpawn.Spawn(pawn, center + pair.Value, map);
            }
            if (pawnPositions.Count > 0)
            {
                LordMaker.MakeNewLord(shipFaction, new LordJob_AssaultColony(shipFaction, false, false), map, pawnPositions.Keys.ToList());
            }
        }

        private static void EnsureAstrorigEquipped(Pawn pawn)
        {
            foreach (var app in pawn.apparel.WornApparel.ToList())
            {
                if (ApparelUtility.CanWearTogether(InternalDefOf.VGE_Apparel_Astrorig, app.def, pawn.RaceProps.body) is false)
                {
                    pawn.apparel.Remove(app);
                    app.Destroy();
                }
            }
            var astrorig = (Apparel)ThingMaker.MakeThing(InternalDefOf.VGE_Apparel_Astrorig);
            PawnGenerator.PostProcessGeneratedGear(astrorig, pawn);
            pawn.apparel.Wear(astrorig, false);
            var comp = astrorig.GetComp<CompApparelOxygenProvider>();
            if (comp != null)
            {
                comp.RemainingChargesExact = comp.MaxCharges;
            }
        }
    }
}
