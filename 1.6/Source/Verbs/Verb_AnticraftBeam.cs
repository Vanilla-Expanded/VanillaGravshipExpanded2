using UnityEngine;
using VanillaGravshipExpanded;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class Verb_AnticraftBeam : Verb_ShootWithWorldTargeting
    {
        protected override IntVec3 GetProjectileDest(IntVec3 intVec3)
        {
            var vec = base.GetProjectileDest(intVec3).ToVector3Shifted() + Gen.RandomHorizontalVector(6.9f);
            return vec.ToIntVec3();
        }
    }
}
