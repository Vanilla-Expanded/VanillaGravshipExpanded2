using System;
using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using UnityEngine;
using Verse;
using VanillaGravshipExpanded;

namespace VanillaGravshipExpanded2
{
    public class SectionLayer_GravshipArmorHull : SectionLayer
    {
        public enum CornerType
        {
            None,
            Corner_NW,
            Corner_NE,
            Corner_SW,
            Corner_SE,
            Diagonal_NW,
            Diagonal_NE,
            Diagonal_SW,
            Diagonal_SE
        }

        private static readonly Vector2[] UVs = new Vector2[4]
        {
            new Vector2(0f, 0f),
            new Vector2(0f, 1f),
            new Vector2(1f, 1f),
            new Vector2(1f, 0f)
        };

        [TweakValue("VGE_ArmorHullCorners", 0f, 2f)]
        private static float HullCornerScale = 2f;

        private const string TexPath_Corner_NW = "Things/Structures/Platform/Gravarmor/AngledGravshipArmorHull_northwest";
        private const string TexPath_Corner_NE = "Things/Structures/Platform/Gravarmor/AngledGravshipArmorHull_northeast";
        private const string TexPath_Corner_SW = "Things/Structures/Platform/Gravarmor/AngledGravshipArmorHull_southwest";
        private const string TexPath_Corner_SE = "Things/Structures/Platform/Gravarmor/AngledGravshipArmorHull_southeast";

        private const string TexPath_Diagonal_NW = "Things/Structures/Platform/Gravarmor/AngledGravshipArmorHull_Partial_northwest";
        private const string TexPath_Diagonal_NE = "Things/Structures/Platform/Gravarmor/AngledGravshipArmorHull_Partial_northeast";
        private const string TexPath_Diagonal_SW = "Things/Structures/Platform/Gravarmor/AngledGravshipArmorHull_Partial_southwest";
        private const string TexPath_Diagonal_SE = "Things/Structures/Platform/Gravarmor/AngledGravshipArmorHull_Partial_southeast";

        private const string TexPath_SubStructure_W = "Things/Building/Linked/GravshipHull/SubstructureCorner_Full_west";
        private const string TexPath_SubStructure_E = "Things/Building/Linked/GravshipHull/SubstructureCorner_Full_east";
        private const string TexPath_SubStructureExtra_W = "Things/Building/Linked/GravshipHull/SubstructureCorner_Tip_west";
        private const string TexPath_SubStructureExtra_E = "Things/Building/Linked/GravshipHull/SubstructureCorner_Tip_east";

        private const int BakedIndoorMaskRenderQueue = 3185;

        private static CachedMaterial mat_Corner_NW;
        private static CachedMaterial mat_Corner_NE;
        private static CachedMaterial mat_Corner_SW;
        private static CachedMaterial mat_Corner_SE;
        private static CachedMaterial mat_Diagonal_NW;
        private static CachedMaterial mat_Diagonal_NE;
        private static CachedMaterial mat_Diagonal_SW;
        private static CachedMaterial mat_Diagonal_SE;

        private static CachedMaterial mat_SubStructure_W;
        private static CachedMaterial mat_SubStructure_E;
        private static CachedMaterial mat_SubStructureExtra_W;
        private static CachedMaterial mat_SubStructureExtra_E;

        private static readonly float cornerAltitude = AltitudeLayer.BuildingOnTop.AltitudeFor();
        private static readonly float substructureAltitude = AltitudeLayer.TerrainEdges.AltitudeFor();
        private static readonly float bakedAltitude = AltitudeLayer.MetaOverlays.AltitudeFor();

        private static bool initalized;

        private static readonly IntVec3[] Directions = new IntVec3[8]
        {
            IntVec3.North,
            IntVec3.East,
            IntVec3.South,
            IntVec3.West,
            IntVec3.North + IntVec3.West,
            IntVec3.North + IntVec3.East,
            IntVec3.South + IntVec3.East,
            IntVec3.South + IntVec3.West
        };

        private static readonly int[][] directionPairs = new int[4][]
        {
            new int[2] { 0, 2 },
            new int[2] { 1, 3 },
            new int[2] { 4, 6 },
            new int[2] { 5, 7 }
        };

        private static bool[] tmpChecks = new bool[Directions.Length];

        private static Shader WallShader => ShaderDatabase.CutoutOverlay;
        private static Shader SubstructureShader => ShaderDatabase.Transparent;

        public override bool Visible => ModsConfig.OdysseyActive;

        public SectionLayer_GravshipArmorHull(Section section)
            : base(section)
        {
            relevantChangeTypes = (ulong)MapMeshFlagDefOf.Buildings | (ulong)MapMeshFlagDefOf.Terrain | (ulong)MapMeshFlagDefOf.Things | (ulong)MapMeshFlagDefOf.Roofs;
        }

        public static List<LayerSubMesh> BakeGravshipIndoorMesh(Map map, CellRect bounds, Vector3 center)
        {
            var dictionary = new Dictionary<CornerType, LayerSubMesh>();
            var terrainGrid = map.terrainGrid;
            foreach (var item in bounds)
            {
                if (ShouldDrawCornerPiece(item, map, terrainGrid, out var cornerType, out var color) && IsCornerSubstructure(item, cornerType) && IsCornerIndoorMasked(item, cornerType, map))
                {
                    var material = GetMaterial(cornerType).Material;
                    var srcTex = material.mainTexture as Texture2D;
                    var color2 = material.color;
                    var material2 = MaterialPool.MatFrom(srcTex, ShaderDatabase.IndoorMaskMasked, color2, BakedIndoorMaskRenderQueue);
                    var offset = GetOffset(cornerType);
                    if (!dictionary.TryGetValue(cornerType, out var value))
                    {
                        dictionary.Add(cornerType, value = MapDrawLayer.CreateFreeSubMesh(material2, map));
                    }
                    AddQuad(value, (item + offset).ToVector3() - center, HullCornerScale, bakedAltitude, color);
                }
            }
            foreach (var value2 in dictionary.Values)
            {
                value2.FinalizeMesh(MeshParts.All);
            }
            return dictionary.Values.ToList();
        }

        public override void Regenerate()
        {
            if (!ModsConfig.OdysseyActive)
            {
                return;
            }
            ClearSubMeshes(MeshParts.All);
            var map = base.Map;
            var terrainGrid = map.terrainGrid;
            foreach (var item in section.CellRect)
            {
                if (ShouldDrawCornerPiece(item, map, terrainGrid, out var cornerType, out var color))
                {
                    var material = GetMaterial(cornerType);
                    var offset = GetOffset(cornerType);
                    var addGravshipMask = IsCornerSubstructure(item, cornerType);
                    var addIndoorMask = IsCornerIndoorMasked(item, cornerType, map);
                    AddQuad(material.Material, item + offset, HullCornerScale, cornerAltitude, color, addGravshipMask, addIndoorMask);
                    var substructureToSouth = terrainGrid.FoundationAt(item + IntVec3.South)?.IsSubstructure ?? false;
                    AddSubstructure(cornerType, item, substructureToSouth, addGravshipMask, addIndoorMask);
                }
            }
            FinalizeMesh(MeshParts.All);
        }

        private static void EnsureInitialized()
        {
            if (!initalized)
            {
                initalized = true;
                mat_Corner_NW = new CachedMaterial(TexPath_Corner_NW, WallShader);
                mat_Corner_NE = new CachedMaterial(TexPath_Corner_NE, WallShader);
                mat_Corner_SW = new CachedMaterial(TexPath_Corner_SW, WallShader);
                mat_Corner_SE = new CachedMaterial(TexPath_Corner_SE, WallShader);
                mat_Diagonal_NW = new CachedMaterial(TexPath_Diagonal_NW, WallShader);
                mat_Diagonal_NE = new CachedMaterial(TexPath_Diagonal_NE, WallShader);
                mat_Diagonal_SW = new CachedMaterial(TexPath_Diagonal_SW, WallShader);
                mat_Diagonal_SE = new CachedMaterial(TexPath_Diagonal_SE, WallShader);

                mat_SubStructure_W = new CachedMaterial(TexPath_SubStructure_W, SubstructureShader);
                mat_SubStructure_E = new CachedMaterial(TexPath_SubStructure_E, SubstructureShader);
                mat_SubStructureExtra_W = new CachedMaterial(TexPath_SubStructureExtra_W, SubstructureShader);
                mat_SubStructureExtra_E = new CachedMaterial(TexPath_SubStructureExtra_E, SubstructureShader);
            }
        }

        private static bool IsIndoorMasked(IntVec3 c, Map map)
        {
            return c.Roofed(map);
        }

        private static bool IsCornerSubstructure(IntVec3 c, CornerType cornerType)
        {
            switch (cornerType)
            {
                case CornerType.Corner_NE:
                case CornerType.Diagonal_NE:
                    return SectionLayer_GravshipMask.IsValidSubstructure(c + IntVec3.North) || SectionLayer_GravshipMask.IsValidSubstructure(c + IntVec3.East);
                case CornerType.Corner_NW:
                case CornerType.Diagonal_NW:
                    return SectionLayer_GravshipMask.IsValidSubstructure(c + IntVec3.North) || SectionLayer_GravshipMask.IsValidSubstructure(c + IntVec3.West);
                case CornerType.Corner_SE:
                case CornerType.Diagonal_SE:
                    return SectionLayer_GravshipMask.IsValidSubstructure(c + IntVec3.South) || SectionLayer_GravshipMask.IsValidSubstructure(c + IntVec3.East);
                case CornerType.Corner_SW:
                case CornerType.Diagonal_SW:
                    return SectionLayer_GravshipMask.IsValidSubstructure(c + IntVec3.South) || SectionLayer_GravshipMask.IsValidSubstructure(c + IntVec3.West);
                default:
                    return false;
            }
        }

        private static bool IsCornerIndoorMasked(IntVec3 c, CornerType cornerType, Map map)
        {
            switch (cornerType)
            {
                case CornerType.Corner_NE:
                case CornerType.Diagonal_NE:
                    return IsIndoorMasked(c + IntVec3.North, map) || IsIndoorMasked(c + IntVec3.East, map);
                case CornerType.Corner_NW:
                case CornerType.Diagonal_NW:
                    return IsIndoorMasked(c + IntVec3.North, map) || IsIndoorMasked(c + IntVec3.West, map);
                case CornerType.Corner_SE:
                case CornerType.Diagonal_SE:
                    return IsIndoorMasked(c + IntVec3.South, map) || IsIndoorMasked(c + IntVec3.East, map);
                case CornerType.Corner_SW:
                case CornerType.Diagonal_SW:
                    return IsIndoorMasked(c + IntVec3.South, map) || IsIndoorMasked(c + IntVec3.West, map);
                default:
                    return false;
            }
        }

        private static CachedMaterial GetMaterial(CornerType edgeType)
        {
            EnsureInitialized();
            switch (edgeType)
            {
                case CornerType.Corner_NW: return mat_Corner_NW;
                case CornerType.Corner_NE: return mat_Corner_NE;
                case CornerType.Corner_SW: return mat_Corner_SW;
                case CornerType.Corner_SE: return mat_Corner_SE;
                case CornerType.Diagonal_NW: return mat_Diagonal_NW;
                case CornerType.Diagonal_NE: return mat_Diagonal_NE;
                case CornerType.Diagonal_SW: return mat_Diagonal_SW;
                case CornerType.Diagonal_SE: return mat_Diagonal_SE;
                default: throw new ArgumentOutOfRangeException(nameof(edgeType), edgeType, null);
            }
        }

        private static IntVec3 GetOffset(CornerType cornerType)
        {
            switch (cornerType)
            {
                case CornerType.Corner_NE:
                case CornerType.Diagonal_NE:
                    return new IntVec3(0, 0, 0);
                case CornerType.Corner_NW:
                case CornerType.Diagonal_NW:
                    return new IntVec3(-1, 0, 0);
                case CornerType.Corner_SE:
                case CornerType.Diagonal_SE:
                    return new IntVec3(0, 0, -1);
                case CornerType.Corner_SW:
                case CornerType.Diagonal_SW:
                    return new IntVec3(-1, 0, -1);
                default:
                    return IntVec3.Zero;
            }
        }

        private static void AddQuad(LayerSubMesh sm, Vector3 c, float scale, float altitude, Color color)
        {
            int count = sm.verts.Count;
            for (int i = 0; i < 4; i++)
            {
                sm.verts.Add(new Vector3(c.x + UVs[i].x * scale, altitude, c.z + UVs[i].y * scale));
                sm.uvs.Add(UVs[i % 4]);
                sm.colors.Add(color);
            }
            sm.tris.Add(count);
            sm.tris.Add(count + 1);
            sm.tris.Add(count + 2);
            sm.tris.Add(count);
            sm.tris.Add(count + 2);
            sm.tris.Add(count + 3);
        }

        private void AddQuad(Material mat, IntVec3 c, float scale, float altitude, Color color, bool addGravshipMask, bool addIndoorMask)
        {
            var subMesh = GetSubMesh(mat);
            AddQuad(subMesh, c.ToVector3(), scale, altitude, color);
            if (addGravshipMask)
            {
                var srcTex = subMesh.material.mainTexture as Texture2D;
                var color2 = subMesh.material.color;
                var material = MaterialPool.MatFrom(srcTex, ShaderDatabase.GravshipMaskMasked, color2);
                AddQuad(GetSubMesh(material), c.ToVector3(), scale, altitude, color);
            }
            if (addIndoorMask)
            {
                var srcTex2 = subMesh.material.mainTexture as Texture2D;
                var color3 = subMesh.material.color;
                var material2 = MaterialPool.MatFrom(srcTex2, ShaderDatabase.IndoorMaskMasked, color3);
                AddQuad(GetSubMesh(material2), c.ToVector3(), scale, altitude, color);
            }
        }

        private void AddSubstructure(CornerType cornerType, IntVec3 c, bool substructureToSouth, bool addGravshipMask, bool addIndoorMask)
        {
            if (cornerType == CornerType.Corner_NW || cornerType == CornerType.Diagonal_NW)
            {
                AddQuad(mat_SubStructure_W.Material, c, 1f, substructureAltitude, Color.white, addGravshipMask, addIndoorMask);
                if (!substructureToSouth)
                {
                    AddQuad(mat_SubStructureExtra_W.Material, c + IntVec3.South, 1f, substructureAltitude, Color.white, addGravshipMask, addIndoorMask);
                }
            }
            if (cornerType == CornerType.Corner_NE || cornerType == CornerType.Diagonal_NE)
            {
                AddQuad(mat_SubStructure_E.Material, c, 1f, substructureAltitude, Color.white, addGravshipMask, addIndoorMask);
                if (!substructureToSouth)
                {
                    AddQuad(mat_SubStructureExtra_E.Material, c + IntVec3.South, 1f, substructureAltitude, Color.white, addGravshipMask, addIndoorMask);
                }
            }
        }

        public static bool ShouldDrawCornerPiece(IntVec3 pos, Map map, TerrainGrid terrGrid, out CornerType cornerType, out Color color)
        {
            cornerType = CornerType.None;
            color = Color.white;
            if (pos.GetEdifice(map) != null)
            {
                return false;
            }
            var terrainDef = terrGrid.FoundationAt(pos);
            if (terrainDef != null && terrainDef.IsSubstructure)
            {
                return false;
            }
            TerrainDef combinedDef = terrainDef ?? terrGrid.TerrainAt(pos);
            if (combinedDef != null && combinedDef.GetModExtension<SubstructureEdgeGraphicsExtension>()?.renderAsSubstructure == true)
            {
                return false;
            }
            for (int i = 0; i < Directions.Length; i++)
            {
                tmpChecks[i] = (pos + Directions[i]).GetEdificeSafe(map)?.def == InternalDefOf.VGE_GravshipArmor;
            }
            if (tmpChecks[0] && tmpChecks[3] && !tmpChecks[2] && !tmpChecks[1])
            {
                cornerType = (tmpChecks[4] ? CornerType.Corner_NW : CornerType.Diagonal_NW);
            }
            else if (tmpChecks[0] && tmpChecks[1] && !tmpChecks[2] && !tmpChecks[3])
            {
                cornerType = (tmpChecks[5] ? CornerType.Corner_NE : CornerType.Diagonal_NE);
            }
            else if (tmpChecks[2] && tmpChecks[1] && !tmpChecks[0] && !tmpChecks[3])
            {
                cornerType = (tmpChecks[6] ? CornerType.Corner_SE : CornerType.Diagonal_SE);
            }
            else if (tmpChecks[2] && tmpChecks[3] && !tmpChecks[0] && !tmpChecks[1])
            {
                cornerType = (tmpChecks[7] ? CornerType.Corner_SW : CornerType.Diagonal_SW);
            }
            if (cornerType == CornerType.None)
            {
                return false;
            }
            for (int j = 0; j < directionPairs.Length; j++)
            {
                var list = directionPairs[j].Where(num2 => tmpChecks[num2]).ToList();
                if (list.Count > 0)
                {
                    int num = list.First();
                    color = (pos + Directions[num]).GetEdificeSafe(map).DrawColor;
                    break;
                }
            }
            return true;
        }
    }
}
