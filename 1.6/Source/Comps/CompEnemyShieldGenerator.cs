using RimWorld;

namespace VanillaGravshipExpanded2
{
    public class CompEnemyShieldGenerator : CompProjectileInterceptor
    {
        public override void CompTick()
        {
            base.CompTick();
            if (parent.Spawned && !Active && !OnCooldown && !Charging && currentHitPoints > 0)
            {
                var power = parent.GetComp<CompPowerTrader>();
                if (power.PowerOn)
                {
                    Activate();
                }
            }
        }
    }
}
