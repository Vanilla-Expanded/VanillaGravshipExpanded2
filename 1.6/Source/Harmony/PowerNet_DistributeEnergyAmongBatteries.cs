using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2;

[HarmonyPatch(typeof(PowerNet), nameof(PowerNet.DistributeEnergyAmongBatteries))]
public static class PowerNet_DistributeEnergyAmongBatteries_Patch
{
    private static readonly List<CompPower_InputOnlyBattery> batteriesShuffled = [];

    private static void Prefix(PowerNet __instance, ref float energy)
    {
        if (energy <= 0f)
            return;

        var comp = __instance.Map.GetComponent<VGE2_MapComponent>();
        if (comp == null || comp.batteries.Count <= 0)
            return;

        batteriesShuffled.Clear();
        batteriesShuffled.AddRange(comp.batteries.Where(c => c.PowerNet == __instance && c.AmountCanAccept > 0f));
        if (batteriesShuffled.Count <= 0)
            return;
        batteriesShuffled.Shuffle();

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
            for (var i = 0; i < batteriesShuffled.Count; i++)
                minAmount = Mathf.Min(minAmount, batteriesShuffled[i].AmountCanAccept);

            // Try to fill up all batteries by splitting all the extra energy equally
            if (energy < minAmount * batteriesShuffled.Count)
            {
                var addPerBattery = energy / batteriesShuffled.Count;
                for (var i = 0; i < batteriesShuffled.Count; i++)
                    batteriesShuffled[i].AddEnergy(addPerBattery);
                energy = 0f;
                break;
            }

            // Fill up batteries by minimum amount possible, while removing all full batteries or batteries matching min amount
            for (var i = batteriesShuffled.Count - 1; i >= 0; i--)
            {
                var amount = batteriesShuffled[i].AmountCanAccept;
                var shouldRemove = amount <= 0f || amount == minAmount;
                if (minAmount > 0)
                    batteriesShuffled[i].AddEnergy(minAmount);
                if (shouldRemove)
                    batteriesShuffled.RemoveAt(i);
            }

            // If there's basically no power, or batteries are full, break
            if (energy < 0.0005f || batteriesShuffled.Count <= 0)
                break;
        }

        batteriesShuffled.Clear();
    }
}