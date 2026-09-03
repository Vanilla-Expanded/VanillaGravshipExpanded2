using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2
{
    [HotSwappable]
    [HarmonyPatch(typeof(ColonistBar), nameof(ColonistBar.ColonistBarOnGUI))]
    public static class ColonistBar_ColonistBarOnGUI_Patch
    {
        public static void Postfix()
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }
            var colonistBar = Find.ColonistBar;
            var entries = colonistBar.Entries;
            var drawLocs = colonistBar.DrawLocs;
            var size = colonistBar.Size;
            for (int i = 0; i < drawLocs.Count; i++)
            {
                var entry = entries[i];
                if (entry.pawn != null || entry.map == null || entry.map.info.parent is not MapParent_WarPlatform warPlatform)
                {
                    continue;
                }
                Rect rect = new Rect(drawLocs[i].x, drawLocs[i].y, size.x, size.y);
                float alpha = colonistBar.GetEntryRectAlpha(rect);
                if (entry.map != Find.CurrentMap || WorldRendererUtility.WorldSelected)
                {
                    alpha = Mathf.Min(alpha, ColonistBar.EntryInAnotherMapAlpha);
                }
                GUI.color = new Color(1f, 1f, 1f, alpha);
                GUI.DrawTexture(rect, ColonistBar.BGTex);
                Color iconColor = warPlatform.ExpandingIconColor;
                iconColor.a *= alpha;
                GUI.color = iconColor;
                GUI.DrawTexture(rect.ContractedBy(4f * colonistBar.Scale), warPlatform.ExpandingIcon, ScaleMode.ScaleToFit);
                GUI.color = Color.white;
                TooltipHandler.TipRegion(rect, warPlatform.LabelCap + "\n\n" + warPlatform.GetDescription());
            }
        }
    }
}
