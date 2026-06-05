using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using VanillaGravshipExpanded;
using VanillaGravshipExpanded2.UI;
using Verse;

namespace VanillaGravshipExpanded2;

public class CompApparelVerbOwner_Oxygen : CompApparelVerbOwner_Charged
{
    public int remainingCooldownTicks = 0;
    protected CompApparelOxygenProvider compOxygen;

    public new CompProperties_ApparelVerbOwnerOxygen Props => (CompProperties_ApparelVerbOwnerOxygen)props;

    public new int RemainingCharges
    {
        get
        {
            if (compOxygen == null)
                return 0;
            return Mathf.FloorToInt(compOxygen.RemainingChargesExact / Props.chargePerUse);
        }
    }

    public new int MaxCharges
    {
        get
        {
            if (compOxygen == null)
                return 0;
            return Mathf.FloorToInt(compOxygen.MaxCharges / Props.chargePerUse);
        }
    }

    public new string LabelRemaining => $"{RemainingCharges} / {MaxCharges}";

    public override string GizmoExtraLabel => LabelRemaining;

    public override string CompInspectStringExtra() => null;

    public override string CompTipStringExtra() => null;

    public override void CompTickInterval(int delta)
    {
        base.CompTickInterval(delta);

        remainingCooldownTicks -= delta;
    }

    public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
    {
        yield break;
    }

    public override void UsedOnce()
    {
        // No base call, we don't care for whatever CompApparelVerbOwner_Charged does.
        // We need to override it, as a bunch of verb code relies on that (rather than just using CompApparelVerbOwner).

        remainingCooldownTicks = Props.cooldownTicks;
        if (compOxygen != null)
            compOxygen.RemainingChargesExact -= Props.chargePerUse;
    }

    public override bool CanBeUsed(out string reason)
    {
        // Funnily, CompApparelVerbOwner_Charged doesn't check for charge count or anything, so we can safely call base method.
        if (!base.CanBeUsed(out reason))
            return false;

        if (compOxygen == null || compOxygen.RemainingChargesExact < Props.chargePerUse)
        {
            reason = "VGE_NotEnoughOxygen".Translate();
            return false;
        }

        if (remainingCooldownTicks > 0)
        {
            reason = "AbilityOnCooldown".Translate(remainingCooldownTicks.ToStringTicksToPeriod()).Resolve();
            return false;
        }

        return true;
    }

    public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
    {
        var drafted = Wearer.Drafted;
        if ((drafted && !Props.displayGizmoWhileDrafted) || (!drafted && !Props.displayGizmoWhileUndrafted))
            yield break;

        foreach (var verb in VerbTracker.AllVerbs)
        {
            if (verb.verbProps.hasStandardCommand)
                yield return CreateVerbTargetCommand_Oxygen(parent, verb);
        }

        if (DebugSettings.ShowDevGizmos && remainingCooldownTicks > 0)
        {
            yield return new Command_Action
            {
                defaultLabel = "DEV: Reset cooldown",
                action = () => remainingCooldownTicks = 0,
            };
        }
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

    private void InitComps()
    {
        compOxygen = parent.GetComp<CompApparelOxygenProvider>();
        // We don't use this value
        remainingCharges = 0;
    }

    protected Command_VerbTarget_Oxygen CreateVerbTargetCommand_Oxygen(Thing gear, Verb verb)
    {
        var command = new Command_VerbTarget_Oxygen(this)
        {
            defaultDesc = gear.def.description,
            hotKey = Props.hotKey,
            defaultLabel = verb.verbProps.label,
            verb = verb
        };

        if (verb.verbProps.defaultProjectile != null && verb.verbProps.commandIcon == null)
        {
            command.icon = verb.verbProps.defaultProjectile.uiIcon;
            command.iconAngle = verb.verbProps.defaultProjectile.uiIconAngle;
            command.iconOffset = verb.verbProps.defaultProjectile.uiIconOffset;
            command.overrideColor = verb.verbProps.defaultProjectile.graphicData.color;
        }
        else
        {
            command.icon = verb.UIIcon != BaseContent.BadTex ? verb.UIIcon : gear.def.uiIcon;
            command.iconAngle = gear.def.uiIconAngle;
            command.iconOffset = gear.def.uiIconOffset;
            command.defaultIconColor = gear.DrawColor;
        }

        if (!Wearer.IsColonistPlayerControlled)
            command.Disable("CannotOrderNonControlled".Translate());
        else if (verb.verbProps.violent && Wearer.WorkTagIsDisabled(WorkTags.Violent))
            command.Disable("IsIncapableOfViolenceLower".Translate(Wearer.LabelShort, Wearer).CapitalizeFirst() + ".");
        else if (!CanBeUsed(out var text))
            command.Disable(text);

        return command;
    }
}