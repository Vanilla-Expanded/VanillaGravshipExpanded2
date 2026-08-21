using Verse;

namespace VanillaGravshipExpanded2
{
    public class CompProperties_GravEngineDestruction : CompProperties
    {
        public float explosionRadius;
        public float shockwaveRadius;
        public float substructureDamageRadius;
        public IntRange gravJunkCountRange;
        public FleckDef implosionFleck;
        public FleckDef shockwaveFleck;

        public CompProperties_GravEngineDestruction()
        {
            compClass = typeof(CompGravEngineDestruction);
        }
    }
}
