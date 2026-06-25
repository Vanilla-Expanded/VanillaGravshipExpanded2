using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2
{
    [StaticConstructorOnStartup]
    public class Building_MechanoidGravTether : Building
    {
        private static readonly Material OrbMaterial = MaterialPool.MatFrom("Things/Mechanoid/MechanoidLargeGravEngine/MechanoidLargeGravEngine_Orb", ShaderDatabase.Cutout);

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            base.DrawAt(drawLoc, flip);
            drawLoc.y += 0.03658537f;
            drawLoc.z += 0.5f * (1f + Mathf.Sin((float)System.Math.PI * 2f * (float)GenTicks.TicksGame / 500f)) * 0.3f;
            Graphics.DrawMesh(MeshPool.plane10Back, Matrix4x4.TRS(drawLoc, base.Rotation.AsQuat, new Vector3(def.graphicData.drawSize.x, 1f, def.graphicData.drawSize.y)), OrbMaterial, 0, null, 0);
        }
    }
}
