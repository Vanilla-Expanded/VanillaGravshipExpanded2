using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2;

[StaticConstructorOnStartup]
public class Gizmo_ProjectileInterceptorHitPointsWithHeat : Gizmo_ProjectileInterceptorHitPoints
{
    private static readonly Texture2D OverchargedBarTex = SolidColorMaterials.NewSolidColorTexture(Color.red);

    public Gizmo_ProjectileInterceptorHitPointsWithHeat()
    {
        Order = -100f;
    }

    public override float GetWidth(float maxWidth)
    {
        return Width;
    }

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        if (interceptor.ChargingTicksLeft <= interceptor.Props.chargeDurationTicks)
            return base.GizmoOnGUI(topLeft, maxWidth, parms);

        // Overcharged
        var rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
        var rect2 = rect.ContractedBy(6f);
        Widgets.DrawWindowBackground(rect);

        // Draw overcharged text
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.UpperLeft;
        var overchargedRect = new Rect(rect2.x, rect2.y - 2f, rect2.width, rect2.height / 2f);
        Widgets.Label(overchargedRect, "VGE_ShieldOverheatedGizmo".Translate());

        // Draw overcharged bar
        var barRect = new Rect(rect2.x, overchargedRect.yMax, rect2.width, rect2.height / 2f);
        var lerp = Mathf.InverseLerp(interceptor.Props.chargeDurationTicks, interceptor.Props.chargeDurationTicks * 2, interceptor.ChargingTicksLeft);
        // Log.ErrorOnce($"Lerp: {lerp}, Min: {interceptor.Props.chargeDurationTicks}, max: {interceptor.Props.chargeDurationTicks * 2}, val: {interceptor.ChargingTicksLeft}", interceptor.ChargingTicksLeft.GetHashCode());
        GUI.DrawTexture(barRect, FullBarTex);
        GUI.DrawTexture(barRect, OverchargedBarTex, ScaleMode.StretchToFill, true, 1f, new Color(1, 1, 1, lerp), 0f, 0f);
        Text.Font = GameFont.Small;
        Text.Anchor = TextAnchor.MiddleCenter;
        // Draw recharging ticks left
        Widgets.Label(barRect, (interceptor.ChargingTicksLeft - interceptor.Props.chargeDurationTicks).ToStringTicksToPeriod());
        Text.Anchor = TextAnchor.UpperLeft;

        // Display tooltip
        if (!interceptor.Props.gizmoTipKey.NullOrEmpty())
            TooltipHandler.TipRegion(rect2, interceptor.Props.gizmoTipKey.Translate());

        return new GizmoResult(GizmoState.Clear);
    }
}