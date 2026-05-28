using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
namespace VanillaGravshipExpanded2
{
	[DefOf]
	public static class InternalDefOf
	{
		static InternalDefOf()
		{
			DefOfHelper.EnsureInitializedInCtor(typeof(InternalDefOf));
		}

		[MayRequireBiotech]
		public static GeneDef PerfectImmunity;

		public static HediffDef VGE_ExowormInfestation;

		public static ThingDef VGE_GiantWormspitter;
        public static ThingDef VGE_ExoHive_Building;
        public static ThingDef VGE_TwistedSteel;
        public static ThingDef VGE_TwistedMachinery;
        public static ThingDef VGE_TwistedElectronics;
        public static ThingDef VGE_TwistedGravlite;
        public static ThingDef VGE_EggSac;
        public static ThingDef VGE_ExowormCocoon;

        public static PawnKindDef VGE_ExowormKnot;

        public static SoundDef VEG_InsectoidTurretTargetAcquired;


    }
}
