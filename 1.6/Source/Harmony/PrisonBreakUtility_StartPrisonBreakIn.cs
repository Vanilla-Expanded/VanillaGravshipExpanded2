using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2;

[HarmonyPatch(typeof(PrisonBreakUtility), nameof(PrisonBreakUtility.StartPrisonBreakIn), typeof(Room), typeof(List<Pawn>), typeof(int), typeof(HashSet<Room>))]
public static class PrisonBreakUtility_StartPrisonBreakIn_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instr, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instr, generator);

        matcher.MatchStartForward(
            new CodeMatch(OpCodes.Newobj, typeof(LordJob_PrisonBreak).DeclaredConstructor([typeof(IntVec3), typeof(IntVec3), typeof(int)]))
        );

        matcher.CreateLabel(out var ctorLabel);
        matcher.CreateLabelWithOffsets(1, out var afterCtorLabel);

        matcher.Insert(
            // room.Map.Biome.inVacuum
            CodeInstruction.LoadArgument(0),
            new CodeInstruction(OpCodes.Callvirt, typeof(Room).DeclaredPropertyGetter(nameof(Room.Map))),
            new CodeInstruction(OpCodes.Callvirt, typeof(Map).DeclaredPropertyGetter(nameof(Map.Biome))),
            CodeInstruction.LoadField(typeof(BiomeDef), nameof(BiomeDef.inVacuum)),
            // If false, jump to the original instruction
            new CodeInstruction(OpCodes.Brfalse_S, ctorLabel),
            // If true, use our constructor and jump over the original one
            new CodeInstruction(OpCodes.Newobj, typeof(LordJob_SpacePrisonBreak).DeclaredConstructor([typeof(IntVec3), typeof(IntVec3), typeof(int)])),
            new CodeInstruction(OpCodes.Br_S, afterCtorLabel)
        );

        matcher.Reset();

        matcher.MatchStartForward(
            CodeMatch.LoadsField(typeof(PrisonBreakUtility).DeclaredField(nameof(PrisonBreakUtility.escapingPrisonersGroup))),
            CodeMatch.LoadsConstant(0)
        );

        var currentInstrLabels = matcher.Instruction.ExtractLabels();
        matcher.CreateLabel(out var randomEscapeSpotLabel);
        // Skip over the next 2 return statements and make a label right after
        matcher.CreateLabelAt(matcher.Clone().MatchStartForward(new CodeMatch(OpCodes.Ret)).Advance().MatchStartForward(new CodeMatch(OpCodes.Ret)).Advance().Pos, out var instrAfterRetLabel);

        matcher.Insert(
            // Load the room arg
            CodeInstruction.LoadArgument(0).WithLabels(currentInstrLabels),
            // Load the room's map
            new CodeInstruction(OpCodes.Callvirt, typeof(Room).DeclaredPropertyGetter(nameof(Room.Map))),
            // Load the map's biome
            new CodeInstruction(OpCodes.Callvirt, typeof(Map).PropertyGetter(nameof(Map.Biome))),
            // Check if biome is in vacuum
            new CodeInstruction(OpCodes.Ldfld, typeof(BiomeDef).DeclaredField(nameof(BiomeDef.inVacuum))),
            // Jump over to code looking for escape tile if not in vacuum
            new CodeInstruction(OpCodes.Brfalse_S, randomEscapeSpotLabel),
            // Set the escape spot to invalid
            new CodeInstruction(OpCodes.Ldsfld, typeof(IntVec3).DeclaredField(nameof(IntVec3.Invalid))),
            new CodeInstruction(OpCodes.Stloc_0),
            // Set the group up spot to invalid
            new CodeInstruction(OpCodes.Ldsfld, typeof(IntVec3).DeclaredField(nameof(IntVec3.Invalid))),
            new CodeInstruction(OpCodes.Stloc_1),
            // Jump over the code searching for the escape tile and the return
            new CodeInstruction(OpCodes.Br, instrAfterRetLabel)
        );

        return matcher.Instructions();
    }
}