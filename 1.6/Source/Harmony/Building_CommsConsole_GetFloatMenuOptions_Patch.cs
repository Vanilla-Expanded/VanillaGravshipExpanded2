using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(Building_CommsConsole), "GetFloatMenuOptions")]
    public static class Building_CommsConsole_GetFloatMenuOptions_Patch
    {
        public static IEnumerable<FloatMenuOption> Postfix(IEnumerable<FloatMenuOption> __result, Pawn myPawn, Building_CommsConsole __instance)
        {
            foreach (var opt in __result)
            {
                yield return opt;
            }

            var comp = WorldComponent_GravshipCombat.Instance;
            if (comp.tributeDemandTick > 0)
            {
                yield return new FloatMenuOption("VGE_CallSalvagerStation".Translate(comp.salvagerStationName), delegate
                {
                    var job = JobMaker.MakeJob(InternalDefOf.VGE_CallSalvagerStation, __instance);
                    myPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                    PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.OpeningComms, KnowledgeAmount.Total);
                }, Faction.OfSalvagers.def.FactionIcon, Faction.OfSalvagers.Color, MenuOptionPriority.InitiateSocial);
            }
        }
    }
}
