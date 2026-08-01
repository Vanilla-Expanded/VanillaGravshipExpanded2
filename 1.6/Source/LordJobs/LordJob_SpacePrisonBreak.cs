using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace VanillaGravshipExpanded2;

public class LordJob_SpacePrisonBreak : LordJob_PrisonBreak
{
    public LordJob_SpacePrisonBreak()
    {
        // Probably used when exposing data
    }

    public LordJob_SpacePrisonBreak(IntVec3 groupUpLoc, IntVec3 exitPoint, int sapperThingID) : base(groupUpLoc, exitPoint, sapperThingID)
    {
        // Used directly (accessed through reflection)
    }

    public override StateGraph CreateGraph()
    {
        var stateGraph = new StateGraph();
        var travelToil = new LordToil_Travel(groupUpLoc)
        {
            maxDanger = Danger.Deadly,
            useAvoidGrid = true
        };

        stateGraph.StartingToil = travelToil;
        var escapeNoDiggingToil = new LordToil_GoToEscapePods(SpaceRebellionsUtility.GetClosestTargetTransportPodTile(Map), LocomotionUrgency.Jog)
        {
            useAvoidGrid = true
        };
        stateGraph.AddToil(escapeNoDiggingToil);

        var getToEscapePodsOnArrived = new Transition(travelToil, escapeNoDiggingToil);
        getToEscapePodsOnArrived.AddTrigger(new Trigger_Memo("TravelArrived"));
        stateGraph.AddTransition(getToEscapePodsOnArrived);

        var getToEscapePodsOnMemberLost = new Transition(travelToil, escapeNoDiggingToil);
        getToEscapePodsOnMemberLost.AddTrigger(new Trigger_PawnLost());
        stateGraph.AddTransition(getToEscapePodsOnMemberLost);

        var tryGettingToPodsAgain = new Transition(escapeNoDiggingToil, escapeNoDiggingToil, true);
        tryGettingToPodsAgain.AddTrigger(new Trigger_PawnLost());
        tryGettingToPodsAgain.AddTrigger(new Trigger_PawnHarmed());
        stateGraph.AddTransition(tryGettingToPodsAgain);

        return stateGraph;
    }
}