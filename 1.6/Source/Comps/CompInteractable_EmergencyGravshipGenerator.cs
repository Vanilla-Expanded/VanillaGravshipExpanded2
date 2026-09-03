using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace VanillaGravshipExpanded2;

public class CompInteractable_EmergencyGravshipGenerator : CompInteractable
{
    protected static bool isCompInspectStringExtraCall = false;

    protected CompPowerEmergencyGravshipGenerator generator;

    public override bool Active => generator.IsActive;

    public override string ExposeKey => "EmergencyGravshipGenerator";

    // False, so the generator doesn't cooldown on its own. We handle it through world comp, as cooldown is global.
    // We need to set it to true during CompInspectStringExtra calls, as otherwise it'll say "(paused)" in the inspect string.
    public override bool CanCooldown => isCompInspectStringExtraCall;

    public override void CompTick()
    {
        base.CompTick();

        var wasOnCooldown = OnCooldown;
        cooldownTicks = WorldComponent_GravshipCombat.Instance.emergencyGravshipGeneratorCooldownTicks;
        if (wasOnCooldown && !OnCooldown)
            CooldownEnded();
    }

    public override void PostPostMake()
    {
        base.PostPostMake();
        
        InitComps();
    }

    public override void PostExposeData()
    {
        base.PostExposeData();

        if (Scribe.mode == LoadSaveMode.LoadingVars)
            InitComps();
    }

    public override void OnInteracted(Pawn caster)
    {
        if (!generator.IsActive)
            generator.Activate(caster);
    }

    public override AcceptanceReport CanInteract(Pawn activateBy = null, bool checkOptionalItems = true)
    {
        if (WorldComponent_GravshipCombat.Instance.activeEmergencyGravshipGenerator != null && WorldComponent_GravshipCombat.Instance.activeEmergencyGravshipGenerator != parent)
            return "VGE_EmergencyGenerator_AnotherActive".Translate();

        var result = base.CanInteract(activateBy, checkOptionalItems);
        if (!result.Accepted)
            return result;

        if (generator.maintainable.maintenance <= 0.99f)
            return "VGE_EmergencyGenerator_MustBeFullyMaintained".Translate();
        if (generator.breakdownableComp is { BrokenDown: true })
            return "BrokenDown".Translate().CapitalizeFirst();

        return true;
    }

    private void InitComps()
    {
        generator = parent.GetComp<CompPowerEmergencyGravshipGenerator>();
    }

    public override string CompInspectStringExtra()
    {
        var prevCooldownTicks = cooldownTicks;
        var anyGeneratorActive = WorldComponent_GravshipCombat.Instance.activeEmergencyGravshipGenerator != null;

        try
        {
            // Temporarily remove the cooldown text, since base class doesn't ever remove it otherwise.
            if (anyGeneratorActive)
                cooldownTicks = 0;

            isCompInspectStringExtraCall = true;
            return base.CompInspectStringExtra();
        }
        finally
        {
            cooldownTicks = prevCooldownTicks;
            isCompInspectStringExtraCall = false;
        }
    }
}