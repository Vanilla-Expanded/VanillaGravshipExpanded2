using HarmonyLib;
using RimWorld;
using VanillaGravshipExpanded;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(JobGiver_AIFightEnemy), "TryGiveJob")]
    public static class JobGiver_AIFightEnemy_TryGiveJob_Patch
    {
        public static void Postfix(JobGiver_AIFightEnemy __instance, Pawn pawn, ref Job __result)
        {
            if (__result != null || pawn.Faction == null) return;

            if (pawn.skills != null && !pawn.skills.GetSkill(SkillDefOf.Intellectual).TotallyDisabled)
            {
                var terminal = GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.InteractionCell, TraverseParms.For(pawn), 30f, t => t.Faction == pawn.Faction && t.TryGetComp<CompEnemyTerminal>() != null && pawn.CanReserve(t) && t.TryGetComp<CompMannable>() is CompMannable m && !m.MannedNow);
                if (terminal != null)
                {
                    __result = JobMaker.MakeJob(InternalDefOf.VGE_OperateEnemyTerminal, terminal);
                    return;
                }
            }
            if (pawn.skills != null && !pawn.skills.GetSkill(SkillDefOf.Construction).TotallyDisabled)
            {
                var building = (Building)GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial), PathEndMode.Touch, TraverseParms.For(pawn), 30f, x => x.Faction == pawn.Faction && x.HitPoints < x.MaxHitPoints && pawn.CanReserve(x));
                if (building != null)
                {
                    __result = JobMaker.MakeJob(JobDefOf.Repair, building);
                }
            }
        }
    }
}
