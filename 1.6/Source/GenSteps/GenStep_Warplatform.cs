using System.Linq;
using RimWorld;
using VEF.Storyteller;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class GenStep_Warplatform : GenStep
    {
        public override int SeedPart => 1634184424;
        public StructureSetDef structureSetDef;

        public override void Generate(Map map, GenStepParams parms)
        {
            StructureSetGenerator.Generate(map, structureSetDef, map.ParentFaction);
            foreach (var crate in map.listerThings.ThingsOfDef(ThingDefOf.AncientHermeticCrate).OfType<Building_Crate>())
            {
                crate.innerContainer.ClearAndDestroyContents();
                var loot = ThingSetMakerDefOf.MapGen_HighValueCrate.root.Generate();
                foreach (var l in loot) crate.TryAcceptThing(l);
            }
        }
    }
}
