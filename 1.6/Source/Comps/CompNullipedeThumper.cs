
using RimWorld;
using System;
using Verse.AI.Group;
using Verse;
using VanillaGravshipExpanded2;
using Verse.Sound;
namespace VanillaGravshipExpanded2
{
    public class CompNullipedeThumper : ThingComp
    {
        public CompProperties_NullipedeThumper Props => (CompProperties_NullipedeThumper)props;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                Messages.Message("VGE_NullipedeThumperNeedsSpace".Translate(), parent,  MessageTypeDefOf.RejectInput);

               
            }
        }
    }
}