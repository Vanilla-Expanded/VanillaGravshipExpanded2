using System.Linq;
using RimWorld;
using VEF.Storyteller;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class GenStep_OrbitalPlatform_Custom : GenStep_OrbitalPlatform
    {
        public StructureSetDef structureSet;
        public override void Generate(Map map, GenStepParams parms)
        {
            var faction = GetFaction(map);
            if (faction?.def == FactionDefOf.TradersGuild)
            {
                map.FogOfWarColor = fogOfWarColor.ToColor;
                var rects = StructureSetGenerator.Generate(map, structureSet, faction);
                GenStep_Warplatform.MakeAllCratesANew(map);

                var minX = rects.Min(r => r.minX);
                var minZ = rects.Min(r => r.minZ);
                var maxX = rects.Max(r => r.maxX);
                var maxZ = rects.Max(r => r.maxZ);
                var spawnRect = CellRect.FromLimits(minX, minZ, maxX, maxZ);
                MapGenerator.SetVar("SpawnRect", spawnRect);
                MapGenerator.UsedRects.Add(spawnRect);
            }
            else
            {
                base.Generate(map, parms);
            }
        }
    }
}
