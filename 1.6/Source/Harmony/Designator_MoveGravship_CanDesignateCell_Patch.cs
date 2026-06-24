using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(Designator_MoveGravship), nameof(Designator_MoveGravship.IsValidCell))]
    public static class Designator_MoveGravship_IsValidCell_Patch
    {
        public static void Postfix(IntVec3 cell, Map map, ref AcceptanceReport __result)
        {
            if (IsCellJammed(cell, map))
            {
                __result = new AcceptanceReport("VGE_DestinationJammed".Translate());
                return;
            }
        }

        public static bool IsCellJammed(IntVec3 cell, Map map)
        {
            if (cell.IsValid && map != null)
            {
                foreach (var thing in map.listerThings.ThingsOfDef(InternalDefOf.VGE_EnemySignalJammer))
                {
                    if (thing.TryGetComp<CompPowerTrader>().PowerOn && thing.Position.DistanceTo(cell) <= 55.9f)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
