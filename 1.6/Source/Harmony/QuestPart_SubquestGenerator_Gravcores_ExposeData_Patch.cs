using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(QuestPart_SubquestGenerator_Gravcores), nameof(QuestPart_SubquestGenerator_Gravcores.ExposeData))]
    public static class QuestPart_SubquestGenerator_Gravcores_ExposeData_Patch
    {
        public static void Postfix(QuestPart_SubquestGenerator_Gravcores __instance)
        {
            if (Scribe.mode != LoadSaveMode.LoadingVars)
            {
                return;
            }
            var gravShipRoot = (QuestNode_Root_GravShip)QuestScriptDefOf.GravEngine.root;
            foreach (var def in gravShipRoot.subquestDefs)
            {
                if (!__instance.subquestDefs.Contains(def))
                {
                    __instance.subquestDefs.Add(def);
                }
            }
        }
    }
}
