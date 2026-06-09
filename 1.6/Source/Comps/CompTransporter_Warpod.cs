using System.Collections.Generic;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class CompTransporter_Warpod : CompTransporter
    {
        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (Props.massCapacity > 0)
            {
                foreach (var gizmo in base.CompGetGizmosExtra())
                {
                    yield return gizmo;
                }
            }
        }
    }
}