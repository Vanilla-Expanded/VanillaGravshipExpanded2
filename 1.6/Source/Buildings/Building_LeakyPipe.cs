using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Analytics;
using Verse;
using Verse.Noise;

namespace VanillaGravshipExpanded2
{
    public class Building_LeakyPipe: Building
    {

        public bool leaking = false;
        public int leaks = 10;

        public override void Tick()
        {
            base.Tick();

            if (this.IsHashIntervalTick(30))
            {
                if (HitPoints < MaxHitPoints)
                {
                    leaking = true;
                }
            }

            if (leaking && leaks > 0 && this.IsHashIntervalTick(60) && Map!=null)
            {
                IntVec3 leakCell = CellFinder.RandomClosewalkCellNear(Position, Map, 4);
                FilthMaker.TryMakeFilth(leakCell, Map, InternalDefOf.VGE_Filth_Astrofuel, 1);
                leaks--;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref leaking, "leaking", false);
            Scribe_Values.Look(ref leaks, "leaks", 10);
        }

    }
}
