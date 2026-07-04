using RimWorld;
using VEF.Storyteller;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class GenStep_GravshipHusk : GenStep
    {
        public override int SeedPart => 1634184427;
        public StructureSetDef structureSetDef;

        public override void Generate(Map map, GenStepParams parms)
        {
            map.OrbitalDebris = InternalDefOf.VGE_GravshipDebris;
            StructureSetGenerator.Generate(map, structureSetDef, Faction.OfAncients);
        }
    }
}
