using RimWorld;

namespace VanillaGravshipExpanded2
{
    public class HistoryAutoRecorderWorker_GravshipVisibility : HistoryAutoRecorderWorker
    {
        public override float PullRecord()
        {
            return WorldComponent_GravshipVisibility.Instance.visibility;
        }
    }
}
