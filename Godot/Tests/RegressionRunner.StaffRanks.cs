using System.Linq;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Utility;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private void RunStaffRankRegressions()
        {
            Check("Marine rank text agrees with effective rank at every promotion boundary", MarineRankBoundaries);
            Check("Saved Captain retains rank text and promotes with matching news at action 40", SavedCaptainPromotion);
            Check("Research and production rank text retain their thresholds and Artisan display", OtherRankBoundaries);
        }

        private void MarineRankBoundaries()
        {
            foreach (var entry in new[]
            {
                (0, StaffLevel_Marines.Pilot), (9, StaffLevel_Marines.Pilot),
                (10, StaffLevel_Marines.Captain), (29, StaffLevel_Marines.Captain),
                (30, StaffLevel_Marines.Captain), (39, StaffLevel_Marines.Captain),
                (40, StaffLevel_Marines.Admiral), (100, StaffLevel_Marines.Admiral)
            })
            {
                var pilot = new Staff { Type = StaffType.Marines, Count = 10, Leader = "Rank Test" };
                pilot.AddAction(entry.Item1);
                Equal((int)entry.Item2, pilot.GetLevel(), $"effective rank after {entry.Item1} actions");
                Equal(entry.Item2.ToScreenString(), pilot.GetLevelString(), $"display after {entry.Item1} actions");
            }
        }

        private void SavedCaptainPromotion()
        {
            Save.Ships.Clear();
            var ship = NavigationShip();
            ship.Pilot = new Staff { Type = StaffType.Marines, Count = 10, Leader = "Rank Test" };
            ship.Pilot.AddAction(30);
            Save.Ships.Add(ship);
            var loaded = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            GameCore.SingletonInstance.GameData.ActiveSaveFile = loaded;
            var pilot = loaded.Ships.Single().Pilot;
            Equal("Captain", pilot.GetLevelString(), "saved 30-action Captain label");
            var before = loaded.News.GetNews(100).Count;
            for (var i = 0; i < 9; i++) pilot.AddAction();
            Equal("Captain", pilot.GetLevelString(), "still Captain at 39");
            Equal(before, loaded.News.GetNews(100).Count, "no premature promotion news");
            pilot.AddAction();
            Equal((int)StaffLevel_Marines.Admiral, pilot.GetLevel(), "effective Admiral at 40");
            Equal("Admiral", pilot.GetLevelString(), "display agrees with promotion");
            Equal(before + 1, loaded.News.GetNews(100).Count, "exactly one promotion report");
            Equal(true, loaded.News.GetNews(1).Single().Contains("New Rank:Admiral"), "news identifies Admiral");
            pilot.AddAction();
            Equal(before + 1, loaded.News.GetNews(100).Count, "no repeated promotion report");
        }

        private void OtherRankBoundaries()
        {
            foreach (var type in new[] { StaffType.Research, StaffType.Production })
            foreach (var actions in new[] { 0, 5, 6, 8, 9, 11, 12 })
            {
                var staff = new Staff { Type = type, Count = 10 };
                staff.AddAction(actions);
                var expected = type == StaffType.Research
                    ? ((StaffLevel_Researcher)staff.GetLevel()).ToScreenString()
                    : ((StaffLevel_Production)staff.GetLevel()).ToScreenString();
                Equal(expected, staff.GetLevelString(), $"{type} rank at {actions}");
                if (type == StaffType.Production)
                    Equal("Artisan", staff.GetLevelString(artisan: true), "Artisan role display unchanged");
            }
        }
    }
}
