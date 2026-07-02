using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class CompHostileWarcomputer_Hackable : CompHackable
    {
        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
            DisablePlatform(previousMap);
        }

        public override void OnHacked(Pawn hacker = null, bool suppressMessages = false)
        {
            base.OnHacked(hacker, suppressMessages);
            GiveGravdata();
            var storedPos = parent.Position;
            var storedMap = parent.Map;
            var storedRot = parent.Rotation;
            parent.Destroy(DestroyMode.Vanish);
            var extension = parent.def.GetModExtension<WreckedBuildingReplacementExtension>();
            var buildingToMake = GenSpawn.Spawn(ThingMaker.MakeThing(extension.replacementBuilding), storedPos, storedMap);
            buildingToMake.Rotation = storedRot;
            if (buildingToMake.def.CanHaveFaction)
            {
                buildingToMake.SetFaction(parent.Faction);
            }
            var targeter = ThingMaker.MakeThing(InternalDefOf.OrbitalTargeterBombardment);
            GenPlace.TryPlaceThing(targeter, storedPos, storedMap, ThingPlaceMode.Near);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (var gizmo in base.CompGetGizmosExtra())
            {
                if (gizmo is Command_Action cmd && cmd.defaultLabel == "HackGizmo".Translate())
                {
                    var originalAction = cmd.action;
                    cmd.action = () =>
                    {
                        CheckPrintWarning();
                        originalAction?.Invoke();
                    };
                }
                else if (gizmo is Command_Toggle tgl && tgl.defaultLabel == "AutoHack".Translate())
                {
                    var originalToggleAction = tgl.toggleAction;
                    tgl.toggleAction = () =>
                    {
                        if (!Autohack) CheckPrintWarning();
                        originalToggleAction?.Invoke();
                    };
                }
                yield return gizmo;
            }
        }

        public override IEnumerable<FloatMenuOption> CompMultiSelectFloatMenuOptions(IEnumerable<Pawn> selPawns)
        {
            foreach (var opt in base.CompMultiSelectFloatMenuOptions(selPawns))
            {
                if (opt.Label == "Hack".Translate(parent.Label))
                {
                    opt.Label = "VGE_HackWarcomputer".Translate();
                    var originalAction = opt.action;
                    opt.action = () =>
                    {
                        CheckPrintWarning();
                        originalAction?.Invoke();
                    };
                }
                yield return opt;
            }
        }

        public void CheckPrintWarning()
        {
            if (!HasBlackBox() && !HasGravtechProject())
            {
                Messages.Message("VGE_GravdataWillBeLostWarning".Translate(), MessageTypeDefOf.CautionInput, false);
            }
        }

        private bool HasBlackBox()
        {
            foreach (var map in Find.Maps)
            {
                if (map.listerBuildings.allBuildingsColonist.Any(b => b.def == InternalDefOf.VGE_GravshipBlackBox))
                {
                    return true;
                }
            }
            return false;
        }

        private bool HasGravtechProject()
        {
            var proj = World_ExposeData_Patch.currentGravtechProject;
            return proj != null;
        }

        private void GiveGravdata()
        {
            var proj = World_ExposeData_Patch.currentGravtechProject;
            var progress = HasGravtechProject() ? proj.ProgressPercent : 0.2f;

            var gain = Mathf.RoundToInt(1000f * (1f - progress));
            if (gain <= 0) return;

            Building blackBox = null;
            foreach (var map in Find.Maps)
            {
                blackBox = map.listerBuildings.allBuildingsColonist.FirstOrDefault(b => b.def == InternalDefOf.VGE_GravshipBlackBox);
                if (blackBox != null) break;
            }

            if (blackBox is Building_GravshipBlackBox bb)
            {
                bb.AddGravdata(gain);
                Messages.Message("VGE_WarcomputerHackedBlackBox".Translate(gain), MessageTypeDefOf.PositiveEvent, false);
            }
            else if (HasGravtechProject())
            {
                Find.ResearchManager.AddProgress(proj, gain);
                Messages.Message("VGE_WarcomputerHackedResearch".Translate(gain, proj.LabelCap), MessageTypeDefOf.PositiveEvent, false);
            }
        }

        private void DisablePlatform(Map map)
        {
            foreach (var thing in map.listerBuildings.allBuildingsNonColonist.ToList())
            {
                if (thing is Building_GravshipTurret turret)
                {
                    turret.DisablePermanently();
                }
            }
        }
    }
}
