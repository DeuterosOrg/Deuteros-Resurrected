using System;
using System.Linq;
using Deuteros.Code.Objects.GameData;

namespace Deuteros.Code.Objects
{
    internal static class ArtifactRecovery
    {
        public static bool Deliver(SaveFile save)
        {
            var item = save.BaseGameData.ItemList.Single(i => i.ItemType == Enums.ItemTypes.alien_artifact);
            if (item.Research.Researched) return false;
            return Apply(save, item, item.Research.ResearchLimit >= 84 ? 100 : item.Research.ResearchLimit + 12);
        }

        public static void RestoreLegacy(SaveFile save)
        {
            var item = save.BaseGameData.ItemList.SingleOrDefault(i => i.ItemType == Enums.ItemTypes.alien_artifact);
            if (item != null)
            {
                item.BuildRequirements ??= new();
                item.OrbitOnly = true;
                item.ToolPod = item.ToolPodSingular = true;
                if (string.IsNullOrWhiteSpace(item.FullName)) item.FullName = "Unknown";
                if (string.IsNullOrWhiteSpace(item.ShortName)) item.ShortName = "Unknown";
            }
            var research = item?.Research;
            if (research == null || research.Researched) return;
            var credit = research.ResearchLimit;
            // Old saves credited 11 per delivery. Original recovery credits 12, with the eighth reaching 100.
            // Other values may be edited saves; leave their history alone.
            if (credit < 0 || credit > 99 || credit % 11 != 0
                || research.ResearchPercentageComplete < 0
                || research.ResearchPercentageComplete > Math.Max(1, credit)) return;
            Apply(save, item, credit >= 88 ? 100 : credit / 11 * 12);
        }

        private static bool Apply(SaveFile save, Item item, int completion)
        {
            var research = item.Research;
            research.ResearchLimit = research.ResearchPercentageComplete = Math.Clamp(completion, 0, 100);
            if (completion > 0) research.Locked = false;
            if (completion < 100 || research.Researched) return false;
            research.Researched = true;
            research.ResearchOrder = save.BaseGameData.ItemList.Count(i => i.Research?.Researched == true);
            item.Locked = false;
            save.AlienTransmissions?.FinalRecovery();
            return true;
        }
    }
}
