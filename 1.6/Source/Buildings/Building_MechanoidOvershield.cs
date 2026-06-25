using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class Building_MechanoidOvershield : Building
    {
        private CompProjectileInterceptor shieldComp;
        private Graphic activeGraphic;
        private Graphic ActiveGraphic => activeGraphic ??= GraphicDatabase.Get<Graphic_Single>("Things/Mechanoid/MechanoidOvershieldGenerator/MechanoidOvershieldGenerator", ShaderDatabase.Cutout, def.graphicData.drawSize, Color.white);
        private Graphic inactiveGraphic;
        private Graphic InactiveGraphic => inactiveGraphic ??= GraphicDatabase.Get<Graphic_Single>("Things/Mechanoid/MechanoidOvershieldGenerator/MechanoidOvershieldGenerator_Off", ShaderDatabase.Cutout, def.graphicData.drawSize, Color.white);
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            shieldComp = GetComp<CompProjectileInterceptor>();
        }

        private bool? wasActive;

        public override void Tick()
        {
            base.Tick();
            if (Spawned)
            {
                var isActive = shieldComp.Active;
                if (wasActive == null || wasActive != isActive)
                {
                    wasActive = isActive;
                    Map.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Things);
                }
            }
        }

        public override Graphic Graphic => shieldComp.Active ? ActiveGraphic : InactiveGraphic;
    }
}
