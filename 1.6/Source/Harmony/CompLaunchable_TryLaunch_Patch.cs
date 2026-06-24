using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(CompLaunchable), nameof(CompLaunchable.TryLaunch))]
    public static class CompLaunchable_TryLaunch_Patch
    {
        public static bool Prefix(PlanetTile destinationTile, TransportersArrivalAction arrivalAction)
        {
            if (arrivalAction is TransportersArrivalAction_LandInSpecificCell landAction)
            {
                var mapParent = Find.WorldObjects.MapParentAt(destinationTile);
                if (mapParent != null && mapParent.HasMap)
                {
                    var cell = landAction.cell;
                    if (Designator_MoveGravship_IsValidCell_Patch.IsCellJammed(cell, mapParent.Map))
                    {
                        Messages.Message("VGE_DestinationJammedDropPod".Translate(), MessageTypeDefOf.RejectInput, false);
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
