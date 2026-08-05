using HarmonyLib;

using RimWorld;
using System;
using System.Collections.Generic;
using Verse;
using Verse.Noise;

namespace VanillaGravshipExpanded2;

[HarmonyPatch(typeof(Building), nameof(Building.Destroy))]
public static class VanillaGravshipExpanded2_Building_Destroy_Patch
{
    public static Map storedMap;
    public static IntVec3 storedPos;
    public static Rot4 storedRot;
    public static bool chance = false;

    private static void Prefix(DestroyMode mode, Building __instance)
    {
        chance = Rand.Chance(0.8f);
        if (chance && mode == DestroyMode.KillFinalize)
        {
            storedMap = __instance.Map;
            storedPos = __instance.Position;
            storedRot = __instance.Rotation;
        }
    }

    private static void Postfix(DestroyMode mode, Building __instance)
    {
        if (mode == DestroyMode.KillFinalize && chance)
        {
            WreckedBuildingReplacementExtension extension = __instance.def.GetModExtension<WreckedBuildingReplacementExtension>();
            if (extension != null && storedMap != null)
            {
                Rot4 rotation = storedRot;
                Thing buildingToMake = GenSpawn.Spawn(ThingMaker.MakeThing(extension.replacementBuilding),
                    storedPos, storedMap);
                buildingToMake.Rotation = rotation;
                if (buildingToMake.def.CanHaveFaction)
                {
                    buildingToMake.SetFaction(__instance.Faction);
                }
            }
        }
        if (__instance.Faction == Faction.OfSalvagers)
        {
            WorldComponent_GravshipCombat.Instance.CheckLocalGravjumperDefeated(storedMap);
        }
    }
}

[HarmonyPatch(typeof(GenLeaving), nameof(GenLeaving.DoLeavingsFor))]
[HarmonyPatch(new Type[] { typeof(Thing), typeof(Map), typeof(DestroyMode), typeof(List<Thing>) })]
public static class VanillaGravshipExpanded2_GenLeaving_DoLeavingsFor_Patch
{

    private static bool Prefix(Thing diedThing, DestroyMode mode)
    {
        if (mode == DestroyMode.KillFinalize && VanillaGravshipExpanded2_Building_Destroy_Patch.chance)
        {
            WreckedBuildingReplacementExtension extension = diedThing.def.GetModExtension<WreckedBuildingReplacementExtension>();
            if (extension != null )
            {
                return false;
            }
        }
        return true;
    }
}
