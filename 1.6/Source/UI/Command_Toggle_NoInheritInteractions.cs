using Verse;

namespace VanillaGravshipExpanded2;

public class Command_Toggle_NoInheritInteractions : Command_Toggle
{
    // Normally, Command_Toggle inherits interaction if both have the same isActive() value.
    // We don't want it, as the toggle is on a global (map comp) level, not local (thing comp) level.
    public override bool InheritInteractionsFrom(Gizmo other) => alsoClickIfOtherInGroupClicked && base.InheritInteractionsFrom(other);
}