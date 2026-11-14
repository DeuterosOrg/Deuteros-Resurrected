using System.Collections.Generic;
using Deuteros.Code;
using Deuteros.Code.Objects;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private void RunDayTickRegressions()
        {
            Check("Day tick updates only the active world's planets once", ActiveWorldDayTick);
            Check("Replacing a world does not run its Earth training twice", ActiveWorldTrainingTick);
            Check("Day display observers see completed simulation after world replacement", DisplayAfterSimulation);
        }

        private void ReplaceTickWorld()
        {
            CoreData.CreateBaseGameData();
            GameCore.SingletonInstance.GameData.ActiveSaveFile = CoreData.CreateNewSaveFile();
            Save.TimeSkip = false;
            Save.TimeSkipDay = false;
        }

        private static void PrepareTickMining(Planet planet)
        {
            planet.BaseBuildParts = 2;
            planet.BaseDamaged = false;
            planet.Station.MtxInstalled = false;
            planet.PlanetResources.Derricks = 1;
            planet.PlanetResources.Materials = new List<Material>
            {
                new Material(ItemTypes.iron, 0) { GroundAmount = 1000 }
            };
            planet.PlanetResources.Stores[ItemTypes.iron] = 0;
        }

        private void AdvanceTickDay()
        {
            Save.TimeSkipDay = true;
            GameCore.SingletonInstance._Process(0);
        }

        private void ActiveWorldDayTick()
        {
            var previousMoon = (Planet)Save.BaseGameData.Planets[StellarBodies.the_moon];
            PrepareTickMining(previousMoon);
            ReplaceTickWorld();
            var activeMoon = (Planet)Save.BaseGameData.Planets[StellarBodies.the_moon];
            PrepareTickMining(activeMoon);

            AdvanceTickDay();

            Equal(0, previousMoon.PlanetResources.Stores[ItemTypes.iron], "inactive world's stock");
            Equal(1000, previousMoon.PlanetResources.Materials[0].GroundAmount, "inactive world's deposit");
            Equal(2, activeMoon.PlanetResources.Stores[ItemTypes.iron], "one active-world mining tick");
            Equal(998, activeMoon.PlanetResources.Materials[0].GroundAmount, "one active-world extraction");
            Equal((uint)1, Save.CurrentDay, "one day advanced");
        }

        private void ActiveWorldTrainingTick()
        {
            ReplaceTickWorld();
            var earth = GameCore.Earth;
            PrepareTickMining(earth);
            earth.ResearchStaff = null;
            var training = earth.TrainingData;
            training.ResearcherTrainingCount = 10;
            training.ResearcherTrainingTime = 1;
            training.ResearcherLocked = false;
            training.AvailableTrainees = 100;

            AdvanceTickDay();

            Equal(true, training.ResearcherLocked, "training starts on first tick");
            Equal(10, training.ResearcherTrainingCount, "same tick must not also complete training");
            Equal(100, training.AvailableTrainees, "recruits remain until completion");
            Equal<Staff>(null, earth.ResearchStaff, "no duplicate Earth tick graduating early");

            AdvanceTickDay();

            Equal(false, training.ResearcherLocked, "training completes on next tick");
            Equal(10, earth.ResearchStaff.Count, "graduates added once");
            Equal(90, training.AvailableTrainees, "recruits charged once");
            Equal(2, earth.PlanetResources.Stores[ItemTypes.iron], "Earth keeps its even-day mining tick");
        }

        private void DisplayAfterSimulation()
        {
            var notifications = 0;
            var observedStock = -1;
            uint observedDay = 0;
            void Observe(uint previousDay, uint currentDay)
            {
                notifications++;
                observedStock = Save.BaseGameData.Planets[StellarBodies.the_moon].PlanetResources.Stores[ItemTypes.iron];
                observedDay = currentDay;
            }

            // Existing screens stay subscribed when a new/load world replaces the models.
            GameCore.SingletonInstance.DayPassed += Observe;
            try
            {
                ReplaceTickWorld();
                PrepareTickMining((Planet)Save.BaseGameData.Planets[StellarBodies.the_moon]);
                AdvanceTickDay();

                Equal(1, notifications, "one display notification");
                Equal(2, observedStock, "display sees this day's completed mining");
                Equal((uint)1, observedDay, "display receives current day");
            }
            finally
            {
                GameCore.SingletonInstance.DayPassed -= Observe;
            }
        }
    }
}
