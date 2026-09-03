using System.Collections.Generic;
using System.Linq;
using RimWorld;
using VEF.Storyteller;
using Verse;
using Verse.AI.Group;

namespace VanillaGravshipExpanded2
{
    public class GenStep_SalvagerStronghold : GenStep
    {
        public override int SeedPart => 1634184455;
        public StructureSetDef structureSetDef;

        public override void Generate(Map map, GenStepParams parms)
        {
            map.regionAndRoomUpdater.Enabled = true;
            var parent = map.Parent;
            if (parent.Faction == null || parent.Faction == Faction.OfPlayer)
            {
                parent.SetFaction(Faction.OfSalvagers);
            }
            var rects = StructureSetGenerator.Generate(map, structureSetDef, map.ParentFaction);
            GenStep_Warplatform.MakeAllCratesANew(map);

            var minX = rects.Min(r => r.minX);
            var minZ = rects.Min(r => r.minZ);
            var maxX = rects.Max(r => r.maxX);
            var maxZ = rects.Max(r => r.maxZ);
            var spawnRect = CellRect.FromLimits(minX, minZ, maxX, maxZ);
            MapGenerator.SetVar("SpawnRect", spawnRect);
            MapGenerator.UsedRects.Add(spawnRect);

            var cells = rects.SelectMany(r => r.Cells).Distinct().Where(c => c.Standable(map) && c.Roofed(map)).ToList();
            if (cells.Any() is false)
            {
                cells = rects.SelectMany(r => r.Cells).Distinct().Where(c => c.Standable(map)).ToList();
            }
            var pawnCount = Rand.RangeInclusive(14, 24);
            var pawns = new List<Pawn>();
            for (int i = 0; i < pawnCount; i++)
            {
                var kind = Faction.OfSalvagers.def.pawnGroupMakers.SelectMany(gm => gm.options).RandomElementByWeight(opt => opt.selectionWeight).kind;
                pawns.Add(PawnGenerator.GeneratePawn(kind, Faction.OfSalvagers));
            }
            LordMaker.MakeNewLord(Faction.OfSalvagers, new LordJob_DefendBase(Faction.OfSalvagers, map.Center, 25000), map, pawns);
            foreach (var pawn in pawns) GenSpawn.Spawn(pawn, cells.RandomElement(), map);
            ScatterMiningProps(map, parms, rects);
        }

        private void ScatterMiningProps(Map map, GenStepParams parms, List<CellRect> rects)
        {
            Scatter(map, parms, rects, InternalDefOf.AncientMiningCharge, new FloatRange(0f, 15f));
            Scatter(map, parms, rects, ThingDefOf.AncientExplosivesCrate, new FloatRange(0f, 1.25f));
            Scatter(map, parms, rects, InternalDefOf.AncientDrillPlatform, new FloatRange(0f, 1.25f));
        }

        private void Scatter(Map map, GenStepParams parms, List<CellRect> rects, ThingDef thingDef, FloatRange countPer10kCellsRange)
        {
            var genStep = new GenStep_ScatterThingsOnEmptyTerrain
            {
                thingDef = thingDef,
                countPer10kCellsRange = countPer10kCellsRange,
                minSpacing = 5f,
                disallowedRects = rects,
                warnOnFail = false
            };
            genStep.Generate(map, parms);
        }
    }
}
