using System.Linq;
using RimWorld;
using RimWorld.Planet;
using VanillaGravshipExpanded;
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
            if (closingInCheckTick > 0 && Find.TickManager.TicksGame >= closingInCheckTick && !defeated && CheckClosingInMidBattle())
            {
                return;
            }
            if (!defeated)
            {
                var playerMap = WorldComponent_GravshipCombat.Instance.GetPlayerTargetMap();
                if (playerMap != null && playerMap.mapPawns.AnyFreeColonistSpawned)
                {
                    playerDestroyedTick = -1;
                    var dist = DistanceUtil.GetDistanceInOrbitTiles(Tile, playerMap.Tile);
                    if (dist > threatDef.escapeDistance && !Map.mapPawns.AnyFreeColonistSpawned)
                    {
                        threatDef.Worker.OnEscape(this);
                        Destroy();
                        return;
                    }
                }
                else if (playerDestroyedTick < 0 && (playerMap == null || !playerMap.mapPawns.AllPawns.Any(p => p.Faction == Faction.OfPlayer)))
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
                    threatDef.Worker.OnEngineDestroyed(this);
                    despawnTick = Find.TickManager.TicksGame;
                }
            }

            if (despawnTick > 0)
            {
                if (Find.TickManager.TicksGame >= despawnTick && GravshipUtility.GetPlayerGravEngine_NewTemp(Map) == null)
                {
                    Destroy();
                    return;
                }
            }
        }

        private bool CheckClosingInMidBattle()
        {
            var map = Map;
            if (map.mapPawns.AnyFreeColonistSpawned) return false;
            closingInCheckTick = -1;
            var enemyThings = map.listerThings.AllThings.Where(x => x.Faction == Faction && x.Destroyed is false).ToList();
            var engine = enemyThings.OfType<Building_EnemyGravEngine>().FirstOrDefault();
            var cockpitExists = enemyThings.Where(x => x.def == InternalDefOf.VGE_EnemyPilotCockpit).Any(x => !x.Destroyed);
            var thrusterExists = enemyThings.Any(x => x.def.HasModExtension<EnemyThrusterExtension>());
            var fuelExists = enemyThings.Any(x => x.TryGetComp<PipeSystem.CompResourceStorage>() is PipeSystem.CompResourceStorage s && s.AmountStored > 0);
            if (engine != null && cockpitExists && thrusterExists && fuelExists)
            {
                var combatComp = WorldComponent_GravshipCombat.Instance;
                var redName = combatComp.enemyGravshipName.Colorize(ColorLibrary.RedReadable);
                Messages.Message("VGE_GravjumperLaunched".Translate(redName), MessageTypeDefOf.NeutralEvent);
                combatComp.gravjumperLandingTick = Find.TickManager.TicksGame + Rand.RangeInclusive(1, 2) * GenDate.TicksPerHour;
                combatComp.gravjumperLandingName = combatComp.enemyGravshipName;
                combatComp.enemyGravjumper = EnemyStructure.CaptureFrom(map, engine);
                Destroy();
                return true;
            }
            return false;
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
            if (defeated && despawnTick > 0 && GravshipUtility.GetPlayerGravEngine_NewTemp(Map) == null)
            {
                sb.AppendLine("VGE_DestabilizesIn".Translate((despawnTick - Find.TickManager.TicksGame).ToStringTicksToPeriod()));
            }
            return sb.ToString().TrimEndNewlines();
        }

        public override bool ShouldRemoveMapNow(out bool alsoRemoveWorldObject)
        {
            alsoRemoveWorldObject = false;
            if (despawnTick > 0 && Find.TickManager.TicksGame >= despawnTick)
            {
                if (!HasMap || GravshipUtility.GetPlayerGravEngine_NewTemp(Map) == null)
                {
                    alsoRemoveWorldObject = true;
                    return true;
                }
            }

            if (!defeated)
            {
                if (playerDestroyedTick > 0 && Find.TickManager.TicksGame >= playerDestroyedTick && (!HasMap || !Map.mapPawns.AnyFreeColonistSpawned))
                {
                    alsoRemoveWorldObject = true;
                    return true;
                }
            }

            return false;
        }
    }
}