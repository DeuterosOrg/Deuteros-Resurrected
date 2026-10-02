using System.Linq;
using Deuteros.Code;
using Deuteros.Code.Platform;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private void EnemyProductionOverdueDeadline()
        {
            GameCore.SingletonInstance.SetProcess(false);
            Save.AtWar = true;
            Save.Ships.Clear();
            foreach (var planet in Save.BaseGameData.Planets.Values) planet.ActiveMethanoid = false;
            Save.EnemyBuildDay = 0;
            EnemyDroneBuilder.BuildDrones(99, 100);
            var station = Save.BaseGameData.Planets.Values.First();
            station.ActiveMethanoid = true;
            station.Station.Resources.Stores[ItemTypes.ios_drone] = 0;
            EnemyDroneBuilder.BuildDrones(100, 101);
            Equal(2, station.Station.Resources.Stores[ItemTypes.ios_drone], "recapture resumes production after zero-system interval");
            Equal(1, Save.AlienTransmissions.EnemySystems, "recapture refreshes the hostile-system sample");
            Save.EnemyBuildDay = 107;
            EnemyDroneBuilder.BuildDrones(106, 109);
            Equal(4, station.Station.Resources.Stores[ItemTypes.ios_drone], "crossing a deadline performs one production batch");
            Equal((uint)116, Save.EnemyBuildDay, "overdue batch schedules from the current day");
            Save.AtWar = false;
            EnemyDroneBuilder.BuildDrones(115, 116);
            Equal(4, station.Station.Resources.Stores[ItemTypes.ios_drone], "peace still prevents production");
        }

        private void EnemyProductionRemainingSystems()
        {
            GameCore.SingletonInstance.SetProcess(false);
            Save.AtWar = true;
            Save.Ships.Clear();
            var stars = Save.BaseGameData.Stars.Keys.ToArray();
            // Original $38A42 intervals translated through the existing whole-day scheduler.
            var nextDays = new uint[] { 100, 107, 110, 109, 109, 109, 108, 107, 107, 108 };
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
                EnemyDroneBuilder.BuildDrones(99, 100);
                Equal(nextDays[count], Save.EnemyBuildDay, "production interval follows remaining hostile systems " + count);
                Equal(count, Save.AlienTransmissions.EnemySystems, "production and story share the same sampled count");
            }
            var station = Save.BaseGameData.Planets.Values.First();
            station.ActiveMethanoid = true;
            station.Station.Resources.Stores[ItemTypes.ios_drone] = 0;
            Save.EnemyBuildDay = 0;
            Save.StarSystemsCaptured = int.MaxValue; // Historical unused field cannot index production.
            EnemyDroneBuilder.BuildDrones(99, 100);
            Equal(2, station.Station.Resources.Stores[ItemTypes.ios_drone], "first production batch");
            GameCore.SingletonInstance.GameData.ActiveSaveFile = Deuteros.Code.Utility.SaveStorage.Deserialize(
                Deuteros.Code.Utility.SaveStorage.Serialize(Save));
            station = Save.BaseGameData.Planets[station.PlanetId];
            EnemyDroneBuilder.BuildDrones(105, 106);
            Equal(2, station.Station.Resources.Stores[ItemTypes.ios_drone], "saved deadline prevents early production");
            EnemyDroneBuilder.BuildDrones(106, 107);
            Equal(4, station.Station.Resources.Stores[ItemTypes.ios_drone], "saved deadline produces the next batch");
        }
    }
}
