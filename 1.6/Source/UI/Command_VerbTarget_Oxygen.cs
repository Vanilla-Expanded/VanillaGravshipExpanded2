using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2;

public class Command_VerbTarget_Oxygen(CompApparelVerbOwner comp) : Command_VerbOwner(comp)
{
    public override bool Disabled
    {
        get
        {
            DisabledCheck();
            return disabled;
        }
        set => disabled = value;
    }

    public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        var result = base.GizmoOnGUI(topLeft, maxWidth, parms);

        if (verb is IOxygenVerb { VerbOwner_OxygenCompSource: { remainingCooldownTicks: > 0 } comp })
        {
            var rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            var num = Mathf.InverseLerp(comp.Props.cooldownTicks, 0f, comp.remainingCooldownTicks);

            // Draw cooldown bar
            Widgets.FillableBar(rect, Mathf.Clamp01(num), Command_Ability.cooldownBarTex, null, false);

            Text.Font = GameFont.Tiny;
            var text = comp.remainingCooldownTicks.ToStringTicksToPeriod();
            var textSize = Text.CalcSize(text);
            textSize.x += 2f;
            var rect2 = rect;
            rect2.x = rect.x + rect.width / 2f - textSize.x / 2f;
            rect2.width = textSize.x;
            rect2.height = textSize.y;
            var rect3 = rect2.ExpandedBy(8f, 0f);
            Text.Anchor = TextAnchor.UpperCenter;
            GUI.DrawTexture(rect3, TexUI.GrayTextBG);
            Widgets.Label(rect2, text);
            Text.Anchor = TextAnchor.UpperLeft;
        }

        return result;
    }

    public override GizmoResult GizmoOnGUIInt(Rect butRect, GizmoRenderParms parms)
    {
        DisabledCheck();
        return base.GizmoOnGUIInt(butRect, parms);
    }

    protected virtual void DisabledCheck()
    {
        disabled = !comp.CanBeUsed(out var text);
        if (disabled)
            DisableWithReason(text.CapitalizeFirst());
    }

    protected void DisableWithReason(string reason)
    {
        disabledReason = reason;
        disabled = true;
    }
}