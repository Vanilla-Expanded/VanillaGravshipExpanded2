using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.AI;

namespace VanillaGravshipExpanded2
{
    public class CompExowormJoiner : ThingComp
    {
        public List<Pawn> candidates = new List<Pawn>();

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Collections.Look(ref candidates, "candidates", LookMode.Reference);
        }
        public override void CompTick()
        {
            base.CompTick();
            if (parent.IsHashIntervalTick(120) && parent.Spawned)
            {
                TryInitiateKnotForming();
            }
        }

        private void TryInitiateKnotForming()
        {
            var pawn = parent as Pawn;
            if (pawn.Dead || pawn.Downed || pawn.Destroyed || pawn.InMentalState || pawn.CurJob?.def == InternalDefOf.VGE_FormKnot)
            {
                return;
            }
            var map = pawn.Map;
            var searchRadius = 8f;
            candidates = new List<Pawn>();
            foreach (var thing in map.listerThings.ThingsOfDef(pawn.def))
            {
                if (thing is Pawn other && !other.Dead && !other.Downed && !other.Destroyed && other.Faction == pawn.Faction && other.Position.InHorDistOf(pawn.Position, searchRadius))
                {
                    if (other.CurJob?.def != InternalDefOf.VGE_FormKnot)
                    {
                        candidates.Add(other);
                    }
                }
            }

            if (candidates.Count >= 10)
            {
                var avgPos = new IntVec3(Mathf.RoundToInt((float)candidates.Average(p => p.Position.x)), 0, Mathf.RoundToInt((float)candidates.Average(p => p.Position.z)));
                var anchor = candidates.MinBy(p => p.Position.DistanceToSquared(avgPos));
                candidates = candidates.OrderBy(p => p.Position.DistanceTo(anchor.Position)).Take(10).ToList();

                anchor.TryGetComp<CompExowormJoiner>().candidates = candidates;

                foreach (var p in candidates)
                {
                    p.pather.StopDead();
                    var job = JobMaker.MakeJob(InternalDefOf.VGE_FormKnot, anchor);
                    p.jobs.StartJob(job, JobCondition.InterruptForced);
                }
            }
        }
    }
}
