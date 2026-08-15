using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Verse;
using System;
using System.Net;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(JobGiver_MaintainHives))]
    [HarmonyPatch("TryGiveJob")]
    public static class VanillaGravshipExpanded2_JobGiver_MaintainHives_TryGiveJob_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> DetectAllHives(IEnumerable<CodeInstruction> codeInstructions)
        {
            var codes = codeInstructions.ToList();
           
            var detectHiveMethod = AccessTools.Method(typeof(VanillaGravshipExpanded2_JobGiver_MaintainHives_TryGiveJob_Patch), "DetectExoHive");

            for (var i = 0; i < codes.Count; i++)
            {

                if (codes[i].opcode == OpCodes.Stloc_3)
                {
                    yield return codes[i];
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldloc_2);
                    yield return new CodeInstruction(OpCodes.Call, detectHiveMethod);
                    yield return new CodeInstruction(OpCodes.Stloc_3);
                }


                else yield return codes[i];
            }
        }

        public static Hive DetectExoHive(Hive hive, Pawn pawn, IntVec3 intVec)
        {
            Hive returnHive = null;
            if (hive == null) {
                returnHive = (Hive)pawn.Map.thingGrid.ThingAt(intVec, InternalDefOf.VGE_ExoHive_Building);
            }
            return returnHive;       
        }

    }
}