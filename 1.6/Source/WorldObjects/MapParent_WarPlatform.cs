using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class MapParent_WarPlatform : SpaceMapParent
    {
        public bool defeated;
        public int despawnTick = -1;
        public int playerDestroyedTick = -1;
        public int closingInCheckTick = -1;
        public GravshipThreatDef threatDef;
        public string customLabel;
        public string customDescription;
        public override string Label => customLabel ?? base.Label;
        public override string GetDescription() => customDescription ?? base.GetDescription();

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref threatDef, "threatDef");
            Scribe_Values.Look(ref defeated, "defeated", false);
            Scribe_Values.Look(ref despawnTick, "despawnTick", -1);
            Scribe_Values.Look(ref playerDestroyedTick, "playerDestroyedTick", -1);
            Scribe_Values.Look(ref closingInCheckTick, "closingInCheckTick", -1);
            Scribe_Values.Look(ref customLabel, "customLabel");
            Scribe_Values.Look(ref customDescription, "customDescription");
        }

        public override void Tick()
        {
            base.Tick();
            if (!HasMap) return;
            if (closingInCheckTick == -1 && threatDef == InternalDefOf.VGE_EnemyGravjumper && !defeated)
            {
                closingInCheckTick = Find.TickManager.TicksGame + Rand.RangeInclusive(12, 16) * GenDate.TicksPerHour;
            }
            if (closingInCheckTick > 0 && Find.TickManager.TicksGame >= closingInCheckTick && !defeated)
            {
                closingInCheckTick = -1;
                CheckClosingInMidBattle();
            }
            if (!defeated)
            {
                var engine = WorldComponent_GravshipCombat.GetActiveGravEngine;
                if (engine != null && engine.Destroyed is false)
                {
                    playerDestroyedTick = -1;
                    var engineTile = engine.Tile;
                    if (engineTile.Valid)
                    {
                        var dist = DistanceUtil.GetDistanceInOrbitTiles(Tile, engineTile);
                        if (dist > threatDef.escapeDistance && !Map.mapPawns.AnyFreeColonistSpawned)
                        {
                            threatDef.Worker.OnEscape(this);
                            Destroy();
                            return;
                        }
                    }
                }
                else if (playerDestroyedTick < 0)
                {
                    playerDestroyedTick = Find.TickManager.TicksGame + threatDef.hoursBombardmentOnEngineDestroyed * GenDate.TicksPerHour;
                }

                if (threatDef.Worker.ShouldDefeat(Map))
                    Defeat();
            }

            if (playerDestroyedTick > 0)
            {
                if (Find.TickManager.TicksGame >= playerDestroyedTick)
                {
                    playerDestroyedTick = -1;
                    var engine = WorldComponent_GravshipCombat.GetActiveGravEngine;
                    if (engine is null || engine.Destroyed)
                    {
                        threatDef.Worker.OnEngineDestroyed(this);
                    }
                    if (!Map.mapPawns.AnyFreeColonistSpawned)
                    {
                        Destroy();
                        return;
                    }
                }
            }

            if (despawnTick > 0)
            {
                if (Find.TickManager.TicksGame >= despawnTick)
                {
                    Destroy();
                    return;
                }
            }
        }

        private void CheckClosingInMidBattle()
        {
            var map = Map;

            var engineExists = map.listerThings.ThingsOfDef(InternalDefOf.VGE_EnemyGravjumperEngine).Any(x => !x.Destroyed);
            var cockpitExists = map.listerThings.ThingsOfDef(InternalDefOf.VGE_EnemyPilotCockpit).Any(x => !x.Destroyed);
            var thrusterExists = map.listerThings.AllThings.Any(x => x is Building b && !b.Destroyed && b.Faction == Faction &&
                (b.def == InternalDefOf.VGE_EnemySmallThruster || b.def == InternalDefOf.VGE_EnemyLargeThruster || b.def == InternalDefOf.VGE_EnemyGiantThruster ||
                b.TryGetComp<CompGravshipThruster>() != null || b.TryGetComp<CompElectricThruster>() != null));
            var fuelExists = map.listerThings.AllThings.Any(x => x is Building b && !b.Destroyed && b.Faction == Faction &&
                ((b.TryGetComp<CompRefuelable>() is CompRefuelable r && r.Fuel > 0) ||
                (b.TryGetComp<PipeSystem.CompResourceStorage>() is PipeSystem.CompResourceStorage s && s.AmountStored > 0) ||
                (b.TryGetComp<CompPower_InputOnlyBattery>() is CompPower_InputOnlyBattery bat && bat.StoredEnergy > 0)));
            var noColonists = !map.mapPawns.AnyFreeColonistSpawned;

            if (engineExists && cockpitExists && thrusterExists && fuelExists && noColonists)
            {
                var combatComp = WorldComponent_GravshipCombat.Instance;
                var redName = combatComp.enemyGravshipName.Colorize(ColorLibrary.RedReadable);
                Messages.Message("VGE_GravjumperLaunched".Translate(redName), MessageTypeDefOf.NeutralEvent);

                combatComp.gravjumperLandingTick = Find.TickManager.TicksGame + Rand.RangeInclusive(1, 2) * GenDate.TicksPerHour;
                combatComp.gravjumperLandingName = combatComp.enemyGravshipName;

                Destroy();
            }
        }

        public void Defeat()
        {
            defeated = true;
            WorldComponent_GravshipCombat.Instance.RemoveVisibility(WorldComponent_GravshipCombat.Instance.visibility);

            var despawnTicks = threatDef.despawnHours * GenDate.TicksPerHour;
            Find.LetterStack.ReceiveLetter(threatDef.defeatLetter, threatDef.defeatLetterDesc.Formatted(despawnTicks.ToStringTicksToPeriod()), LetterDefOf.PositiveEvent);
            despawnTick = Find.TickManager.TicksGame + despawnTicks;
            threatDef.Worker.OnDefeat(Map);
        }

        public override string GetInspectString()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(base.GetInspectString());
            if (defeated && despawnTick > 0)
            {
                sb.AppendLine("VGE_DestabilizesIn".Translate((despawnTick - Find.TickManager.TicksGame).ToStringTicksToPeriod()));
            }
            return sb.ToString().TrimEndNewlines();
        }

        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            if (!defeated)
            {
                if (playerDestroyedTick > 0 && Find.TickManager.TicksGame >= playerDestroyedTick && !Map.mapPawns.AnyFreeColonistSpawned)
                {
                    alsoRemoveWorldObject = true;
                    return true;
                }
                alsoRemoveWorldObject = false;
                return false;
            }
            if (defeated)
            {
                if (despawnTick > 0 && Find.TickManager.TicksGame >= despawnTick && !Map.mapPawns.AnyFreeColonistSpawned)
                {
                    alsoRemoveWorldObject = true;
                    return true;
                }
                alsoRemoveWorldObject = false;
                return false;
            }

            return base.ShouldRemoveMapNow(out alsoRemoveWorldObject);
        }
    }
}
