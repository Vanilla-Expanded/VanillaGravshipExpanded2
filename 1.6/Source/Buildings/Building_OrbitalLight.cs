using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Analytics;
using Verse;
using Verse.Noise;

namespace VanillaGravshipExpanded2
{
    public class Building_OrbitalLight : Building
    {
        public CompGlower cachedCompGlower;

        ColorInt color = new ColorInt(255, 40, 40, 0);

        public CompGlower CompGlower
        {
            get
            {
                if(cachedCompGlower == null)
                {
                    cachedCompGlower=this.TryGetComp<CompGlower>();
                }
                return cachedCompGlower;
            }

        }
      
        public override void TickRare()
        {
            base.TickRare();
            if(Map != null)
            {
                if (Position.GetVacuum(Map) > 0.2f)
                {
                    CompGlower.GlowColor = color;
                }
                else
                {
                    CompGlower.GlowColor = CompGlower.Props.glowColor;
                }
            }
            

          
        }

       

    }
}
