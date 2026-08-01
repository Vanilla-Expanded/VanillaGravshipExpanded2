using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded2;

public class JobGiver_SpacePrisonEscape_UseEscapePod : ThinkNode_JobGiver
{
    public override Job TryGiveJob(Pawn pawn)
    {
        if (pawn.mindState.duty.focus.Thing == null)
            return null;
        return JobMaker.MakeJob(InternalDefOf.VGE_SpacePrisonEscape_UseEscapePod, pawn.mindState.duty.focus.Thing);
    }
}