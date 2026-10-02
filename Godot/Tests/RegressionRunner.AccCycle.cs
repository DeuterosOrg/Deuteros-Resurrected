using System.Linq;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Base;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Utility;
using Godot;
using static Deuteros.Code.Enums;
using AccPanel = Deuteros.Code.Platform.Screens.ACC;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private void RunAccCycleRegressions()
        {
            foreach (var shuttle in new[] { true, false })
            {
                CheckUi($"ACC finishing {shuttle} starts one leg through its control", () => AccCycleControl(shuttle, false));
                CheckUi($"ACC finishing {shuttle} waits for fuel without changing modes", () => AccCycleControl(shuttle, true));
                CheckUi($"ACC finishing {shuttle} unloads on arrival without refuelling or reloading", () => AccCycleArrival(shuttle, false));
                CheckUi($"ACC finishing {shuttle} retains overflow and stops on arrival", () => AccCycleArrival(shuttle, true));
                CheckUi($"ACC finishing {shuttle} survives saving with legacy dual flags", () => AccCycleSave(shuttle));
            }
            CheckUi("ACC Engage replaces a finishing request without reloading in transit", () => AccCycleEngage(false));
            CheckUi("ACC finishing does not start automatic asteroid mining", AccCycleAsteroids);
            CheckUi("ACC finishing completes a real shuttle flight and stays stopped", () => AccCycleFlight(false));
            CheckUi("ACC finishing lands a shuttle and stops at ground stores", () => AccCycleFlight(true));
            CheckUi("ACC Engage replaces legacy dual flags in transit", () => AccCycleEngage(true));
            CheckUi("ACC finishing replaces Engage during a fuel wait", AccCycleDuringFuelWait);
        }

        private Ship AccCycleShip(bool shuttle)
        {
            var ship = AccBacklogShip(shuttle);
            Save.CurrentDay = 100;
            Save.CurrentPlanet = ship.PlanetLocation;
            ship.ACC.SourceItems.Add(ItemTypes.iron);
            ship.ACC.DestinationItems.Add(ItemTypes.copper);
            var origin = AccEndpointStore(ship, false);
            var destination = AccEndpointStore(ship, true);
            origin[ship.FuelType] = shuttle ? 100 : 250;
            origin[ItemTypes.iron] = 12;
            destination[ship.FuelType] = 1000;
            destination[ItemTypes.copper] = 99;
            GameCore.Earth.Station.Built = true;
            Save.BaseGameData.Planets[StellarBodies.the_moon].Station.Built = true;
            return ship;
        }

        private void AccCyclePress(Ship ship, string button, bool waiting = false)
        {
            GameCore.SingletonInstance.ShipSelected = ship.ShipID;
            var interior = OpenUi<ShipInterior>("res://Screens/ShipInterior.tscn");
            try
            {
                Press(interior, "OpenACC");
                var panel = interior.GetNode<Control>("ACCScreen").GetChildren().OfType<AccPanel>().Single();
                Press(panel, "Window/Buttons/" + button);
                if (button == "Cycle")
                    Equal(true, interior.GetNode<Label>("TextLayout/ACCStatus").Text.Contains("Finishing"), "finishing status visible");
                if (waiting)
                    Equal(true, interior.GetNode<Label>("TextLayout/Status").Text.Contains("Refueling"), "finishing fuel wait visible");
            }
            finally
            {
                GetTree().CurrentScene.GetNode<GlobalInput>("VirtualCursorView").Unlock();
                interior.Free();
            }
        }

        private void AccCycleControl(bool shuttle, bool missingFuel)
        {
            var ship = AccCycleShip(shuttle);
            var stores = AccEndpointStore(ship, false);
            var capacity = shuttle ? 100 : 250;
            if (missingFuel) stores[ship.FuelType] = 0;
            AccCyclePress(ship, "Cycle", missingFuel);
            Equal(false, ship.ACC.Active, "continuous mode is off");
            Equal(true, ship.ACC.CycleMode, "finish request remains active");
            if (missingFuel)
            {
                Equal(Ship_States.Docked, ship.ShipState, "wait at source");
                Equal(12, stores[ItemTypes.iron], "no cargo loaded before fuel is available");
                stores[ship.FuelType] = capacity;
                ship.ACC.Update(Ship_States.Docked);
            }
            Equal(shuttle ? Ship_States.TakingOff : Ship_States.Launching, ship.ShipState, "one leg starts");
            Equal(12, ship.Modules.Sum(m => m.ItemCount), "outgoing supplies loaded once");
            Equal(capacity, ship.Fuel, "source fuel loaded");
            Equal(true, ship.ACC.CycleMode, "fuel resumption preserves finish mode");
        }

        private void FinishAccAtDestination(Ship ship, bool overflow)
        {
            AtAccEndpoint(ship, true);
            ship.Fuel = 17;
            ship.Modules[0].ItemStored = ItemTypes.iron;
            ship.Modules[0].ItemCount = 12;
            var stores = AccEndpointStore(ship, true);
            stores[ItemTypes.iron] = overflow ? 49995 : 25;
            ship.ACC.Update(Ship_States.Docking);
            Equal(Ship_States.Docked, ship.ShipState, "arrival remains docked");
            Equal(false, ship.ACC.Active, "continuous mode stopped");
            Equal(false, ship.ACC.CycleMode, "finish mode completed");
            Equal(false, ship.ACC.Refuelling, "no arrival fuel wait");
            Equal(17, ship.Fuel, "no arrival refuelling");
            Equal(1000, stores[ship.FuelType], "fuel stock preserved");
            Equal(99, stores[ItemTypes.copper], "return supplies not loaded");
            Equal(overflow ? 50000 : 37, stores[ItemTypes.iron], "delivered ore credited up to capacity");
            Equal(overflow ? 7 : 0, ship.Modules.Sum(m => m.ItemCount), "overflow retained without loss");
            Equal(ItemTypes.copper, ship.ACC.DestinationItems.Single(), "selection retained for next trip");
        }

        private void AccCycleArrival(bool shuttle, bool overflow)
        {
            var ship = AccCycleShip(shuttle);
            ship.ACC.CycleMode = true;
            FinishAccAtDestination(ship, overflow);
        }

        private void AccCycleSave(bool shuttle)
        {
            var ship = AccCycleShip(shuttle);
            ship.ShipState = Ship_States.InTransit;
            ship.ACC.Active = ship.ACC.CycleMode = true; // Older Cycle control saved both flags.
            var loaded = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            GameCore.SingletonInstance.GameData.ActiveSaveFile = loaded;
            ship = (Ship)loaded.Ships.Single();
            FinishAccAtDestination(ship, false);
        }

        private void AccCycleEngage(bool legacy)
        {
            var ship = AccCycleShip(false);
            ship.ShipState = Ship_States.InTransit;
            ship.ACC.CycleMode = true;
            ship.ACC.Active = legacy;
            ship.Fuel = 17;
            ship.Modules[0].ItemStored = ItemTypes.iron;
            ship.Modules[0].ItemCount = 12;
            AccCyclePress(ship, "Engage");
            Equal(true, ship.ACC.Active, "continuous mode enabled");
            Equal(false, ship.ACC.CycleMode, "finish request cancelled");
            Equal(Ship_States.InTransit, ship.ShipState, "current leg unaffected");
            Equal(17, ship.Fuel, "no mid-flight refuel");
            Equal(12, ship.Modules[0].ItemCount, "cargo unaffected");
        }

        private void AccCycleAsteroids()
        {
            var ship = AsteroidAccShip();
            ship.Modules[0].ItemStored = ItemTypes.a__m__a;
            ship.Modules[1].ItemCount = 0;
            ship.ACC.Active = false;
            ship.ACC.CycleMode = true;
            ship.ItemScanResults = new Asteroid { Type = ItemTypes.titanium, Class = 6, Mass = 10000, MassName = "Large" };
            ship.ACC.Update(Ship_States.UnDocked);
            Equal(Ship_States.UnDocked, ship.ShipState, "finish mode cannot start an automatic mining approach");
        }

        private void AccCycleDuringFuelWait()
        {
            var ship = AccCycleShip(false);
            var stores = AccEndpointStore(ship, false);
            stores[ship.FuelType] = 0;
            ship.ACC.Activate();
            Equal(true, ship.ACC.Refuelling, "Engage waits for fuel");
            AccCyclePress(ship, "Cycle", true);
            Equal(false, ship.ACC.Active, "continuous mode replaced");
            stores[ship.FuelType] = 250;
            ship.ACC.Update(Ship_States.Docked);
            Equal(Ship_States.Launching, ship.ShipState, "fuel wait resumes current leg");
            FinishAccAtDestination(ship, false);
        }

        private void AccCycleFlight(bool toGround)
        {
            var ship = AccCycleShip(true);
            if (toGround) AtAccEndpoint(ship, true);
            AccCyclePress(ship, "Cycle");
            for (var i = 0; i < 12; i++)
            {
                var before = Save.CurrentDay++;
                ShipInterior.UpdateShips(before, Save.CurrentDay);
            }
            Equal(toGround, ((Shuttle)ship).OnGround, "one leg reaches requested endpoint");
            Equal(Ship_States.Docked, ship.ShipState, "no return leg begins");
            Equal(false, ship.ACC.CycleMode, "cycle remains finished after later ticks");
            Equal(toGround ? 99 : 12, AccEndpointStore(ship, !toGround)[toGround ? ItemTypes.copper : ItemTypes.iron], "one cargo delivery");
            Equal(toGround ? 12 : 99, AccEndpointStore(ship, !toGround)[toGround ? ItemTypes.iron : ItemTypes.copper], "no return cargo consumed");
        }
    }
}
