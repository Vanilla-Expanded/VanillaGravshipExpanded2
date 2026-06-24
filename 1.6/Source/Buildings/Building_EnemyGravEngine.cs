using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2
{
	public class Building_EnemyGravEngine : Building
	{
		private Material _mat;
		private Material OrbMaterial => _mat ??= GetOrbMaterial();
		private Material GetOrbMaterial()
		{
			if (def == InternalDefOf.VGE_EnemyGravjumperEngine)
				return MaterialPool.MatFrom("Things/Enemy/EnemyGravJumperEngine/EnemyGravJumperEngine_Orb");
			if (def == InternalDefOf.VGE_EnemyGravEngine)
				return MaterialPool.MatFrom("Things/Enemy/EnemyGravEngine/EnemyGravEngine_Orb");
			if (def == InternalDefOf.VGE_EnemyGravhulkEngine)
				return MaterialPool.MatFrom("Things/Enemy/EnemyGravhulkEngine/EnemyGravHulkEngine_Orb");
			if (def == InternalDefOf.VGE_EnemyGravlockTether)
				return MaterialPool.MatFrom("Things/Enemy/EnemyGravlockTether/EnemyGravlockTether_Orb");
			return BaseContent.ClearMat;
		}

		public override void DrawAt(Vector3 drawLoc, bool flip = false)
		{
			base.DrawAt(drawLoc, flip);
			drawLoc.y += 0.03658537f;
			drawLoc.z += 0.5f * (1f + Mathf.Sin((float)System.Math.PI * 2f * (float)GenTicks.TicksGame / 500f)) * 0.3f;
			var s = new Vector3(def.graphicData.drawSize.x, 1f, def.graphicData.drawSize.y);
			Graphics.DrawMesh(MeshPool.plane10Back, Matrix4x4.TRS(drawLoc, base.Rotation.AsQuat, s), OrbMaterial, 0, null, 0);
		}
	}
}
