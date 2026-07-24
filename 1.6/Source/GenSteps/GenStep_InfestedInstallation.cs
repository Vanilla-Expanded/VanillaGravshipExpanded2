using System.Linq;
using RimWorld;
using VEF.Storyteller;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class GenStep_InfestedInstallation : GenStep
    {
        public override int SeedPart => 1345184429;
        public StructureSetDef structureSetDef;

        public override void Generate(Map map, GenStepParams parms)
        {
            var parent = map.Parent;
            if (parent.Faction == null || parent.Faction == Faction.OfPlayer)
            {
                parent.SetFaction(Faction.OfInsects);
            }
            map.OrbitalDebris = InternalDefOf.VGE_GravshipDebris;
            var rects = StructureSetGenerator.Generate(map, structureSetDef, Faction.OfInsects);
            GenStep_Warplatform.MakeAllCratesANew(map);
            var minX = rects.Min(r => r.minX);
            var minZ = rects.Min(r => r.minZ);
            var maxX = rects.Max(r => r.maxX);
            var maxZ = rects.Max(r => r.maxZ);
            var spawnRect = CellRect.FromLimits(minX, minZ, maxX, maxZ);
            MapGenerator.SetVar("SpawnRect", spawnRect);
            MapGenerator.UsedRects.Add(spawnRect);
        }
    }
}
