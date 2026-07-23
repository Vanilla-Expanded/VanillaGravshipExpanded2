using RimWorld;
using RimWorld.Planet;
using RimWorld.QuestGen;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class QuestNode_Root_Gravcore_InfestedInstallation : QuestNode_Root_Gravcore
    {
        public override void RunInt()
        {
            var slate = QuestGen.slate;
            var quest = QuestGen.quest;
            if (!TryFindSiteTile(out var tile))
            {
                Log.Error("Could not find valid site tile for infested installation quest.");
                return;
            }
            var text = QuestGenUtility.HardcodedSignalWithQuestID("site.MapGenerated");
            var inSignal = QuestGenUtility.HardcodedSignalWithQuestID("site.MapRemoved");
            var site = QuestGen_Sites.GenerateSite(new SitePartDefWithParams[1]
            {
                new SitePartDefWithParams(InternalDefOf.VGE_InfestedInstallation, new SitePartParams
                {
                    points = slate.Get("points", 0f),
                    threatPoints = slate.Get("points", 0f)
                })
            }, tile, Faction.OfAncientsHostile, hiddenSitePartsPossible: false, null, WorldObjectDefOf.ClaimableSpaceSite);
            slate.Set("site", site);
            quest.SpawnWorldObject(site);
            var choice = new QuestPart_Choice.Choice();
            choice.rewards.Add(new Reward_DefinedThingDef(ThingDefOf.Gravcore));
            quest.RewardChoice().choices.Add(choice);
            quest.Letter(LetterDefOf.NeutralEvent, text, null, null, null, useColonistsFromCaravanArg: false, QuestPart.SignalListenMode.OngoingOnly, label: "VGE_InfestedInstallationLetterLabel".Translate(), text: "VGE_InfestedInstallationLetterText".Translate(), lookTargets: Gen.YieldSingle(site.Map));
            quest.End(QuestEndOutcome.Success, 0, null, text);
            quest.End(QuestEndOutcome.Unknown, 0, null, inSignal);
        }
    }
}
