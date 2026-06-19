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

		public static ThingDef VGE_Exoworm;
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

		public static TerrainDef VGE_GravshipSubarmor;
        public static TerrainDef VGE_Subcreep;

        public static DesignationCategoryDef VGE_Designer;
		public static DesignationCategoryDef Odyssey;
		[DefAlias("VGE_EmptySpace")]
		public static WorldObjectDef VGE_EmptySpaceObj;
		public static MapGeneratorDef VGE_EmptySpace;
		public static SoundDef OrbitalTargeter_Fire;
		public static StatDef VGE_GravshipTargeting;
		public static ThingDef VGE_GravshipArmor;
		public static JobDef VGE_FormKnot;
		public static SoundDef Hive_Spawn;

		public static ThingDef VGE_WarpodLeaving;
		public static ThingDef VGE_WarpodIncoming;
		public static WorldObjectDef VGE_TravellingWarpod;
		public static ThingDef VGE_GravshipBlackBox;
	}
}
