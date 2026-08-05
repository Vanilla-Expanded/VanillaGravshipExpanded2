using System.Linq;
using RimWorld;
using VEF.Storyteller;
using Verse;
using Verse.AI.Group;

namespace VanillaGravshipExpanded2
{
    public class GenStep_SalvagerStation : GenStep
    {
        public override int SeedPart => 1634184425;
        public StructureSetDef structureSetDef;

        public override void Generate(Map map, GenStepParams parms)
        {
            var parent = map.Parent;
            if (parent.Faction == null || parent.Faction == Faction.OfPlayer)
            {
                parent.SetFaction(Faction.OfSalvagers);
            }
            var rects = StructureSetGenerator.Generate(map, structureSetDef, map.ParentFaction);
            GenStep_Warplatform.MakeAllCratesANew(map);

            var cells = rects.SelectMany(r => r.Cells).Distinct().Where(c => c.Standable(map) && c.Roofed(map)).ToList();
            if (!cells.Any())
            {
                cells = rects.SelectMany(r => r.Cells).Distinct().Where(c => c.Standable(map)).ToList();
            }
            var comp = WorldComponent_GravshipCombat.Instance;

            var pawnCount = Rand.RangeInclusive(16, 24);
            var pawns = new System.Collections.Generic.List<Pawn>();

            pawns.Add(comp.salvagerLeader);
            comp.salvagerLeader = null;
            pawnCount--;

            for (int i = 0; i < pawnCount; i++)
            {
                var kind = Faction.OfSalvagers.def.pawnGroupMakers.SelectMany(gm => gm.options).RandomElementByWeight(opt => opt.selectionWeight).kind;
                pawns.Add(PawnGenerator.GeneratePawn(kind, Faction.OfSalvagers));
            }

            LordMaker.MakeNewLord(Faction.OfSalvagers, new LordJob_DefendBase(Faction.OfSalvagers, map.Center, 25000), map, pawns);
            foreach (var pawn in pawns)
                GenSpawn.Spawn(pawn, cells.RandomElement(), map);

            var turrets = Rand.RangeInclusive(5, 10);
            for (int i = 0; i < turrets; i++)
            {
                GenSpawn.Spawn(InternalDefOf.VGE_Turret_DefenderTurret, cells.RandomElement(), map).SetFaction(Faction.OfSalvagers);
            }

            if (map.Parent is MapParent_WarPlatform wp)
            {
                wp.customLabel = comp.salvagerStationName;
                wp.customDescription = InternalDefOf.VGE_SalvagerStation.worldObjectDef.description.Formatted(wp.customLabel);
            }
        }
    }
}
