using System.Collections.Generic;
using System.Linq;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Utility;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private void CrewShipLossNews()
        {
            GameCore.SingletonInstance.SetProcess(false);
            Save.Ships.Clear();
            foreach (var type in new[] { Ship_Types.Shuttle, Ship_Types.IOS, Ship_Types.SCG })
            {
                var ship = LoadedDismantleShip(type);
                ship.Name = "Lost " + type;
                ship.Pilot.Leader = "Aster " + type;
                ship.Pilot.AddAction(10);
                ship.Modules[0].StaffStored.Leader = "Frozen " + type;
                if (type == Ship_Types.SCG) ship.Modules[0].StaffStored.Type = StaffType.Research;
                ship.Modules[0].StaffStored.AddAction(6);
                ship.ACC = null;
                ship.ShipState = Ship_States.UnDocked;
                ship.Fuel = 0;
                ship.FallingCount = 4;
                Save.Ships.Add(ship);
            }
            Deuteros.Code.Platform.Screens.ShipInterior.UpdateShips(0, 1);
            Equal(0, Save.Ships.Count, "three fuel losses commit");
            var reports = Save.News.GetNews(100);
            foreach (var type in new[] { Ship_Types.Shuttle, Ship_Types.IOS, Ship_Types.SCG })
            {
                Equal(1, reports.Count(n => n.Contains("Captain Aster " + type + " Killed.")), "pilot rank and name reported once");
                Equal(1, reports.Count(n => n.Contains((type == Ship_Types.SCG ? "Doctor" : "Engineer") + " Frozen " + type + " Killed.")), "frozen passengers reported once");
            }
            Equal(9, reports.Count, "six crew reports and three ship reports");
            Equal(true, reports[2].Contains("Lost Shuttle Destroyed."), "crew reports precede the vessel loss");
            Deuteros.Code.Platform.Screens.ShipInterior.UpdateShips(1, 2);
            Equal(9, Save.News.GetNews(100).Count, "removed crews cannot repeat reports");
            var restored = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            Equal(string.Join("\n", reports), string.Join("\n", restored.News.GetNews(100)), "crew history survives save/load");
        }

        private void CrewStationLossNews()
        {
            foreach (var capture in new[] { false, true })
            {
                Save.News = new News();
                var target = SdmLossWorld();
                var docked = SdmVictim(target, Ship_States.Docked);
                docked.Pilot.Leader = "Docked Pilot";
                docked.Modules[0].StaffStored.Leader = "Docked Passenger";
                var ground = AttritionTeam(); ground.Leader = "Ground Team";
                var orbit = AttritionTeam(); orbit.Leader = "Orbital Team";
                var builder = AttritionTeam(StaffType.Production); builder.Leader = "Factory Team";
                target.PlanetResources.AddStaff(ground);
                target.Station.Resources.AddStaff(orbit);
                target.Station.Factory.Builder = builder;
                var survivor = SdmVictim(target, Ship_States.UnDocked);
                survivor.PlanetLocation = StellarBodies.mars;
                survivor.Pilot.Leader = "Survivor";
                survivor.Modules[0].StaffStored.Leader = "Survivor Passenger";
                if (capture)
                {
                    target.Station.SdmCountdown = 0;
                    var fleet = new EnemyFleet { DestinationPlanetLocation = target.PlanetId, Modules = new List<ShipModule>() };
                    fleet.CapturePlanet();
                    fleet.CapturePlanet();
                }
                else
                {
                    GameCore.SingletonInstance._Process(1);
                    GameCore.SingletonInstance._Process(1);
                }
                var reports = Save.News.GetNews(100);
                foreach (var name in new[] { "Docked Pilot", "Docked Passenger", "Ground Team", "Orbital Team", "Factory Team" })
                    Equal(1, reports.Count(n => n.Contains(name + " Killed.")), "each lost team reported once capture=" + capture);
                Equal(0, reports.Count(n => n.Contains("Survivor")), "distant crew is not declared dead");
                Equal<Staff>(null, target.Station.Factory.Builder, "lost factory staff cannot remain active after capture/destruction");
                Equal(true, Save.Ships.Contains(survivor), "distant ship survives");
            }
            Save.News = new News();
            var earth = SdmLossWorld(StellarBodies.earth);
            var safeShuttle = SdmVictim(earth, Ship_States.Docked, true);
            safeShuttle.Pilot.Leader = "Earth Survivor";
            safeShuttle.Modules[0].StaffStored.Leader = "Earth Passenger";
            earth.PlanetResources.Staff[0] = new Staff { Leader = "Earth Ground", Type = StaffType.Marines, Count = 1 };
            GameCore.SingletonInstance._Process(1);
            Equal(true, Save.Ships.Contains(safeShuttle), "Earth ground shuttle survives orbital destruction");
            Equal(0, Save.News.GetNews(100).Count(n => n.Contains("Earth Survivor") || n.Contains("Earth Passenger") || n.Contains("Earth Ground")),
                "preserved Earth ground crews receive no death report");
        }
    }
}
