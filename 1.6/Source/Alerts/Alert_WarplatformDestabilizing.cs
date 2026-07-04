using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class Alert_WarplatformDestabilizing : Alert_Warplatform
    {
        protected override bool ShouldReport(MapParent_WarPlatform wp) => wp.defeated && wp.despawnTick > 0 && (wp.despawnTick - Find.TickManager.TicksGame) < 12 * GenDate.TicksPerHour && wp.Map.mapPawns.AnyFreeColonistSpawned;
        public override string GetLabel() => "VGE_LocationDestabilizing".Translate();

        public override TaggedString GetExplanation()
        {
            var wp = FindWarplatform(ShouldReport);
            return "VGE_LocationDestabilizingDesc".Translate((wp.despawnTick - Find.TickManager.TicksGame).ToStringTicksToPeriod());
        }
    }
}
