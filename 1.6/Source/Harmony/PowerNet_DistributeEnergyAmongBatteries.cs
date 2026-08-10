using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2;

[HarmonyPatch(typeof(PowerNet), nameof(PowerNet.DistributeEnergyAmongBatteries))]
public static class PowerNet_DistributeEnergyAmongBatteries_Patch
{
    private static void Prefix(PowerNet __instance, ref float energy)
    {
        if (energy <= 0f)
            return;

        var batteries = __instance.Map.GetComponent<VGE2_MapComponent>()?.unchargedBatteries;
        if (batteries is not { Count: > 0 })
            return;

        batteries.Shuffle();

        for (var tries = 0;; tries++)
        {
            // Too many attempts, break
            if (tries > 10000)
            {
                Log.Error("Too many iterations");
                break;
            }

            // Grab the lowest count we can add to any battery
            var minAmount = float.MaxValue;
            for (var i = 0; i < batteries.Count; i++)
                minAmount = Mathf.Min(minAmount, batteries[i].AmountCanAccept);

            // Try to fill up all batteries by splitting all the extra energy equally
            if (energy < minAmount * batteries.Count)
            {
                var addPerBattery = energy / batteries.Count;
                for (var i = 0; i < batteries.Count; i++)
                    batteries[i].AddEnergy(addPerBattery);
                energy = 0f;
                break;
            }

            // Fill up batteries by minimum amount possible, while removing all full batteries or batteries matching min amount
            for (var i = batteries.Count - 1; i >= 0; i--)
            {
                var amount = batteries[i].AmountCanAccept;
                var shouldRemove = amount <= 0f;
                if (minAmount > 0)
                    batteries[i].AddEnergy(minAmount);
                if (shouldRemove)
                    batteries.RemoveAt(i);
            }

            // If there's basically no power, or batteries are full, break
            if (energy < 0.0005f || batteries.Count <= 0)
                break;
        }
    }
}