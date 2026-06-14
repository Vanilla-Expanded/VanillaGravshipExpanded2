using RimWorld;

namespace VanillaGravshipExpanded2;

public class Verb_OxygenJump : Verb_Jump, IOxygenVerb
{
    public CompApparelVerbOwner_Oxygen VerbOwner_OxygenCompSource => DirectOwner as CompApparelVerbOwner_Oxygen;

    public override float EffectiveRange
    {
        get
        {
            if (cachedEffectiveRange < 0f)
            {
                var equipment = EquipmentSource;
                if (equipment != null && verbProps.rangeStat != null)
                    cachedEffectiveRange = equipment.GetStatValue(verbProps.rangeStat);
                else
                    cachedEffectiveRange = base.EffectiveRange;
            }
            return cachedEffectiveRange;
        }
    }

    public override bool TryCastShot()
    {
        var comp = VerbOwner_OxygenCompSource;
        if (comp == null || !comp.CanBeUsed(out _))
            return false;
        comp.UsedOnce();

        // Just in case, copied from original method with null comp. Just in case vanilla decides to use CompApparelVerbOwner there or something.
        return JumpUtility.DoJump(CasterPawn, currentTarget, null, verbProps);
    }
}