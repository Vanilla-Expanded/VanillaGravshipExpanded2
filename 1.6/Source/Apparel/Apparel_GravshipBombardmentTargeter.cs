using System.Collections.Generic;
using RimWorld;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class Apparel_GravshipBombardmentTargeter : Apparel, ITurretLinker
    {
        public Building_GravshipTurret linkedTurret;
        public int lastFireTick = -999999;
        public const int CooldownTicks = 3600;
        public float LinkRange => 36f;

        public Thing LinkerThing => Wearer;
        public IntVec3 LinkerPosition => Wearer != null ? Wearer.Position : Position;
        public bool MannedByPlayer => Wearer != null;
        public float GravshipTargeting => Wearer != null ? Wearer.GetStatValue(InternalDefOf.VGE_GravshipTargeting) : 0f;
        public Pawn ManningPawn => Wearer;

        public IEnumerable<Building_GravshipTurret> LinkedTurrets
        {
            get
            {
                if (linkedTurret != null)
                {
                    yield return linkedTurret;
                }
            }
        }

        public int MaxLinkedTurrets => 1;
        public string OnlyArtilleryErrorKey => "VGE_TargetingTerminalCanOnlyLinkWithGravshipArtillery";
        public string LinkGizmoDesc => "VGE_LinkWithTurretTargeterDesc".Translate();
        public string UnlinkGizmoDesc => "VGE_UnlinkWithTurretTargeterDesc".Translate();
        public string SelectGizmoDesc => "VGE_SelectLinkedTurretTargeterDesc".Translate();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref linkedTurret, "linkedTurret");
            Scribe_Values.Look(ref lastFireTick, "lastFireTick", -999999);
        }

        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            foreach (var gizmo in base.GetWornGizmos())
            {
                if (gizmo is Command_VerbTarget command && linkedTurret != null)
                {
                    command.defaultLabel = "VGE_FireBombardmentTargeter".Translate(linkedTurret.LabelNoParenthesis);
                    command.defaultDesc = "VGE_FireBombardmentTargeterDesc".Translate(linkedTurret.LabelNoParenthesis);
                    command.icon = linkedTurret.def.uiIcon;
                }
                yield return gizmo;
            }
            foreach (var gizmo in TurretLinkerUtility.GetLinkerGizmos(this, LinkRange))
            {
                yield return gizmo;
            }
        }

        public void LinkTo(Building_GravshipTurret turret)
        {
            linkedTurret = turret;
            if (turret.linkedTerminal != this)
            {
                turret.LinkTo(this);
            }
        }

        public void Unlink(Building_GravshipTurret turret)
        {
            if (linkedTurret == turret)
            {
                linkedTurret = null;
                turret.unlinking = true;
                turret.Unlink();
                turret.unlinking = false;
            }
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            if (linkedTurret != null)
            {
                Unlink(linkedTurret);
            }
        }

        public void EnableOverlay()
        {
        }

        public void DisableOverlay()
        {
        }
    }
}
