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

        return matcher.Instructions();
    }
}