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


    }
}
