using UnityEngine;
using VanillaGravshipExpanded;

namespace VanillaGravshipExpanded2
{
    public class Building_EnemyJavelinPod : Building_EnemyMechTurret
    {
        public override Material TurretTopMaterial
        {
            get
            {
                if (burstCooldownTicksLeft <= 0)
                    return def.building.turretGunDef.building.turretTopLoadedMat;
                return base.TurretTopMaterial;
            }
        }
    }
}
