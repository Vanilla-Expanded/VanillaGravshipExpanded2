using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2;

[HarmonyPatch(typeof(PrisonBreakUtility), nameof(PrisonBreakUtility.CanParticipateInPrisonBreak))]
public static class PrisonBreakUtility_CanParticipateInPrisonBreak_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr)
    {
        var matcher = new CodeMatcher(instr);

        matcher.MatchEndForward(
            CodeMatch.Calls(typeof(Thing).DeclaredPropertyGetter(nameof(Thing.Map))),
            CodeMatch.Calls(typeof(Map).DeclaredPropertyGetter(nameof(Map.Biome))),
            CodeMatch.LoadsField(typeof(BiomeDef).DeclaredField(nameof(BiomeDef.inVacuum)))
        );

        if (matcher.IsInvalid)
            Log.Error("[VGE] Failed to find vacuum check in PrisonBreakUtility:CanParticipateInPrisonBreak. Space prison breaks won't work.");

        matcher.InsertAfter(
            CodeInstruction.LoadArgument(0),
            CodeInstruction.Call(() => InVacuumWrapper)
        );

        return matcher.Instructions();
    }

    private static bool InVacuumWrapper(bool inVacuum, Pawn pawn)
    {
        // Not in vacuum, don't bother at all
        if (!inVacuum)
            return false;
        // Condition is negated (false = allowed, true = not allowed)
        return !SpaceRebellionsUtility.CanInitiateRebellion(pawn);
    }
}