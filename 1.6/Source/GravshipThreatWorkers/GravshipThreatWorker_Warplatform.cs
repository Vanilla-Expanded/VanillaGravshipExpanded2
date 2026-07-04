
using RimWorld;

namespace VanillaGravshipExpanded2
{
    public class GravshipThreatWorker_Warplatform : GravshipThreatWorker
    {
        public override Faction EnemyFaction => Faction.OfAncientsHostile ?? base.EnemyFaction;
    }
}
