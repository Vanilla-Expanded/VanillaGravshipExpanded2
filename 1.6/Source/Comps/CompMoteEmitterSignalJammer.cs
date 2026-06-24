using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace VanillaGravshipExpanded2
{
    public class CompMoteEmitterSignalJammer : CompMoteEmitter
    {
        private Sustainer sustainer;
        private new CompProperties_MoteEmitterProximityScan Props => (CompProperties_MoteEmitterProximityScan)props;
        public override void PostDeSpawn(Map map, DestroyMode mode = DestroyMode.Vanish)
        {
            if (sustainer != null && !sustainer.Ended)
            {
                sustainer.End();
            }
        }

        public override void CompTick()
        {
            if (!parent.Spawned)
            {
                return;
            }
            if (mote == null)
            {
                Emit();
            }
            if (!Props.soundEmitting.NullOrUndefined())
            {
                if (sustainer == null || sustainer.Ended)
                {
                    sustainer = Props.soundEmitting.TrySpawnSustainer(SoundInfo.InMap(parent));
                }
                sustainer.Maintain();
            }
            if (mote == null)
            {
                return;
            }
            mote.Maintain();
            float a;
            if (ticksSinceLastEmitted >= Props.emissionInterval)
            {
                ticksSinceLastEmitted = 0;
            }
            else
            {
                ticksSinceLastEmitted++;
            }
            var num = (float)ticksSinceLastEmitted / 60f;
            a = ((num <= Props.warmupPulseFadeInTime) ? ((!(Props.warmupPulseFadeInTime > 0f)) ? 1f : (num / Props.warmupPulseFadeInTime)) : ((num <= Props.warmupPulseFadeInTime + Props.warmupPulseSolidTime) ? 1f : ((!(Props.warmupPulseFadeOutTime > 0f)) ? 1f : (1f - Mathf.InverseLerp(Props.warmupPulseFadeInTime + Props.warmupPulseSolidTime, Props.warmupPulseFadeInTime + Props.warmupPulseSolidTime + Props.warmupPulseFadeOutTime, num)))));
            mote.instanceColor.a = a;
        }
    }
}
