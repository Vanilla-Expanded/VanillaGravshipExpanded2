using System.Linq;
using RimWorld;
using VEF.Storyteller;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HotSwappable]
    public class GenStep_GravshipGraveyard : GenStep_SpaceEncounter
    {
        public override int SeedPart => 1345184428;
        public StructureSetDef structureSetDef;

        protected override void GenerateSpaceMap(Map map, GenStepParams parms)
        {
            map.OrbitalDebris = InternalDefOf.VGE_GravshipDebris;
            StructureSetGenerator.Generate(map, structureSetDef, map.ParentFaction);
            GenStep_Warplatform.MakeAllCratesANew(map);
            var mines = Rand.RangeInclusive(30, 55);
            var spaceTerrains = map.AllCells.Where(c => c.GetTerrain(map) == TerrainDefOf.Space).ToList();
            for (var i = 0; i < mines; i++)
            {
                if (spaceTerrains.TryRandomElement(out var cell))
                {
                    var mine = GenSpawn.Spawn(InternalDefOf.VGE_AncientGravmine, cell, map);
                    mine.SetFaction(Faction.OfAncientsHostile);
                }
            }
        }
    }
}
