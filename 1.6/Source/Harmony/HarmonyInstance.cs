using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using HarmonyLib;
using System.Reflection;
using VanillaGravshipExpanded;
namespace VanillaGravshipExpanded2
{
    [StaticConstructorOnStartup]
    public class Main
    {
        static Main()
        {
            var harmony = new Harmony("com.VanillaGravshipExpanded2");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            Verb_ShootWithWorldTargeting.OnCrossMapShotFired += turret =>
            {
                turret.TryAddVisibility();
            };
            ScenPart_PlayerPawnsArriveMethod_DoGravship_Patch.postGravshipGenerated += (_, _, things, _) =>
            {
                foreach (var spawnedThing in things)
                    spawnedThing?.TryGetComp<CompPower_InputOnlyBattery>()?.SetStoredEnergyPct(1f);
            };
        }
    }
}
