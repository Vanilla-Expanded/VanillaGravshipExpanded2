using System.Collections.Generic;
using System.Linq;
using LudeonTK;
using RimWorld;
using VanillaGravshipExpanded;
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

        [DebugAction("VGE2", "Force encounter (random)")]
        public static void ForceEncounter()
        {
            if (GravEngineTracker.GetPlayerGravEngine() == null)
            {
                Log.Error("[VGE2] No gravengine found - cannot trigger encounter");
                return;
            }
            WorldComponent_GravshipCombat.Instance.TriggerEncounter();
        }

        [DebugAction("VGE2", "Force encounter (specific)")]
        public static void ForceEncounterSpecific()
        {
            if (GravEngineTracker.GetPlayerGravEngine() == null)
            {
                Log.Error("[VGE2] No gravengine found - cannot trigger encounter");
                return;
            }

            var engine = GravEngineTracker.GetPlayerGravEngine();
            var validThreats = DefDatabase<GravshipThreatDef>.AllDefsListForReading.Where(x => x.Worker.CanFire(engine));
            var list = new List<DebugMenuOption>();
            foreach (var threat in validThreats)
            {
                list.Add(new DebugMenuOption(threat.label ?? threat.defName, DebugMenuOptionMode.Action, () =>
                {
                    WorldComponent_GravshipCombat.Instance.activeThreatDef = threat;
                    threat.Worker.Fire(engine);
                }));
            }
            Find.WindowStack.Add(new Dialog_DebugOptionListLister(list));
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

        [DebugAction("VGE2", "Spawn StructureSet as skyfaller", actionType = DebugActionType.Action, allowedGameStates = AllowedGameStates.PlayingOnMap)]
        private static List<DebugActionNode> SpawnStructureSetSkyfaller()
        {
            var list = new List<DebugActionNode>();
            foreach (var setDef in DefDatabase<VEF.Storyteller.StructureSetDef>.AllDefsListForReading)
            {
                list.Add(new DebugActionNode(setDef.defName, DebugActionType.ToolMap, () =>
                {
                    var map = Find.CurrentMap;
                    if (UI.MouseCell().InBounds(map))
                    {
                        var landingStructure = (LandingStructure_StructureSet)ThingMaker.MakeThing(InternalDefOf.VGE_LandingStructure_StructureSet);
                        landingStructure.structureSetDef = setDef;
                        var standardLayouts = VEF.Storyteller.StructureSetGenerator.SelectStandardLayouts(setDef, 0f);
                        landingStructure.selectedDefs = standardLayouts.Select(x => x.def).ToList();
                        landingStructure.shipFaction = Faction.OfPirates;
                        landingStructure.shipRotation = Rot4.Random;
                        landingStructure.pawnCountRange = new IntRange(3, 6);
                        GenSpawn.Spawn(landingStructure, UI.MouseCell(), map);
                    }
                }));
            }
            return list;
        }
    }
}
