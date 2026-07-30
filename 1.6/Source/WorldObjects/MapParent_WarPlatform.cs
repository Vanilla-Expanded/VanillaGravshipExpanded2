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
            Scribe_Values.Look(ref customLabel, "customLabel");
            Scribe_Values.Look(ref customDescription, "customDescription");
        }

        public override void Tick()
        {
            base.Tick();
            if (!HasMap) return;
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

        public void Defeat()
        {
            defeated = true;
            WorldComponent_GravshipCombat.Instance.RemoveVisibility(WorldComponent_GravshipCombat.Instance.visibility);

            var despawnTicks = (int)(threatDef.daysToDespawnAfterDefeat * GenDate.TicksPerDay);
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
