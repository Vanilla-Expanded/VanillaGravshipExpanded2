using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(Pawn_JobTracker), nameof(Pawn_JobTracker.StartJob))]
    public static class Pawn_JobTracker_StartJob_Patch
    {
        public static void Postfix(Job newJob)
        {
            if (newJob != null && newJob.def == JobDefOf.Hack && newJob.playerForced && newJob.targetA.Thing.TryGetComp<CompHostileWarcomputer_Hackable>() != null)
            {
                newJob.targetA.Thing.TryGetComp<CompHostileWarcomputer_Hackable>().CheckPrintWarning();
            }
        }
    }
}
