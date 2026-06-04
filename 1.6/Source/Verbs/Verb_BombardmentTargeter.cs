using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;
using VanillaGravshipExpanded;
using Verse.AI;

namespace VanillaGravshipExpanded2
{
    public class Verb_BombardmentTargeter : Verb_CastBase
    {
        public override float EffectiveRange => 55.9f;
        public override bool MultiSelect => false;

        public override bool TryCastShot()
        {
            var targeter = (Apparel_GravshipBombardmentTargeter)EquipmentSource;
            if (targeter.linkedTurret == null || targeter.linkedTurret.Destroyed) return false;
            var comp = targeter.linkedTurret.TryGetComp<CompWorldArtillery>();
            if (targeter.linkedTurret.Map == caster.Map)
            {
                targeter.linkedTurret.OrderAttack(currentTarget);
            }
            else if (comp != null)
            {
                var globalTarget = new GlobalTargetInfo(caster.Map.Parent);
                comp.StartAttack(globalTarget, currentTarget, targeter.linkedTurret);
            }

            targeter.lastFireTick = Find.TickManager.TicksGame;
            InternalDefOf.OrbitalTargeter_Fire.PlayOneShot(targeter.Wearer);
            return true;
        }

        public override void DrawHighlight(LocalTargetInfo target)
        {
            if (caster == null || !caster.Spawned) return;

            base.DrawHighlight(target);
        }

        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = true;

            if (EquipmentSource is Apparel_GravshipBombardmentTargeter targeter && targeter.linkedTurret != null)
            {
                var turret = targeter.linkedTurret;
                var proj = turret.AttackVerb?.verbProps?.defaultProjectile;

                if (proj != null && proj.projectile.explosionRadius > 0f)
                {
                    float radius = proj.projectile.explosionRadius + proj.projectile.explosionRadiusDisplayPadding;

                    float forcedMiss = turret.AttackVerb.verbProps.ForcedMissRadius;
                    if (forcedMiss > 0f && turret.AttackVerb.BurstShotCount > 1)
                    {
                        radius += forcedMiss;
                    }

                    return radius;
                }
            }
            return 0f;
        }

        public override Texture2D UIIcon
        {
            get
            {
                if (EquipmentSource is Apparel_GravshipBombardmentTargeter targeter && targeter.linkedTurret != null)
                {
                    return targeter.linkedTurret.def.uiIcon;
                }
                return base.UIIcon;
            }
        }

        public override bool ValidateTarget(LocalTargetInfo target, bool showMessages = true)
        {
            if (!base.ValidateTarget(target, showMessages))
            {
                return false;
            }

            var targeter = (Apparel_GravshipBombardmentTargeter)EquipmentSource;
            if (targeter?.linkedTurret != null)
            {
                var turret = targeter.linkedTurret;
                if (turret.Map == caster.Map)
                {
                    float dist = (target.Cell - turret.Position).LengthHorizontal;
                    if (dist < turret.AttackVerb.verbProps.minRange)
                    {
                        if (showMessages)
                        {
                            Messages.Message("MessageTargetBelowMinimumRange".Translate(), MessageTypeDefOf.RejectInput, false);
                        }
                        return false;
                    }
                    if (dist > turret.AttackVerb.EffectiveRange)
                    {
                        if (showMessages)
                        {
                            Messages.Message("MessageTargetBeyondMaximumRange".Translate(), MessageTypeDefOf.RejectInput, false);
                        }
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
