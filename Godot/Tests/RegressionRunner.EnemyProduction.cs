using System.Linq;
using Deuteros.Code;
using Deuteros.Code.Platform;
using Deuteros.Code.Objects;
using Deuteros.Code.Utility;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private void EnemyAttackRequiresPlayerStation()
        {
            GameCore.SingletonInstance.SetProcess(false);
            Save.AtWar = true;
            var target = Save.BaseGameData.Planets[StellarBodies.atlantic];
            foreach (var planet in Save.BaseGameData.Planets.Values.Where(p => p.ParentStar == target.ParentStar))
                planet.ActiveMethanoid = true;
            target.Station.BuildParts = 8;
            var fleet = new EnemyFleet { StarLocation = target.ParentStar, PlanetLocation = target.PlanetId,
                DestinationPlanetLocation = target.PlanetId, DroneCount = 200, AttackTrigger = 100 };
            Save.Ships.Add(fleet);
            foreach (var repeats in new[] { 0, 2 })
            {
                fleet.AttackCount = repeats;
                fleet.ProcessAttackTrigger();
                Equal(0, fleet.AttackDay, "enemy-owned system cannot schedule attacks on an old target");
                Equal(repeats, fleet.AttackCount, "no player station leaves repeat state untouched");
            }
            target.ActiveMethanoid = false;
            fleet.AttackCount = 0;
            fleet.ProcessAttackTrigger();
            Equal(true, fleet.AttackDay > 0, "recaptured player station resumes ordinary attack scheduling");
            Equal(target.PlanetId, fleet.DestinationPlanetLocation, "new player station is selected");
        }

        private void EnemyAttackRechecksOwnership()
        {
            GameCore.SingletonInstance.SetProcess(false);
            Save.AtWar = true;
            var target = Save.BaseGameData.Planets[StellarBodies.atlantic];
            var fleet = new EnemyFleet { StarLocation = target.ParentStar, PlanetLocation = target.PlanetId,
                DestinationPlanetLocation = target.PlanetId, DroneCount = 200, AttackTrigger = 100 };
            Save.Ships.Add(fleet);
            foreach (var captured in new[] { true, false })
            {
                target.ActiveMethanoid = captured;
                target.Station.BuildParts = captured ? 8 : 0;
                target.Station.Resources.Stores[ItemTypes.ios_drone] = 177;
                foreach (var capturing in new[] { false, true })
                {
                    fleet.Attacking = capturing;
                    fleet.AttackDay = 1;
                    var news = string.Join("\n", Save.News.GetNews(100));
                    var attacks = target.MethanoidAttackedCount;
                    var before = SaveStorage.Serialize(Save);
                    GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(before);
                    fleet = Save.Ships.OfType<EnemyFleet>().Last();
                    target = Save.BaseGameData.Planets[StellarBodies.atlantic];
                    Save.TimeSkip = true;
                    fleet.ProcessFleet();
                    Equal(false, fleet.Attacking, "invalid target cannot enter or remain in capture phase");
                    Equal(0, fleet.AttackDay, "expired invalid target countdown clears");
                    Equal(true, Save.TimeSkip, "invalid target cannot interrupt time advance");
                    Equal(attacks, target.MethanoidAttackedCount, "invalid target does not count as an attack");
                    Equal(177, target.Station.Resources.Stores[ItemTypes.ios_drone], "invalid capture preserves station stock");
                    Equal(news, string.Join("\n", Save.News.GetNews(100)), "invalid target emits no attack or capture news");
                }
            }
            target.ActiveMethanoid = false;
            target.Station.BuildParts = 8;
            fleet.AttackDay = 1;
            fleet.ProcessFleet();
            Equal(true, fleet.Attacking, "valid player station starts capture countdown");
            Equal(5, fleet.AttackDay, "valid attack preserves the five-update capture window");
            for (var tick = 0; tick < 5; tick++) fleet.ProcessFleet();
            Equal(true, target.ActiveMethanoid, "valid attack still captures the player station");
        }

        private void EnemyProductionOverdueDeadline()
        {
            GameCore.SingletonInstance.SetProcess(false);
            Save.AtWar = true;
            Save.Ships.Clear();
            foreach (var planet in Save.BaseGameData.Planets.Values) planet.ActiveMethanoid = false;
            Save.EnemyBuildDay = 0;
            Save.Clock.DateCentidays = 10000;
            EnemyDroneBuilder.BuildDrones(99, 100);
            var station = Save.BaseGameData.Planets.Values.First();
            station.ActiveMethanoid = true;
            station.Station.Resources.Stores[ItemTypes.ios_drone] = 0;
            Save.Clock.DateCentidays = 10100;
            EnemyDroneBuilder.BuildDrones(100, 101);
            Equal(2, station.Station.Resources.Stores[ItemTypes.ios_drone], "recapture resumes production after zero-system interval");
            Equal(1, Save.AlienTransmissions.EnemySystems, "recapture refreshes the hostile-system sample");
            Save.EnemyBuildDay = 10700;
            Save.Clock.DateCentidays = 10900;
            EnemyDroneBuilder.BuildDrones(106, 109);
            Equal(4, station.Station.Resources.Stores[ItemTypes.ios_drone], "crossing a deadline performs one production batch");
            Equal(11600ul, Save.EnemyBuildDay, "overdue batch schedules from the current day");
            Save.AtWar = false;
            Save.Clock.DateCentidays = 11600;
            EnemyDroneBuilder.BuildDrones(115, 116);
            Equal(4, station.Station.Resources.Stores[ItemTypes.ios_drone], "peace still prevents production");
        }

        private void EnemyProductionRemainingSystems()
        {
            GameCore.SingletonInstance.SetProcess(false);
            Save.AtWar = true;
            Save.Ships.Clear();
            var stars = Save.BaseGameData.Stars.Keys.ToArray();
            // Original $38A42 raw intervals retain the half-day value.
            var nextDates = new ulong[] { 10000, 10700, 11000, 10950, 10900, 10900, 10800, 10700, 10700, 10800 };
            for (var count = 9; count >= 0; count--)
            {
                foreach (var planet in Save.BaseGameData.Planets.Values) planet.ActiveMethanoid = false;
                for (var index = 0; index < count; index++)
                {
                    var planet = Save.BaseGameData.Planets.Values.First(p => p.ParentStar == stars[index]);
                    planet.ActiveMethanoid = true;
                    planet.Station.Resources.Stores[ItemTypes.ios_drone] = 0;
                }
                Save.EnemyBuildDay = 0;
                Save.Clock.DateCentidays = 10000;
                EnemyDroneBuilder.BuildDrones(99, 100);
                Equal(nextDates[count], Save.EnemyBuildDay, "production interval follows remaining hostile systems " + count);
                Equal(count, Save.AlienTransmissions.EnemySystems, "production and story share the same sampled count");
            }
            var station = Save.BaseGameData.Planets.Values.First();
            station.ActiveMethanoid = true;
            station.Station.Resources.Stores[ItemTypes.ios_drone] = 0;
            Save.EnemyBuildDay = 0;
            Save.StarSystemsCaptured = int.MaxValue; // Historical unused field cannot index production.
            Save.Clock.DateCentidays = 10000;
            EnemyDroneBuilder.BuildDrones(99, 100);
            Equal(2, station.Station.Resources.Stores[ItemTypes.ios_drone], "first production batch");
            GameCore.SingletonInstance.GameData.ActiveSaveFile = Deuteros.Code.Utility.SaveStorage.Deserialize(
                Deuteros.Code.Utility.SaveStorage.Serialize(Save));
            station = Save.BaseGameData.Planets[station.PlanetId];
            Save.Clock.DateCentidays = 10600;
            EnemyDroneBuilder.BuildDrones(105, 106);
            Equal(2, station.Station.Resources.Stores[ItemTypes.ios_drone], "saved deadline prevents early production");
            Save.Clock.DateCentidays = 10700;
            EnemyDroneBuilder.BuildDrones(106, 107);
            Equal(4, station.Station.Resources.Stores[ItemTypes.ios_drone], "saved deadline produces the next batch");
        }
    }
}
