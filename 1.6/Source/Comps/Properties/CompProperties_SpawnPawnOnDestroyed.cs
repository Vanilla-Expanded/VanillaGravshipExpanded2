
using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI.Group;
namespace VanillaGravshipExpanded2
{
    public class CompProperties_SpawnPawnOnDestroyed : CompProperties
    {
        public PawnKindDef pawnKind;
        public ThingDef filthCreated;
        public IntRange filthCountRange;
        public SoundDef sound;

        public CompProperties_SpawnPawnOnDestroyed()
        {
            compClass = typeof(CompSpawnPawnOnDestroyed);
        }


    }
}