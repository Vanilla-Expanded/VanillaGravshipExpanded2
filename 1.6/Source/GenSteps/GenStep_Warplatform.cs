using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
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
            var parent = map.Parent;
            if (parent.Faction == null || parent.Faction == Faction.OfPlayer)
            {
                parent.SetFaction(Faction.OfAncientsHostile);
            }
            StructureSetGenerator.Generate(map, structureSetDef, map.ParentFaction);
            MakeAllCratesANew(map);
        }

        public static void MakeAllCratesANew(Map map, HashSet<Thing> existingThings = null)
        {
            foreach (var crate in map.listerThings.AllThings.OfType<Building_Crate>())
            {
                if (existingThings != null && existingThings.Contains(crate)) continue;
                crate.innerContainer.ClearAndDestroyContents();
                var loot = ThingSetMakerDefOf.MapGen_HighValueCrate.root.Generate();
                foreach (var l in loot) crate.TryAcceptThing(l);
                crate.contentsKnown = false;
                var comp = crate.GetComp<CompHackable>();
                if (comp != null && comp.hacked)
                {
                    comp.hacked = false;
                    comp.progress = 0;
                    comp.sentLetter = false;
                }
            }
        }
    }
}
