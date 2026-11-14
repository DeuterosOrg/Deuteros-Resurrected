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
        private void RunAccFuelRegressions()
        {
            foreach (var destination in new[] { false, true })
            {
                var endpoint = destination ? "destination" : "source";
                Check($"ACC {endpoint} HeD cursor terminates with empty filters", () => EmptyHedCycle(destination, false));
                Check($"ACC {endpoint} HeD cursor terminates with depleted stock", () => EmptyHedCycle(destination, true));
                Check($"ACC {endpoint} saves HeD selection and loads fuel across the cycle boundary", () => LoadHedCycle(destination));
            }
        }

        private IOS HedCargoShip(bool destination)
        {
            var ship = NavigationShip();
            ship.PlanetLocation = destination ? StellarBodies.the_moon : StellarBodies.earth;
            ship.DestinationPlanetLocation = destination ? StellarBodies.earth : StellarBodies.the_moon;
            ship.Modules[0].ModuleType = Module_Types.Supply;
            ship.ACC = new ACC
            {
                Ship = ship,
                Source = StellarBodies.earth,
                Destination = StellarBodies.the_moon,
                SourceItems = new List<ItemTypes>(),
                DestinationItems = new List<ItemTypes>(),
                CurrentSource = ItemTypes.hed_fuel,
                CurrentDestination = ItemTypes.hed_fuel
            };
            Save.Ships.Add(ship);
            Save.BaseGameData.Planets[ship.PlanetLocation].Station.Resources.Stores.Items.Clear();
            return ship;
        }

        private void EmptyHedCycle(bool destination, bool selected)
        {
            var ship = HedCargoShip(destination);
            if (selected)
                (destination ? ship.ACC.DestinationItems : ship.ACC.SourceItems).Add(ItemTypes.hed_fuel);

            // The pre-fix cycle wraps at MeH and can never return to a HeD start cursor.
            // Run in the harness's isolated process with its external timeout.
            ship.ACC.LoadSupply();

            Equal(0, ship.Modules[0].ItemCount, "empty pod remains empty");
            Equal(ItemTypes.none, ship.Modules[0].ItemStored, "empty pod has no cargo type");
            Equal(ItemTypes.hed_fuel, destination ? ship.ACC.CurrentDestination : ship.ACC.CurrentSource,
                "one complete cycle returns to the initial HeD cursor");
            Equal(0, Save.BaseGameData.Planets[ship.PlanetLocation].Station.Resources.Stores[ItemTypes.hed_fuel],
                "no fuel created while cycling");
        }

        private void LoadHedCycle(bool destination)
        {
            var ship = HedCargoShip(destination);
            (destination ? ship.ACC.DestinationItems : ship.ACC.SourceItems).Add(ItemTypes.hed_fuel);
            Save.BaseGameData.Planets[ship.PlanetLocation].Station.Resources.Stores[ItemTypes.hed_fuel] = 400;

            var restored = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            ship = (IOS)restored.Ships.Single(s => s.ShipID == ship.ShipID);
            Equal(ItemTypes.hed_fuel, ship.ACC.CurrentSource, "saved source HeD selection");
            Equal(ItemTypes.hed_fuel, ship.ACC.CurrentDestination, "saved destination HeD selection");
            Equal(true, ReferenceEquals(ship, ship.ACC.Ship), "restored cargo computer owner");
            GameCore.SingletonInstance.GameData.ActiveSaveFile = restored;

            // Begin immediately before HeD to prove it is included in the cycle,
            // rather than succeeding only when HeD happens to be the first item.
            if (destination) ship.ACC.CurrentDestination = ItemTypes.meh_fuel;
            else ship.ACC.CurrentSource = ItemTypes.meh_fuel;
            var stores = Save.BaseGameData.Planets[ship.PlanetLocation].Station.Resources.Stores;
            for (var cycle = 0; cycle < 2; cycle++)
            {
                ship.ACC.LoadSupply();
                Equal(ItemTypes.hed_fuel, ship.Modules[0].ItemStored, "HeD cargo selected");
                Equal(250, ship.Modules[0].ItemCount, "pod filled to capacity");
                Equal(150, stores[ItemTypes.hed_fuel], "remaining station fuel");
                Equal(400, stores[ItemTypes.hed_fuel] + ship.Modules[0].ItemCount, "fuel conserved across unload/reload");
                Equal(ItemTypes.iron, destination ? ship.ACC.CurrentDestination : ship.ACC.CurrentSource,
                    "cycle wraps after HeD");
            }
        }
    }
}
