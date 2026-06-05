using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;
using Verse.Sound;

namespace VanillaGravshipExpanded2
{
    public class JobDriver_FormKnot : JobDriver
    {
        private const int MaxFormingTicks = 2500;
        private const int TargetFormingTicks = 360;

        private int ticksForming;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref ticksForming, "ticksForming", 0);
        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return true;
        }

        public override Vector3 ForcedBodyOffset
        {
            get
            {
                if (pawn.pather.Moving) return Vector3.zero;
                var tick = Find.TickManager.TicksGame + pawn.thingIDNumber * 17;
                var x = Mathf.Sin(tick * 0.15f) * 0.15f;
                var z = Mathf.Cos(tick * 0.2f) * 0.15f;
                return new Vector3(x, 0f, z);
            }
        }

        public override IEnumerable<Toil> MakeNewToils()
        {
            var anchor = TargetPawnA;
            this.FailOn(delegate
            {
                var a = TargetPawnA;
                if (a == null || a.Dead || a.Destroyed || !a.Spawned)
                {
                    return true;
                }
                var comp = a.TryGetComp<CompExowormJoiner>();
                foreach (var candidate in comp.candidates)
                {
                    if (candidate == null || candidate.Dead || candidate.Destroyed)
                    {
                        candidate?.jobs?.StopAll();
                        return true;
                    }
                }
                return false;
            });
            if (pawn != anchor)
            {
                yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.OnCell);
            }

            var formToil = Toils_General.Wait(MaxFormingTicks);
            formToil.WithProgressBarToilDelay(TargetIndex.A);
            formToil.tickAction = delegate
            {
                ticksForming++;
                if (pawn != anchor && pawn.IsHashIntervalTick(30))
                {
                    pawn.Rotation = Rot4.Random;
                }

                if (ticksForming >= TargetFormingTicks)
                {
                    var comp = anchor.TryGetComp<CompExowormJoiner>();
                    var allInCell = true;
                    foreach (var candidate in comp.candidates)
                    {
                        if (candidate != null && !candidate.Dead && !candidate.Destroyed && candidate.Position != anchor.Position)
                        {
                            allInCell = false;
                            break;
                        }
                    }
                    if (allInCell)
                    {
                        ReadyForNextToil();
                    }
                }
            };
            yield return formToil;

            var transformToil = ToilMaker.MakeToil();
            transformToil.initAction = delegate
            {
                if (pawn == anchor)
                {
                    TryTransformIntoKnot();
                }
            };
            transformToil.defaultCompleteMode = ToilCompleteMode.Instant;
            yield return transformToil;
        }

        private void TryTransformIntoKnot()
        {
            var toTransform = pawn.TryGetComp<CompExowormJoiner>().candidates;
            var spawnPos = pawn.Position;
            var map = pawn.Map;

            var knot = PawnGenerator.GeneratePawn(InternalDefOf.VGE_ExowormKnot, pawn.Faction);
            GenSpawn.Spawn(knot, spawnPos, map);

            foreach (var exoworm in toTransform)
            {
                if (exoworm != pawn)
                {
                    exoworm.Destroy();
                }
            }

            foreach (var cell in GenRadial.RadialCellsAround(spawnPos, 3f, true))
            {
                if (cell.InBounds(map))
                {
                    FilthMaker.TryMakeFilth(cell, map, ThingDefOf.Filth_BloodInsect);
                    FilthMaker.TryMakeFilth(cell, map, ThingDefOf.Filth_Slime);
                }
            }
            InternalDefOf.Hive_Spawn.PlayOneShot(new TargetInfo(spawnPos, map));
            pawn.Destroy();
        }
    }
}
