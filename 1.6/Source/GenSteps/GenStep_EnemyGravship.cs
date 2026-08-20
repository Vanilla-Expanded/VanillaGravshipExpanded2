using System.Linq;
using RimWorld;
using VEF.Storyteller;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class GenStep_EnemyGravship : GenStep
    {
        public override int SeedPart => 1634184429;
        public override void Generate(Map map, GenStepParams parms)
        {
            var parent = map.Parent;
            if (parent.Faction == null || parent.Faction == Faction.OfPlayer) parent.SetFaction(Faction.OfSalvagers);
            var structureSetDef = InternalDefOf.VGE_EnemyGravshipSet;
            var landingStructure = (LandingStructure_StructureSet)ThingMaker.MakeThing(InternalDefOf.VGE_LandingStructure_EnemyGravship);
            landingStructure.structureSetDef = structureSetDef;
            var standardLayouts = StructureSetGenerator.SelectStandardLayouts(structureSetDef);
            landingStructure.selectedDefs = standardLayouts.Select(x => x.def).ToList();
            landingStructure.shipRotation = Rot4.Random;
            landingStructure.shipFaction = Faction.OfSalvagers;
            landingStructure.pawnCountRange = new IntRange(9, 14);
            GenSpawn.Spawn(landingStructure, map.Center, map);
        }
    }
}
