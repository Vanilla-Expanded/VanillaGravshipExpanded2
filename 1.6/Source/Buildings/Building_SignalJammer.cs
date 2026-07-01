using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class Building_SignalJammer : Building
    {
        private int cooldownTicksLeft;
        public const int CooldownDuration = 60000 * 10;
        private Graphic cooldownGraphic;

        public bool OnCooldown => cooldownTicksLeft > 0;

        public override Graphic Graphic
        {
            get
            {
                if (OnCooldown)
                {
                    if (cooldownGraphic == null)
                        cooldownGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Structures/Misc/SignalJammer/SignalJammer_Cooldown", def.graphicData.shaderType.Shader, def.graphicData.drawSize, def.graphicData.color, def.graphicData.colorTwo);
                    return cooldownGraphic;
                }
                return base.Graphic;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref cooldownTicksLeft, "cooldownTicksLeft", 0);
        }

        public override void Tick()
        {
            base.Tick();
            if (cooldownTicksLeft > 0)
            {
                cooldownTicksLeft--;
                if (cooldownTicksLeft == 0)
                    Map.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Things);
            }
        }

        public void StartCooldown()
        {
            cooldownTicksLeft = CooldownDuration;
            Map.mapDrawer.MapMeshDirty(Position, MapMeshFlagDefOf.Things);
        }

        public override string GetInspectString()
        {
            var str = base.GetInspectString();
            if (OnCooldown)
                str += "\n" + "VGE_SignalJammerCooldown".Translate(cooldownTicksLeft.ToStringTicksToPeriod());
            return str;
        }
    }
}