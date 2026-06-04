using RimWorld;
using Verse;
using VanillaGravshipExpanded;

namespace VanillaGravshipExpanded2
{
    public class CompApparelVerbOwner_BombardmentTargeter : CompApparelVerbOwner
    {
        public override bool CanBeUsed(out string reason)
        {
            if (!base.CanBeUsed(out reason))
            {
                return false;
            }

            var targeter = (Apparel_GravshipBombardmentTargeter)parent;
            if (targeter.linkedTurret == null || targeter.linkedTurret.Destroyed)
            {
                reason = "VGE_NeedsLinkedTargetingTerminal".Translate();
                return false;
            }

            if (Find.TickManager.TicksGame < targeter.lastFireTick + Apparel_GravshipBombardmentTargeter.CooldownTicks)
            {
                reason = "VGE_TargeterOnCooldown".Translate((targeter.lastFireTick + Apparel_GravshipBombardmentTargeter.CooldownTicks - Find.TickManager.TicksGame).ToStringSecondsFromTicks());
                return false;
            }

            if (targeter.linkedTurret.refuelableComp != null && !targeter.linkedTurret.refuelableComp.HasFuel)
            {
                reason = "VGE_TargeterNoAmmo".Translate();
                return false;
            }

            if (!targeter.linkedTurret.CanFire)
            {
                reason = "VGE_NeedsMannedTargetingTerminal".Translate();
                return false;
            }

            var wearer = Wearer;
            if (wearer?.Map == null)
            {
                reason = "VGE_NeedsLinkedTargetingTerminal".Translate();
                return false;
            }
            if (targeter.linkedTurret.Map == wearer.Map)
            {
                float dist = wearer.Position.DistanceTo(targeter.linkedTurret.Position);
                float maxRange = targeter.linkedTurret.AttackVerb.EffectiveRange;
                if (dist > maxRange)
                {
                    reason = "VGE_TargeterOutOfRange".Translate();
                    return false;
                }
            }
            else
            {
                var comp = targeter.linkedTurret.TryGetComp<CompWorldArtillery>();
                float dist = comp != null ? GravshipHelper.GetDistance(targeter.linkedTurret.Map.Tile, wearer.Map.Tile) : 99999f;
                float maxRange = comp != null ? comp.Props.worldMapAttackRange : 0f;
                if (comp == null || dist > maxRange)
                {
                    reason = "VGE_TargeterOutOfRange".Translate();
                    return false;
                }
            }

            return true;
        }
    }
}
