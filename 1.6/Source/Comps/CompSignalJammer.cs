using RimWorld;
using UnityEngine;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class CompProperties_SignalJammer : CompProperties_GravshipFacility
    {
        public CompProperties_SignalJammer()
        {
            compClass = typeof(CompSignalJammer);
        }
    }

    public class CompSignalJammer : CompGravshipFacility
    {
        private int cooldownTicksLeft;

        public const int CooldownDuration = GenDate.TicksPerDay * 10;

        private Graphic cooldownGraphic;

        public bool OnCooldown => cooldownTicksLeft > 0;

        public override bool CanBeActive => base.CanBeActive && !OnCooldown;

        private Graphic CooldownGraphic
        {
            get
            {
                if (cooldownGraphic == null)
                {
                    cooldownGraphic = GraphicDatabase.Get<Graphic_Multi>("Things/Structures/Misc/SignalJammer/SignalJammer_Cooldown", parent.def.graphicData.shaderType.Shader, parent.def.graphicData.drawSize, parent.def.graphicData.color, parent.def.graphicData.colorTwo);
                }
                return cooldownGraphic;
            }
        }

        public override bool DontDrawParent()
        {
            return OnCooldown;
        }

        public override void PostDraw()
        {
            if (OnCooldown && parent.def.drawerType != DrawerType.MapMeshOnly)
            {
                CooldownGraphic.Draw(parent.DrawPos, parent.Rotation, parent);
            }
        }

        public override void PostPrintOnto(SectionLayer layer)
        {
            if (OnCooldown)
            {
                CooldownGraphic.Print(layer, parent, 0f);
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (cooldownTicksLeft > 0)
            {
                cooldownTicksLeft--;
                if (cooldownTicksLeft == 0)
                {
                    parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Things);
                }
            }
        }

        public void StartCooldown()
        {
            cooldownTicksLeft = CooldownDuration;
            parent.Map.mapDrawer.MapMeshDirty(parent.Position, MapMeshFlagDefOf.Things);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref cooldownTicksLeft, "cooldownTicksLeft", 0);
        }

        public override string CompInspectStringExtra()
        {
            var str = base.CompInspectStringExtra();
            if (OnCooldown)
            {
                str += "\n" + "VGE_SignalJammerCooldown".Translate(cooldownTicksLeft.ToStringTicksToPeriod());
            }
            return str;
        }
    }
}
