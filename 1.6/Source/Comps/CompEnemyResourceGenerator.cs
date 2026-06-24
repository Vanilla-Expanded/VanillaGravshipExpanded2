using PipeSystem;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class CompProperties_EnemyResourceGenerator : CompProperties
    {
        public int generateAmount = 1;
        public int generateInterval = 120;
        public CompProperties_EnemyResourceGenerator() => compClass = typeof(CompEnemyResourceGenerator);
    }

    public class CompEnemyResourceGenerator : ThingComp
    {
        public CompProperties_EnemyResourceGenerator Props => (CompProperties_EnemyResourceGenerator)props;
        private CompPowerTrader power;
        private CompResourceTrader resourceTrader;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            power = parent.GetComp<CompPowerTrader>();
            resourceTrader = parent.GetComp<CompResourceTrader>();
        }

        public override void CompTick()
        {
            base.CompTick(); 
            if (power.PowerOn && parent.IsHashIntervalTick(Props.generateInterval) && resourceTrader.PipeNet != null)
            {
                resourceTrader.PipeNet.DistributeAmongStorage(Props.generateAmount, out _);
            }
        }
    }
}
