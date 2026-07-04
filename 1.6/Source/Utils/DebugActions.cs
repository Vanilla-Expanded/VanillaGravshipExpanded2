using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using Verse;

namespace VanillaGravshipExpanded2
{
    public static class DebugActions
    {
        [DebugAction("VGE2", "Set visibility...")]
        public static void SetVisibility()
        {
            var comp = WorldComponent_GravshipCombat.Instance;
            var list = new List<DebugMenuOption>
            {
                new DebugMenuOption("0", DebugMenuOptionMode.Action, () => comp.RemoveVisibility(comp.visibility)),
                new DebugMenuOption("400000 (warning 1)", DebugMenuOptionMode.Action, () =>
                {
                    comp.RemoveVisibility(comp.visibility);
                    comp.AddVisibility(400000f);
                }),
                new DebugMenuOption("600000 (warning 2)", DebugMenuOptionMode.Action, () =>
                {
                    comp.RemoveVisibility(comp.visibility);
                    comp.AddVisibility(600000f);
                }),
                new DebugMenuOption("800000 (detection)", DebugMenuOptionMode.Action, () =>
                {
                    comp.RemoveVisibility(comp.visibility);
                    comp.AddVisibility(800000f);
                })
            };
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
        }

        [DebugAction("VGE2", "Add visibility...")]
        public static void AddVisibility()
        {
            var comp = WorldComponent_GravshipCombat.Instance;
            var list = new List<DebugMenuOption>
            {
                new DebugMenuOption("100000", DebugMenuOptionMode.Action, () =>
                {
                    comp.AddVisibility(100000f);
                }),
                new DebugMenuOption("200000", DebugMenuOptionMode.Action, () =>
                {
                    comp.AddVisibility(200000f);
                }),
                new DebugMenuOption("300000", DebugMenuOptionMode.Action, () =>
                {
                    comp.AddVisibility(300000f);
                }),
                new DebugMenuOption("500000", DebugMenuOptionMode.Action, () =>
                {
                    comp.AddVisibility(500000f);
                }),
            };
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
        }

        [DebugAction("VGE2", "Force encounter")]
        public static void ForceEncounter()
        {
            if (WorldComponent_GravshipCombat.GetActiveGravEngine == null)
            {
                Log.Error("[VGE2] No gravengine found - cannot trigger encounter");
                return;
            }
            WorldComponent_GravshipCombat.Instance.TriggerEncounter();
        }

        [DebugAction("VGE2", "Force defeat warplatform")]
        public static void ForceDefeatWarplatform()
        {
            var warplatform = Find.WorldObjects.AllWorldObjects.OfType<MapParent_WarPlatform>().FirstOrDefault();
            if (warplatform == null)
            {
                Log.Error("[VGE2] No warplatform found");
                return;
            }
            warplatform.Defeat();
        }
    }
}
