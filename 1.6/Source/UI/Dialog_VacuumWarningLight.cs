using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2;

public class Dialog_VacuumWarningLight : Window
{
    public float curValue;
    public float from;
    public float to;
    public float roundTo;
    public bool evacuatePawns;
    public readonly bool evacuatePawnsConfigurable;

    private const float BotAreaHeight = 30f;
    private const float NumberYOffset = 10f;

    public override Vector2 InitialSize => new(300f, 130f + (evacuatePawnsConfigurable ? Text.LineHeight * 2f + 4f : 0f));

    public override float Margin => 10f;

    public Dialog_VacuumWarningLight(FloatRange range, bool evacuatePawns, bool evacuatePawnsConfigurable, float startingValue = float.NaN, float roundTo = 0.01f) : this(range.min, range.max, evacuatePawns, evacuatePawnsConfigurable, startingValue, roundTo)
    {
    }

    public Dialog_VacuumWarningLight(float from, float to, bool evacuatePawns, bool evacuatePawnsConfigurable, float startingValue = float.NaN, float roundTo = 0.01f)
    {
        if (from > to)
            (from, to) = (to, from);

        this.from = from;
        this.to = to;
        this.roundTo = roundTo;
        this.evacuatePawns = evacuatePawns;
        this.evacuatePawnsConfigurable = evacuatePawnsConfigurable;

        forcePause = true;
        closeOnClickedOutside = true;

        // Not NaN, not infinity
        if (float.IsFinite(startingValue))
            curValue = startingValue;
        else
            curValue = from;
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Small;
        var text = "VGE_VacuumEmergencyLight_UnsafeVacuumSlider".Translate(curValue);
        var num = Text.CalcHeight(text, inRect.width);
        var rect = new Rect(inRect.x, inRect.y, inRect.width, num);
        Text.Anchor = TextAnchor.UpperCenter;
        // Draw the 
        Widgets.Label(rect, text);
        Text.Anchor = TextAnchor.UpperLeft;
        // Draw the slider
        var sliderRect = new Rect(inRect.x, inRect.y + rect.height + 10f, inRect.width, 30f);
        curValue = Widgets.HorizontalSlider(sliderRect, curValue, from, to, true, null, null, null, roundTo);
        GUI.color = ColoredText.SubtleGrayColor;
        Text.Font = GameFont.Tiny;
        // Draw "From"
        Widgets.Label(new Rect(inRect.x, sliderRect.yMax - NumberYOffset, inRect.width / 2f, Text.LineHeight), from.ToStringPercent());
        Text.Anchor = TextAnchor.UpperRight;
        // Draw "to"
        Widgets.Label(new Rect(inRect.x + inRect.width / 2f, sliderRect.yMax - NumberYOffset, inRect.width / 2f, Text.LineHeight), to.ToStringPercent());
        Text.Anchor = TextAnchor.UpperLeft;
        Text.Font = GameFont.Small;
        GUI.color = Color.white;

        // Checkbox to turn on/off the auto evacuation
        if (evacuatePawnsConfigurable)
            Widgets.CheckboxLabeled(new Rect(sliderRect.xMin, sliderRect.yMax + 2f, sliderRect.width, Text.LineHeight * 2f + 2f), "VGE_VacuumEmergencyLight_EvacuatePawns".Translate(), ref evacuatePawns);

        // Draw cancel/confirm buttons
        var buttonWidth = (inRect.width - 10f) / 2f;
        // Cancel, just close
        if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - BotAreaHeight, buttonWidth, BotAreaHeight), "CancelButton".Translate()))
            Close();
        // Accept, change all selected lights
        if (Widgets.ButtonText(new Rect(inRect.x + buttonWidth + 10f, inRect.yMax - BotAreaHeight, buttonWidth, BotAreaHeight), "OK".Translate()))
        {
            Close();
            foreach (var obj in Find.Selector.SelectedObjects)
            {
                if (obj is ThingWithComps thing && thing.GetComp<CompVacuumWarningLight>() is { Props.visualOnly: false } comp)
                {
                    comp.ConcerningVacuumLevel = curValue;
                    if (evacuatePawnsConfigurable)
                        comp.EvacuatePawns = evacuatePawns;
                }
            }
        }
    }
}