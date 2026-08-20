using System.Linq;
using RimWorld;
using VEF.Storyteller;
using Verse;
using Verse.AI.Group;

namespace VanillaGravshipExpanded2
{
    public class GenStep_OrbitalCluster : GenStep
    {
        public override int SeedPart => 1634184428;
        public StructureSetDef structureSetDef;

        public override void Generate(Map map, GenStepParams parms)
        {
            var parent = map.Parent;
            if (parent.Faction == null || parent.Faction == Faction.OfPlayer)
            {
                parent.SetFaction(Faction.OfMechanoids);
            }
            var rects = StructureSetGenerator.Generate(map, structureSetDef, map.ParentFaction);
            GenStep_Warplatform.MakeAllCratesANew(map);

            foreach (var building in map.listerBuildings.allBuildingsNonColonist)
            {
                if (building is Building_TurretGun turret)
                {
                    turret.GetComp<CompCanBeDormant>()?.WakeUp();
                }
            }

            var cells = rects.SelectMany(r => r.Cells).Distinct().Where(c => c.Standable(map)).ToList();

            var points = StorytellerUtility.DefaultThreatPointsNow(Find.World) * 1.25f;
            var groupParms = new PawnGroupMakerParms
            {
                faction = Faction.OfMechanoids,
                groupKind = PawnGroupKindDefOf.Combat,
                points = points
            };

            var pawns = PawnGroupMakerUtility.GeneratePawns(groupParms).ToList();

            LordMaker.MakeNewLord(Faction.OfMechanoids, new LordJob_DefendBase(Faction.OfMechanoids, map.Center, 25000), map, pawns);
            foreach (var pawn in pawns)
            {
                if (cells.TryRandomElement(out var cell)) GenSpawn.Spawn(pawn, cell, map);
                else GenSpawn.Spawn(pawn, map.Center, map);
            }
        }
    }
}
