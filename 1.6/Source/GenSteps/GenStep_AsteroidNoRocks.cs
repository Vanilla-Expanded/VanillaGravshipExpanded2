using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class GenStep_AsteroidNoRocks : GenStep_Asteroid
    {
        public override void Generate(Map map, GenStepParams parms)
        {
            if (ModLister.CheckOdyssey("Asteroid"))
            {
                GenerateAsteroidElevation(map, parms);
                SpawnAsteroidWithoutRocks(map);
                map.OrbitalDebris = OrbitalDebrisDefOf.Asteroid;
            }
        }

        private static void SpawnAsteroidWithoutRocks(Map map)
        {
            using (map.pathing.DisableIncrementalScope())
            {
                foreach (IntVec3 allCell in map.AllCells)
                {
                    if (MapGenerator.Elevation[allCell] > 0.5f)
                    {
                        map.terrainGrid.SetTerrain(allCell, ThingDefOf.Vacstone.building.naturalTerrain);
                    }
                }
                HashSet<IntVec3> mainIsland = new HashSet<IntVec3>();
                map.floodFiller.FloodFill(map.Center, (IntVec3 x) => x.GetTerrain(map) != TerrainDefOf.Space, delegate (IntVec3 x)
                {
                    mainIsland.Add(x);
                });
                foreach (IntVec3 allCell2 in map.AllCells)
                {
                    if (mainIsland.Contains(allCell2))
                    {
                        continue;
                    }
                    map.terrainGrid.SetTerrain(allCell2, TerrainDefOf.Space);
                    map.roofGrid.SetRoof(allCell2, null);
                    foreach (Thing item in allCell2.GetThingList(map).ToList())
                    {
                        item.Destroy();
                    }
                }
            }
        }
    }
}
