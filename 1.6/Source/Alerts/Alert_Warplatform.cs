using System.Linq;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    public abstract class Alert_Warplatform : Alert_Critical
    {
        protected abstract bool ShouldReport(MapParent_WarPlatform wp);

        protected static MapParent_WarPlatform FindWarplatform(System.Func<MapParent_WarPlatform, bool> predicate)
        {
            return Find.WorldObjects.AllWorldObjects.OfType<MapParent_WarPlatform>().FirstOrDefault(predicate);
        }

        public override AlertReport GetReport()
        {
            var wp = FindWarplatform(ShouldReport);
            if (wp != null) return AlertReport.CulpritIs(wp);
            return AlertReport.Inactive;
        }

        public override void OnClick()
        {
            var wp = FindWarplatform(ShouldReport);
            if (wp != null && wp.HasMap) CameraJumper.TryJump(wp.Map.Center, wp.Map);
        }
    }
}
