using RimWorld;
using VEF.Storyteller;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class GenStep_GravshipHusk : GenStep_SpaceEncounter
    {
        public override int SeedPart => 1634184427;
        public StructureSetDef structureSetDef;

        protected override void GenerateSpaceMap(Map map, GenStepParams parms)
        {
            map.OrbitalDebris = InternalDefOf.VGE_GravshipDebris;
            StructureSetGenerator.Generate(map, structureSetDef, Faction.OfAncients);
        }
    }
}
