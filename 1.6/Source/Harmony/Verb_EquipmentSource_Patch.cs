using HarmonyLib;
using Verse;
using RimWorld;

namespace VanillaGravshipExpanded2
{
    [HarmonyPatch(typeof(Verb), "EquipmentSource", MethodType.Getter)]
    public static class Verb_EquipmentSource_Patch
    {
        public static void Postfix(Verb __instance, ref ThingWithComps __result)
        {
            if (__result == null && __instance.DirectOwner is CompApparelVerbOwner comp)
            {
                __result = comp.parent;
            }
        }
    }
}
