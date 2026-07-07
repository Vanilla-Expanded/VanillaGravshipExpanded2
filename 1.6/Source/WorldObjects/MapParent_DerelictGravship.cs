using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded2
{
    public enum DerelictThreatType
    {
        Exoinsectoids,
        Mechanoids,
        Drones,
        Pirates
    }

    public class MapParent_DerelictGravship : SpaceMapParent
    {
        public DerelictThreatType threatType;
        public float threatPoints;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref threatType, "threatType");
            Scribe_Values.Look(ref threatPoints, "threatPoints");
        }
    }
}
