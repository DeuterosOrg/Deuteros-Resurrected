using System;
using System.Collections.Generic;
using System.Linq;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Utility;
using Godot;
using static Deuteros.Code.Enums;
using AccPanel = Deuteros.Code.Platform.Screens.ACC;
using InteriorPanel = Deuteros.Code.Platform.Screens.ShipInterior;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private void RunAccBacklogRegressions()
        {
            foreach (var shuttle in new[] { true, false })
            {
                var hull = shuttle ? "shuttle" : "IOS";
                foreach (var stocks in new[]
                {
                    (1000, 0, 500, 500, 500), (1001, 0, 501, 500, 500), (100, 100, 100, 100, 0),
                    (0, 0, 0, 0, 0), (0, 1000, 500, 500, 0), (50000, 49901, 49951, 49950, 49),
                    (5000, 0, 2500, 2500, 500)
                })
                    Check($"ACC {hull} balances shared stocks {stocks.Item1}/{stocks.Item2}",
                        () => BalanceAccStocks(shuttle, stocks.Item1, stocks.Item2, stocks.Item3, stocks.Item4, stocks.Item5));
                Check($"ACC {hull} balances two minerals across multiple pods", () => BalanceAccMinerals(shuttle));
                Check($"ACC {hull} preserves cargo when both endpoints are full", () => PreserveAccOverflow(shuttle));
                foreach (var destination in new[] { false, true })
                    CheckUi($"ACC {hull} activates immediately at {(destination ? "destination" : "source")}",
                        () => ImmediateAccActivation(shuttle, destination));
                CheckUi($"ACC {hull} waits for missing fuel and resumes when supplied", () => AccWaitsForFuel(shuttle));
            }
        }

        private Ship AccBacklogShip(bool shuttle)
        {
            Save.Ships.Clear();
            Ship ship = shuttle ? new Shuttle { OnGround = true, ShipType = Ship_Types.Shuttle } : NavigationShip();
            ship.Name = "Cargo regression";
            ship.ShipState = Ship_States.Docked;
            ship.PlanetLocation = StellarBodies.earth;
            ship.DestinationPlanetLocation = shuttle ? StellarBodies.earth : StellarBodies.the_moon;
            ship.StarLocation = ship.DestinationStarLocation = StellarBodies.the_sun;
            ship.Engine = true;
            ship.Fuel = 0;
            ship.FuelType = shuttle ? ItemTypes.meh_fuel : ItemTypes.hed_fuel;
            ship.Pilot = new Staff { Leader = "Pilot", Type = StaffType.Marines, Count = 10 };
            ship.Modules = Enumerable.Range(0, 2).Select(_ => new ShipModule { ModuleType = Module_Types.Supply }).ToList();
            ship.ACC = new ACC
            {
                Ship = ship, Source = StellarBodies.earth,
                Destination = shuttle ? StellarBodies.earth : StellarBodies.the_moon,
                SourceItems = new List<ItemTypes>(), DestinationItems = new List<ItemTypes>(),
                CurrentSource = ItemTypes.iron, CurrentDestination = ItemTypes.iron
            };
            Save.Ships.Add(ship);
            AccEndpointStore(ship, false).Items.Clear();
            AccEndpointStore(ship, true).Items.Clear();
            return ship;
        }

        private Store AccEndpointStore(Ship ship, bool destination)
        {
            if (ship is Shuttle)
                return destination ? GameCore.Earth.Station.Resources.Stores : GameCore.Earth.PlanetResources.Stores;
            return Save.BaseGameData.Planets[destination ? StellarBodies.the_moon : StellarBodies.earth].Station.Resources.Stores;
        }

        private static void AtAccEndpoint(Ship ship, bool destination)
        {
            ship.ShipState = Ship_States.Docked;
            if (ship is Shuttle shuttle) shuttle.OnGround = !destination;
            else
            {
                ship.PlanetLocation = destination ? StellarBodies.the_moon : StellarBodies.earth;
                ship.DestinationPlanetLocation = destination ? StellarBodies.earth : StellarBodies.the_moon;
            }
        }

        private void AssertAccTotal(Ship ship, ItemTypes item, int expected)
        {
            var total = AccEndpointStore(ship, false)[item] + AccEndpointStore(ship, true)[item]
                + ship.Modules.Where(m => m.ItemStored == item).Sum(m => m.ItemCount);
            Equal(expected, total, "stock plus cargo conservation");
            foreach (var destination in new[] { false, true })
                Equal(true, AccEndpointStore(ship, destination)[item] <= 50000, "store capacity");
            Equal(true, ship.Modules.All(m => m.ItemCount >= 0 && m.ItemCount <= 250), "pod capacity");
        }

        private void BalanceAccStocks(bool shuttle, int source, int destination, int expectedSource, int expectedDestination, int expectedFirstCargo)
        {
            var ship = AccBacklogShip(shuttle);
            ship.ACC.SourceItems.Add(ItemTypes.iron);
            ship.ACC.DestinationItems.Add(ItemTypes.iron);
            AccEndpointStore(ship, false)[ItemTypes.iron] = source;
            AccEndpointStore(ship, true)[ItemTypes.iron] = destination;

            for (var trip = 0; trip < 7; trip++)
                foreach (var atDestination in new[] { false, true })
                {
                    AtAccEndpoint(ship, atDestination);
                    ship.ACC.LoadSupply();
                    AssertAccTotal(ship, ItemTypes.iron, source + destination);
                    if (trip == 0 && !atDestination)
                        Equal(expectedFirstCargo, ship.Modules.Sum(m => m.ItemCount), "pods do not overshoot the needed transfer");
                }

            Equal(expectedSource, AccEndpointStore(ship, false)[ItemTypes.iron], "balanced source");
            Equal(expectedDestination, AccEndpointStore(ship, true)[ItemTypes.iron], "balanced destination");
            Equal(0, ship.Modules.Sum(m => m.ItemCount), "balanced cargo stays empty instead of shuttling back");
        }

        private void BalanceAccMinerals(bool shuttle)
        {
            var ship = AccBacklogShip(shuttle);
            foreach (var item in new[] { ItemTypes.iron, ItemTypes.carbon })
            {
                ship.ACC.SourceItems.Add(item);
                ship.ACC.DestinationItems.Add(item);
            }
            AccEndpointStore(ship, false)[ItemTypes.iron] = 800;
            // Existing inbound cargo must be unloaded before calculating all outbound pods.
            foreach (var pod in ship.Modules)
            {
                pod.ItemStored = ItemTypes.iron;
                pod.ItemCount = 100;
            }
            AccEndpointStore(ship, true)[ItemTypes.carbon] = 1000;
            for (var trip = 0; trip < 3; trip++)
                foreach (var destination in new[] { false, true })
                {
                    AtAccEndpoint(ship, destination);
                    ship.ACC.LoadSupply();
                    AssertAccTotal(ship, ItemTypes.iron, 1000);
                    AssertAccTotal(ship, ItemTypes.carbon, 1000);
                }
            foreach (var destination in new[] { false, true })
                foreach (var item in new[] { ItemTypes.iron, ItemTypes.carbon })
                    Equal(500, AccEndpointStore(ship, destination)[item], "balanced mineral at endpoint");
            Equal(0, ship.Modules.Sum(m => m.ItemCount), "both minerals stop travelling at balance");
        }

        private void PreserveAccOverflow(bool shuttle)
        {
            var ship = AccBacklogShip(shuttle);
            ship.ACC.SourceItems.Add(ItemTypes.iron);
            ship.ACC.DestinationItems.Add(ItemTypes.iron);
            AccEndpointStore(ship, false)[ItemTypes.iron] = 49990;
            AccEndpointStore(ship, true)[ItemTypes.iron] = 50000;
            foreach (var pod in ship.Modules)
            {
                pod.ItemStored = ItemTypes.iron;
                pod.ItemCount = 250;
            }
            foreach (var destination in new[] { false, true, false })
            {
                AtAccEndpoint(ship, destination);
                ship.ACC.LoadSupply();
                AssertAccTotal(ship, ItemTypes.iron, 100490);
            }
            Equal(50000, AccEndpointStore(ship, false)[ItemTypes.iron], "partial unload fills source");
            Equal(50000, AccEndpointStore(ship, true)[ItemTypes.iron], "destination stays full");
            Equal(490, ship.Modules.Sum(m => m.ItemCount), "unaccepted cargo remains aboard");
            // Space opening later allows the retained cargo to unload normally.
            AccEndpointStore(ship, true)[ItemTypes.iron] = 49510;
            AtAccEndpoint(ship, true);
            ship.ACC.LoadSupply();
            AssertAccTotal(ship, ItemTypes.iron, 100000);
            Equal(0, ship.Modules.Sum(m => m.ItemCount), "retained cargo unloads when room appears");
        }

        private void ImmediateAccActivation(bool shuttle, bool destination)
        {
            foreach (var cycle in new[] { false, true })
                ActivateAccFromUi(shuttle, destination, cycle, false);
        }

        private void AccWaitsForFuel(bool shuttle) => ActivateAccFromUi(shuttle, false, false, true);

        private void ActivateAccFromUi(bool shuttle, bool destination, bool cycle, bool missingFuel)
        {
            var ship = AccBacklogShip(shuttle);
            AtAccEndpoint(ship, destination);
            Save.CurrentPlanet = ship.PlanetLocation;
            Save.CurrentDay = 42;
            var stores = AccEndpointStore(ship, destination);
            var fuelCapacity = shuttle ? 100 : 250;
            stores[ship.FuelType] = missingFuel ? 0 : fuelCapacity;
            stores[ItemTypes.iron] = 12;
            GameCore.SingletonInstance.ShipSelected = ship.ShipID;
            var interior = OpenUi<InteriorPanel>("res://Screens/ShipInterior.tscn");
            try
            {
                Press(interior, "OpenACC");
                var panel = interior.GetNode<Control>("ACCScreen").GetChildren().OfType<AccPanel>().Single();
                Press(panel, $"Window/{(destination ? "DestinationButtons" : "SourceButtons")}/Col01/00");
                Press(panel, "Window/Buttons/" + (cycle ? "Cycle" : "Engage"));
                Equal(true, ship.ACC.Active, "ACC immediately active");
                Equal(cycle, ship.ACC.CycleMode, "requested cycle mode");
                Equal((uint)42, Save.CurrentDay, "activation needs no day tick");
                Equal(0, interior.GetNode<Control>("ACCScreen").GetChildCount(), "controller closes immediately");
                if (missingFuel)
                {
                    Equal(Ship_States.Docked, ship.ShipState, "no departure without fuel");
                    Equal(true, ship.ACC.Refuelling, "waiting for fuel");
                    Equal(0, ship.Modules.Sum(m => m.ItemCount), "cargo untouched until refuel succeeds");
                    stores[ship.FuelType] = fuelCapacity;
                    ship.ACC.Update(Ship_States.Docked);
                }
                Equal(shuttle && !destination ? Ship_States.TakingOff : Ship_States.Launching,
                    ship.ShipState, "departure begins immediately");
                Equal(fuelCapacity, ship.Fuel, "fuel loaded from current endpoint");
                Equal(0, stores[ship.FuelType], "fuel stock charged once");
                Equal(12, ship.Modules.Sum(m => m.ItemCount), "cargo loaded from current endpoint");
                Equal(0, stores[ItemTypes.iron], "cargo stock charged once");
                Equal((uint)42, ship.StartTravelDay, "departure timestamp");
                Equal(false, ship.ACC.Refuelling, "fuel wait cleared");
            }
            finally
            {
                GetTree().CurrentScene.GetNode<GlobalInput>("VirtualCursorView").Unlock();
                interior.Free();
            }
        }
    }
}
