using Verse;

namespace VanillaGravshipExpanded2;

public class Building_EmergencyGravshipGenerator : Building
{
    protected CompPowerEmergencyGravshipGenerator generator;
    protected Graphic offGraphic;

    public override Graphic Graphic
    {
        get
        {
            if (generator == null || generator.IsActive)
                return base.Graphic;

            if (offGraphic == null)
            {
                var style = StyleDef;
                if (style?.Graphic != null)
                {
                    if (style.graphicData != null)
                        offGraphic = GraphicDatabase.Get(style.graphicData.graphicClass, $"{style.graphicData.texPath}_Off", style.graphicData.shaderType.Shader, style.graphicData.drawSize, DrawColor, DrawColorTwo);
                    // Can this happen? Vanilla here just grabs the graphic and calls it a day, but we can't since we rely on the off graphic.
                    else
                        offGraphic = GraphicDatabase.Get(style.Graphic.GetType(), $"{style.Graphic.path}_Off", style.Graphic.Shader, style.Graphic.drawSize, style.Graphic.Color, style.Graphic.ColorTwo);
                }
                else
                {
                    offGraphic = GraphicDatabase.Get(def.graphicData.graphicClass, $"{def.graphicData.texPath}_Off", def.graphicData.shaderType.Shader, def.graphicData.drawSize, DrawColor, DrawColorTwo);
                }
            }

            return offGraphic;
        }
    }

    public override ThingStyleDef StyleDef
    {
        get => base.StyleDef;
        set
        {
            offGraphic = null;
            base.StyleDef = value;
        }
    }

    public override void PostMake()
    {
        base.PostMake();

        InitComps();
    }

    public override void ExposeData()
    {
        base.ExposeData();

        if (Scribe.mode == LoadSaveMode.LoadingVars)
            InitComps();
    }

    private void InitComps() => generator = GetComp<CompPowerEmergencyGravshipGenerator>();
}