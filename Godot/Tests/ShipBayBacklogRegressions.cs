using System;
using System.Collections.Generic;
using System.Linq;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Objects.Interfaces;
using Deuteros.Code.Platform.Helpers;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        public void RunShipBayBacklogRegressions()
        {
            CheckUi("Entering a bay already at the cockpit does not jiggle", BayEntryStaysStill);
            foreach (var type in new[] { Ship_Types.Shuttle, Ship_Types.IOS, Ship_Types.SCG })
                CheckUi($"Dismantling {type} returns crew, fuel and installed cargo exactly once", () => DismantleReturnsContents(type));
            CheckUi("Dismantling with insufficient staff slots leaves everything aboard", () => DismantleBlocked("staff"));
            CheckUi("Dismantling with insufficient combined fuel storage changes nothing", () => DismantleBlocked("stores"));
            CheckUi("Dismantling with grapple salvage requires unloading it first", () => DismantleBlocked("grapple"));
        }

        private IShip LoadedDismantleShip(Ship_Types type)
        {
            IShip ship = type == Ship_Types.Shuttle ? new Shuttle { OnGround = true }
                : type == Ship_Types.IOS ? new IOS() : new SCG();
            ship.ShipType = type;
            ship.ShipState = Ship_States.Docked;
            ship.PlanetLocation = StellarBodies.earth;
            ship.FuelType = type == Ship_Types.SCG ? ItemTypes.hed_fuel : ItemTypes.meh_fuel;
            ship.Fuel = 17;
            ship.Engine = true;
            ship.Pilot = new Staff { Leader = "Pilot", Type = StaffType.Marines, Count = 1 };
            ship.ACC = new Deuteros.Code.Objects.ACC { Ship = ship };
            ship.Modules = Enumerable.Range(0, type == Ship_Types.Shuttle ? 1 : type == Ship_Types.IOS ? 3 : 5)
                .Select(_ => new ShipModule()).ToList();
            ship.Modules[0].ModuleType = Module_Types.Cryo;
            ship.Modules[0].StaffStored = new Staff { Leader = "Crew", Type = StaffType.Production, Count = 8 };
            if (ship is InterStellarShip stellar)
            {
                ship.Modules[1].ModuleType = Module_Types.Supply;
                ship.Modules[1].ItemStored = ItemTypes.iron;
                ship.Modules[1].ItemCount = 200;
                ship.Modules[2].ModuleType = Module_Types.Tool;
                ship.Modules[2].ItemStored = ItemTypes.a__m__a;
                ship.Modules[2].ItemCount = 1;
                stellar.DroneCount = 12;
            }
            return ship;
        }

        private ShipBay OpenDismantleBay(IShip ship)
        {
            Save.Ships.Clear();
            Save.Ships.Add(ship);
            GameCore.Earth.Station.Built = true;
            GameCore.Earth.Station.BuildParts = 8;
            return OpenUi<ShipBay>("res://Screens/ShipBay.tscn", new List<SceneVariables>
            {
                ship is Shuttle ? SceneVariables.Ground : SceneVariables.Orbit,
                ship is Shuttle ? SceneVariables.Shuttle : SceneVariables.Ship
            });
        }

        private void CloseDismantleBay(ShipBay bay, HashSet<Tween> existingTweens)
        {
            if (OverlayManager.Instance.IsOpen) OverlayManager.Instance.CloseOverlay();
            foreach (var tween in GetTree().GetProcessedTweens().Where(t => !existingTweens.Contains(t))) tween.Kill();
            bay.Free();
        }

        private void DismantleReturnsContents(Ship_Types type)
        {
            var ship = LoadedDismantleShip(type);
            var resources = type == Ship_Types.Shuttle ? (Deuteros.Code.Platform.Resource)GameCore.Earth.PlanetResources : GameCore.Earth.Station.Resources;
            var other = type == Ship_Types.Shuttle ? (Deuteros.Code.Platform.Resource)GameCore.Earth.Station.Resources : GameCore.Earth.PlanetResources;
            resources.RemoveAllStaff();
            resources.Stores.Items.Clear();
            other.Stores.Items.Clear();
            var chassis = type == Ship_Types.Shuttle ? ItemTypes.s_chassis : type == Ship_Types.IOS ? ItemTypes.i_chassis : ItemTypes.g_chassis;
            var engine = type == Ship_Types.Shuttle ? ItemTypes.s_drive : type == Ship_Types.IOS ? ItemTypes.i_drive : ItemTypes.star_drive;
            resources.Stores[chassis] = resources.Stores[engine] = 1;
            var pilot = ship.Pilot;
            var crew = ship.Modules[0].StaffStored;
            var existingTweens = GetTree().GetProcessedTweens().ToHashSet();
            var bay = OpenDismantleBay(ship);
            try
            {
                Press(bay, "Buttons/Nav_Dismantle");
                Equal(false, Save.Ships.Contains(ship), "ship removed");
                Equal(1, resources.Staff.Count(s => ReferenceEquals(s, pilot)), "same pilot returned once");
                Equal(1, resources.Staff.Count(s => ReferenceEquals(s, crew)), "same cryo team returned once");
                Equal(8, crew.Count, "team size preserved");
                Equal(17, resources.Stores[ship.FuelType], "fuel returned locally");
                Equal(1, resources.Stores[ItemTypes.cryo_pod], "cryo pod returned");
                Equal(1, resources.Stores[ItemTypes.a__c__c], "ACC returned");
                Equal(1, resources.Stores[chassis], "chassis already in stock is not duplicated");
                Equal(1, resources.Stores[engine], "engine already in stock is not duplicated");
                if (ship is InterStellarShip)
                {
                    Equal(200, resources.Stores[ItemTypes.iron], "cargo returned");
                    Equal(1, resources.Stores[ItemTypes.supply_pod], "supply pod returned");
                    Equal(1, resources.Stores[ItemTypes.tool_pod], "tool pod returned");
                    Equal(1, resources.Stores[ItemTypes.a__m__a], "AMA returned");
                    Equal(12, resources.Stores[type == Ship_Types.IOS ? ItemTypes.ios_drone : ItemTypes.star_drone], "matching drones returned");
                }
                Equal(0, other.Stores.Items.Count, "other location unchanged");
                var returned = resources.Stores.Items.ToDictionary(x => x.Key, x => x.Value);
                Press(bay, "Buttons/Nav_Dismantle");
                Equal(true, returned.OrderBy(x => x.Key).SequenceEqual(resources.Stores.Items.OrderBy(x => x.Key)), "repeat callback cannot duplicate returns");
            }
            finally { CloseDismantleBay(bay, existingTweens); }
        }

        private void DismantleBlocked(string reason)
        {
            var ship = LoadedDismantleShip(Ship_Types.IOS);
            var resources = GameCore.Earth.Station.Resources;
            resources.RemoveAllStaff();
            resources.Stores.Items.Clear();
            if (reason == "staff")
                for (int i = 0; i < 3; i++) resources.AddStaff(new Staff { Leader = "Waiting" + i, Type = StaffType.Marines, Count = 1 });
            if (reason == "stores")
            {
                resources.Stores[ship.FuelType] = 49980;
                ship.Modules[1].ItemStored = ship.FuelType;
                ship.Modules[1].ItemCount = 10;
            }
            if (reason == "grapple")
            {
                ship.Modules[2].ItemStored = ItemTypes.grapple;
                ship.Modules[2].HeldItem = new UnknownItem(UnknownItemTypes.AlienArtifact);
            }
            var stock = resources.Stores.Items.ToDictionary(x => x.Key, x => x.Value);
            var roster = resources.Staff.ToArray();
            var pilot = ship.Pilot;
            var crew = ship.Modules[0].StaffStored;
            var salvage = ship.Modules[2].HeldItem;
            var existingTweens = GetTree().GetProcessedTweens().ToHashSet();
            var bay = OpenDismantleBay(ship);
            try
            {
                Press(bay, "Buttons/Nav_Dismantle");
                Equal(true, Save.Ships.Contains(ship), "ship retained until everything can be returned");
                Equal(pilot, ship.Pilot, "pilot retained");
                Equal(crew, ship.Modules[0].StaffStored, "cryo crew retained");
                Equal(salvage, ship.Modules[2].HeldItem, "grapple salvage retained");
                Equal(17, ship.Fuel, "fuel retained");
                Equal(reason == "stores" ? 10 : 200, ship.Modules[1].ItemCount, "cargo retained");
                Equal(true, roster.SequenceEqual(resources.Staff), "roster unchanged");
                Equal(true, stock.OrderBy(x => x.Key).SequenceEqual(resources.Stores.Items.OrderBy(x => x.Key)), "stores unchanged");
                Equal(true, OverlayManager.Instance.IsOpen, "explanation shown");
                var error = OverlayManager.Instance.GetNode("GlobalOverlay/Center").GetChild(0)
                    .GetNode<Label>("ErrorButton/OuterColorRect/InnerColorRect/ErrorLabel").Text;
                Equal(true, error.Contains(reason == "staff" ? "Staff" : reason == "stores" ? "Stores" : "Grapple"), "explanation identifies the blocker");
            }
            finally { CloseDismantleBay(bay, existingTweens); }
        }

        private void BayEntryStaysStill()
        {
            Save.Ships.Clear();
            Save.Ships.Add(new Shuttle
            {
                ShipType = Ship_Types.Shuttle, ShipState = Ship_States.Docked,
                PlanetLocation = StellarBodies.earth, OnGround = true,
                Modules = new List<ShipModule> { new ShipModule() }, FuelType = ItemTypes.meh_fuel
            });
            GameCore.Earth.ShuttleState = 0;
            var existingTweens = GetTree().GetProcessedTweens().ToHashSet();
            var bay = OpenUi<ShipBay>("res://Screens/ShipBay.tscn",
                new List<SceneVariables> { SceneVariables.Ground, SceneVariables.Shuttle });
            var tweens = GetTree().GetProcessedTweens().Where(t => !existingTweens.Contains(t)).ToArray();
            try
            {
                var furthest = bay.ScrollContainer.ScrollHorizontal;
                // Advance the actual animation deterministically, observing its scroll position.
                for (int frame = 0; frame < 60; frame++)
                {
                    foreach (var tween in tweens) tween.CustomStep(0.02);
                    furthest = Math.Max(furthest, bay.ScrollContainer.ScrollHorizontal);
                }
                Equal(0, furthest, "cockpit must remain still throughout entry");
            }
            finally
            {
                foreach (var tween in tweens) tween.Kill();
                bay.Free();
            }
        }
    }
}
