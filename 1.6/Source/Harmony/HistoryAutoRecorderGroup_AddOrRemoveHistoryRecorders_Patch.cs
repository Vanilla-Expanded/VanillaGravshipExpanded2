using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
	[HarmonyPatch(typeof(HistoryAutoRecorderGroup), nameof(HistoryAutoRecorderGroup.ExposeData))]
	public static class HistoryAutoRecorderGroup_ExposeData_Patch
	{
		public static void Postfix(HistoryAutoRecorderGroup __instance)
		{
			foreach (var recorder in __instance.recorders)
			{
				if (recorder.def == InternalDefOf.VGE_GravshipVisibilityRecorder)
				{
					var expectedCount = Find.TickManager.TicksGame / recorder.def.recordTicksFrequency;
					if (recorder.records.Count < expectedCount)
					{
						while (recorder.records.Count < expectedCount)
						{
							recorder.records.Insert(0, 0f);
						}
					}
				}
			}
		}
	}
}
