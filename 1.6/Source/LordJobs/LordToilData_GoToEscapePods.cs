using System.Collections.Generic;
using RimWorld.Planet;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace VanillaGravshipExpanded2;

public class LordToilData_GoToEscapePods : LordToilData
{
    public Dictionary<Pawn, Thing> targetsForPawns = [];

    public LocomotionUrgency locomotion;
    public bool canDig;
    public bool interruptCurrentJob;
    public PlanetTile dropPodTile;
    public int cachedDropPodDistance;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref locomotion, nameof(locomotion));
        Scribe_Values.Look(ref canDig, nameof(canDig));
        Scribe_Values.Look(ref interruptCurrentJob, nameof(interruptCurrentJob));
        Scribe_Values.Look(ref dropPodTile, nameof(dropPodTile));
        Scribe_Values.Look(ref cachedDropPodDistance, nameof(cachedDropPodDistance));

        if (Scribe.mode == LoadSaveMode.Saving)
            targetsForPawns.RemoveAll(x => x.Key.DestroyedOrNull() || x.Value.DestroyedOrNull());
        Scribe_Collections.Look(ref targetsForPawns, nameof(targetsForPawns), LookMode.Reference, LookMode.Reference);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
            targetsForPawns ??= [];
    }
}