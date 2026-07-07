using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace VanillaGravshipExpanded2
{
    public class QuestNode_DerelictGravshipThreat : QuestNode
    {
        public override bool TestRunInt(Slate slate) => true;
        public override void RunInt()
        {
            var slate = QuestGen.slate;
            var derelict = slate.Get<MapParent_DerelictGravship>("worldObject");
            derelict.threatType = (DerelictThreatType)Rand.RangeInclusive(0, 3);
            var threatDesc = "";
            var faction = Faction.OfInsects;
            switch (derelict.threatType)
            {
                case DerelictThreatType.Exoinsectoids:
                    threatDesc = "VGE_ThreatDescription_Exoinsectoids".Translate();
                    faction = Faction.OfInsects;
                    break;
                case DerelictThreatType.Mechanoids:
                    threatDesc = "VGE_ThreatDescription_Mechanoids".Translate();
                    faction = Faction.OfMechanoids;
                    break;
                case DerelictThreatType.Drones:
                    threatDesc = "VGE_ThreatDescription_Drones".Translate();
                    faction = Faction.OfAncientsHostile;
                    break;
                case DerelictThreatType.Pirates:
                    threatDesc = "VGE_ThreatDescription_Pirates".Translate();
                    faction = Faction.OfSalvagers;
                    break;
            }
            derelict.SetFaction(faction);
            derelict.threatPoints = StorytellerUtility.DefaultThreatPointsNow(Find.World);
            slate.Set("threatDescription", threatDesc);
        }
    }
}
