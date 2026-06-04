using Verse;
using RimWorld.Planet;

namespace VanillaGravshipExpanded2
{
    [HotSwappable]
    public class EmptySpace : MapParent
    {
        public override MapGeneratorDef MapGeneratorDef => InternalDefOf.VGE_EmptySpace;
        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            alsoRemoveWorldObject = false;
            return false;
        }
    }
}
