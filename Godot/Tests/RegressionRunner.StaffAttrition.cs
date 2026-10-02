using System;
using System.Linq;
using System.Reflection;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Utility;
using Newtonsoft.Json.Linq;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private void RunStaffAttritionRegressions()
        {
            Check("Staff attrition consumes only crossed 100-day boundaries", AttritionBoundaries);
            Check("Staff countdown one reaches zero before either random draw", AttritionCountdown);
            Check("Attrition uses both rolls and can remove the final member", AttritionRolls);
            Check("Attrition visits all active human staff once and excludes enemy fleets", AttritionLocations);
            Check("Cryopod transfer freezes and resumes the retained countdown", AttritionCryopods);
            Check("Attrition countdown and global phase survive save and load", AttritionSave);
            Check("Older saves start staff countdowns at zero without retroactive losses", AttritionLegacySave);
            Check("Normal simulation runs attrition before display observers", AttritionDayIntegration);
            Check("Training replenishes an empty team without resetting rank or attrition", AttritionTraining);
            Check("Empty research and production teams make no progress until replenished", AttritionWorkforce);
        }

        private Staff AttritionTeam(StaffType type = StaffType.Marines, int countdown = 0, int count = 10) =>
            new Staff { Leader = "Attrition", Type = type, Count = count, AttritionCountdown = countdown };

        private void ClearAttritionWorld()
        {
            Save.Ships.Clear();
            GameCore.Earth.ResearchStaff = null;
            GameCore.Earth.Factory.Builder = null;
            foreach (var planet in Save.BaseGameData.Planets.Values)
            {
                planet.PlanetResources.RemoveAllStaff();
                planet.Station.Resources.RemoveAllStaff();
                planet.Station.Factory.Builder = null;
            }
        }

        private void AttritionBoundaries()
        {
            ClearAttritionWorld();
            var team = AttritionTeam(countdown: 3);
            GameCore.Earth.PlanetResources.AddStaff(team);
            Func<int> noRoll = () => throw new InvalidOperationException("Unexpected random draw");
            StaffAttrition.Advance(Save, 0, 99, noRoll);
            StaffAttrition.Advance(Save, 99, 99, noRoll);
            StaffAttrition.Advance(Save, 101, 99, noRoll);
            Equal(3, team.AttritionCountdown, "no boundary or backwards date does not age staff");
            StaffAttrition.Advance(Save, 99, 100, noRoll);
            Equal(2, team.AttritionCountdown, "first global boundary");
            StaffAttrition.Advance(Save, 100, 299, noRoll);
            Equal(1, team.AttritionCountdown, "one further boundary despite a multi-day interval");
            StaffAttrition.Advance(Save, 299, 300, noRoll);
            Equal(0, team.AttritionCountdown, "third boundary reaches zero without roll");
            Equal(10, team.Count, "no premature member loss");
        }

        private void AttritionCountdown()
        {
            ClearAttritionWorld();
            var team = AttritionTeam(countdown: 1);
            GameCore.Earth.PlanetResources.AddStaff(team);
            var calls = 0;
            int Roll() { calls++; return calls == 1 ? 1 : 15; }
            StaffAttrition.Advance(Save, 99, 100, Roll);
            Equal(0, team.AttritionCountdown, "subtract before underflow");
            Equal(0, calls, "no draws at one to zero");
            StaffAttrition.Advance(Save, 100, 200, Roll);
            Equal(9, team.Count, "next boundary can lose one member");
            Equal(15, team.AttritionCountdown, "rearm maximum");
            Equal(2, calls, "exactly two random draws");
        }

        private void AttritionRolls()
        {
            ClearAttritionWorld();
            var team = AttritionTeam(count: 1);
            GameCore.Earth.PlanetResources.AddStaff(team);
            var calls = 0;
            StaffAttrition.Advance(Save, 0, 100, () => { calls++; return 0; });
            Equal(2, calls, "no-loss outcome still rearms");
            Equal(1, team.Count, "zero roll preserves member");
            Equal(0, team.AttritionCountdown, "rearm minimum permits another attempt next gate");
            StaffAttrition.Advance(Save, 100, 200, () => { calls++; return 17; });
            Equal(0, team.Count, "last member can be lost");
            Equal(1, team.AttritionCountdown, "rearm uses low four bits");
            StaffAttrition.Advance(Save, 200, 400, () => { calls++; return 1; });
            Equal(6, calls, "zero-size team still consumes two draws on underflow");
            Equal(0, team.Count, "zero cannot wrap or become negative");
        }

        private void AttritionLocations()
        {
            ClearAttritionWorld();
            var teams = Enumerable.Range(0, 7).Select(_ => AttritionTeam(countdown: 2)).ToArray();
            GameCore.Earth.ResearchStaff = teams[0]; teams[0].Type = StaffType.Research;
            GameCore.Earth.Factory.Builder = teams[1]; teams[1].Type = StaffType.Production;
            GameCore.Earth.PlanetResources.AddStaff(teams[2]);
            GameCore.Earth.Station.Resources.AddStaff(teams[3]);
            var mars = Save.BaseGameData.Planets[StellarBodies.mars];
            mars.Station.Factory.Builder = teams[4]; teams[4].Type = StaffType.Production;
            mars.PlanetResources.AddStaff(teams[5]);
            var ship = NavigationShip(); ship.Pilot = teams[6]; Save.Ships.Add(ship);
            // One shared object must not age twice, including loaded reference-preserving graphs.
            mars.Station.Resources.AddStaff(teams[2]);
            var enemy = new EnemyFleet { Pilot = AttritionTeam(countdown: 2) }; Save.Ships.Add(enemy);
            StaffAttrition.Advance(Save, 0, 100, () => throw new InvalidOperationException("Unexpected draw"));
            foreach (var team in teams) Equal(1, team.AttritionCountdown, "every human location ages once");
            Equal(2, enemy.Pilot.AttritionCountdown, "synthetic alien fleet crew is not a human roster entry");
        }

        private void AttritionCryopods()
        {
            ClearAttritionWorld();
            var team = AttritionTeam(countdown: 1);
            var other = AttritionTeam(countdown: 3);
            var ship = NavigationShip(); ship.Pilot = null; Save.Ships.Add(ship);
            var pod = ship.Modules[0]; pod.ModuleType = Module_Types.Cryo; pod.StaffStored = team;
            GameCore.Earth.PlanetResources.AddStaff(other);
            StaffAttrition.Advance(Save, 0, 100, () => throw new InvalidOperationException("Frozen staff rolled"));
            Equal(1, team.AttritionCountdown, "cryo preserves remainder");
            Equal(2, other.AttritionCountdown, "waiting team ages");
            pod.StaffStored = GameCore.Earth.PlanetResources.SwapStaff(other, team);
            StaffAttrition.Advance(Save, 100, 200, () => throw new InvalidOperationException("Unexpected draw"));
            Equal(0, team.AttritionCountdown, "unloaded team resumes without reset");
            Equal(2, other.AttritionCountdown, "new cryo occupant freezes");
            Equal(10, team.Count, "loading does not heal or lose members");
            var loaded = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            StaffAttrition.Advance(loaded, 200, 300, () => 1);
            Equal(9, loaded.BaseGameData.Planets[StellarBodies.earth].PlanetResources.Staff[0].Count, "unloaded crew remains active after load");
            Equal(2, loaded.Ships.Single().Modules[0].StaffStored.AttritionCountdown, "saved cryo remains frozen");
        }

        private void AttritionSave()
        {
            ClearAttritionWorld();
            var team = AttritionTeam(countdown: 1);
            GameCore.Earth.ResearchStaff = team;
            Save.CurrentDay = 199;
            var loaded = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            var researcher = ((Earth)loaded.BaseGameData.Planets[StellarBodies.earth]).ResearchStaff;
            Equal(1, researcher.AttritionCountdown, "countdown persists");
            StaffAttrition.Advance(loaded, loaded.CurrentDay, ++loaded.CurrentDay, () => throw new InvalidOperationException("Unexpected draw"));
            Equal(0, researcher.AttritionCountdown, "saved day retains global gate phase");
            Equal(1, team.AttritionCountdown, "old world unchanged");
        }

        private void AttritionLegacySave()
        {
            ClearAttritionWorld();
            GameCore.Earth.ResearchStaff = AttritionTeam(countdown: 7);
            Save.CurrentDay = 2399;
            var document = JObject.Parse(SaveStorage.Serialize(Save));
            foreach (var property in document.Descendants().OfType<JProperty>().Where(p => p.Name == "AttritionCountdown").ToList()) property.Remove();
            var loaded = SaveStorage.Deserialize(document.ToString());
            var team = ((Earth)loaded.BaseGameData.Planets[StellarBodies.earth]).ResearchStaff;
            Equal(0, team.AttritionCountdown, "missing field uses original allocator default");
            Equal(10, team.Count, "loading does not replay historical deaths");
            StaffAttrition.Advance(loaded, 2399, 2400, () => 1);
            Equal(9, team.Count, "only the next crossed gate applies");
        }

        private void AttritionDayIntegration()
        {
            ClearAttritionWorld();
            var team = AttritionTeam(countdown: 1);
            GameCore.Earth.PlanetResources.AddStaff(team);
            Save.CurrentDay = 100;
            var observed = -1;
            void Observe(uint previous, uint current) => observed = team.AttritionCountdown;
            var core = GameCore.SingletonInstance;
            core.DayPassed += Observe;
            try { typeof(GameCore).GetMethod("TriggerDay", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(core, new object[] { 99u, 100u }); }
            finally { core.DayPassed -= Observe; }
            Equal(0, observed, "display sees the new workforce");
            Equal(10, team.Count, "one to zero does not remove a member");
        }

        private void AttritionTraining()
        {
            ClearAttritionWorld();
            var team = AttritionTeam(StaffType.Research, countdown: 7, count: 0); team.AddAction(9);
            GameCore.Earth.ResearchStaff = team;
            var training = GameCore.Earth.TrainingData;
            training.ResearcherLocked = true; training.ResearcherDayStart = 0;
            training.ResearcherTrainingCount = 5; training.ResearcherTrainingTime = 24; training.AvailableTrainees = 10;
            training.ChildDayTick(23, 24);
            Equal(team, GameCore.Earth.ResearchStaff, "training reuses depleted team");
            Equal(5, team.Count, "graduates replenish workforce");
            Equal(3, team.GetLevel(), "leader rank preserved");
            Equal(7, team.AttritionCountdown, "training does not rejuvenate countdown");
        }

        private void AttritionWorkforce()
        {
            ClearAttritionWorld();
            var team = AttritionTeam(StaffType.Research, count: 0);
            GameCore.Earth.ResearchStaff = team;
            var item = GameCore.SingletonInstance.GameData.GetItem(ItemTypes.derrick);
            item.Research.Researched = false; item.Research.ResearchValue = 20;
            item.Research.ResearchPercentageComplete = 11; item.Research.ResearchLimit = 100; item.Research.TechLevel = 1;
            GameCore.Earth.CurrentResearchItem = item.Research;
            Research.UpdateResearch(0, 1);
            Equal(20, item.Research.ResearchValue, "empty research does no work");
            var builder = AttritionTeam(StaffType.Production, count: 0);
            var factory = GameCore.Earth.Factory; factory.Builder = builder;
            var order = new ProductionItem(item) { Active = true, Production_Value = 20, Production_Complete = 1 };
            factory.ProductionQueue.Add(order);
            factory.IncrementCurrentProd();
            Equal(20, order.Production_Value, "empty production does no work");
            builder.Count = 200; team.Count = 250;
            factory.IncrementCurrentProd(); Research.UpdateResearch(1, 2);
            Equal(true, order.Production_Value != 20 || order.Production_Complete != 1, "replenished production resumes");
            Equal(true, item.Research.ResearchValue != 20 || item.Research.ResearchPercentageComplete != 11, "replenished research resumes");
        }
    }
}
