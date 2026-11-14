using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Utility;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task RunDfccFuelRegressions()
        {
            foreach (var type in new[] { Ship_Types.IOS, Ship_Types.SCG })
            {
                await CheckAsync($"DFCC {type} manual fuel transfers conserve ten stock per gauge unit", () => DfccManualFuel(type));
                CheckUi($"DFCC {type} ACC fuel preserves remainders and threshold behavior", () => DfccAccFuel(type));
                await CheckAsync($"DFCC {type} fitting returns old fuel atomically before conversion", () => DfccFuelFitting(type));
                await CheckAsync($"DFCC {type} dismantling accounts for fuel and cargo together", () => DfccFuelDismantle(type));
            }
            CheckUi("DFCC fuel gauge drain does not multiply again or depend on drone count", DfccFuelBurn);
            CheckUi("Existing DFCC saves retain loaded range and future refuelling uses corrected cost", DfccFuelSave);
            await CheckAsync("Unmodified shuttle and IOS fuel transfers stay one to one", DfccNormalFuel);
        }

        private async Task<ShipBay> DfccFuelBay(Ship_Types type, bool installed = true)
        {
            var bay = await EmptyAssemblyBay(type);
            var stores = bay.ResourceList.Stores;
            stores.Items.Clear();
            stores[AssemblyChassis(type)] = 1;
            Press(bay, "Buttons/Nav_Create_" + type);
            ((InterStellarShip)bay.Ship).DFCC = installed;
            return bay;
        }

        private async Task DfccManualFuel(Ship_Types type)
        {
            var bay = await DfccFuelBay(type);
            var ship = (Ship)bay.Ship;
            var stores = bay.ResourceList.Stores;
            var fuel = ship.FuelType;
            GameCore.Earth.PlanetResources.Stores[fuel] = 1000;
            var plus = "Fuel/FuelGauge/Plus/RepeatingButton";
            var minus = "Fuel/FuelGauge/Minus/RepeatingButton";
            stores[fuel] = 9;
            Press(bay, plus);
            Equal(0, ship.Fuel, "partial stock cannot supply a gauge unit");
            Equal(9, stores[fuel], "partial stock retained");
            stores[fuel] = 20;
            Press(bay, plus);
            Equal(1, ship.Fuel, "one gauge unit loaded");
            Equal(10, stores[fuel], "ten local stock consumed");
            Press(bay, plus);
            Equal(2, ship.Fuel, "exact remaining stock usable");
            Equal(0, stores[fuel], "exact cost charged");
            Press(bay, minus);
            Equal(1, ship.Fuel, "one gauge unit unloaded");
            Equal(10, stores[fuel], "ten stock recovered");
            stores[fuel] = 49991;
            Press(bay, minus);
            Equal(1, ship.Fuel, "insufficient room leaves fuel aboard");
            Equal(49991, stores[fuel], "no overflow or fuel disposal");
            stores[fuel] = 49990;
            Press(bay, minus);
            Equal(0, ship.Fuel, "exact room permits unloading");
            Equal(50000, stores[fuel], "store fills exactly");
            ship.Fuel = 250;
            stores[fuel] = 20;
            Press(bay, plus);
            Equal(250, ship.Fuel, "gauge capacity unchanged");
            Equal(20, stores[fuel], "full tank takes no stock");
            Equal(1000, GameCore.Earth.PlanetResources.Stores[fuel], "other location conserved");
        }

        private InterStellarShip DfccAccShip(Ship_Types type)
        {
            var ship = DamageShip(type == Ship_Types.SCG);
            Save.AtWar = false;
            ship.ShipState = Ship_States.Docked;
            ship.FuelType = type == Ship_Types.SCG ? ItemTypes.hed_fuel : ItemTypes.meh_fuel;
            ship.DFCC = true;
            ship.ACC = new Deuteros.Code.Objects.ACC { Ship = ship, Source = StellarBodies.earth, Destination = StellarBodies.mars,
                SourceItems = new(), DestinationItems = new(), CurrentSource = ItemTypes.iron, CurrentDestination = ItemTypes.iron };
            Save.GameConfig.IOSRefuelThreshold = 50;
            return ship;
        }

        private void DfccAccFuel(Ship_Types type)
        {
            var ship = DfccAccShip(type);
            var stores = GameCore.Earth.Station.Resources.Stores;
            ship.Fuel = 40;
            stores[ship.FuelType] = 99;
            Equal(false, ship.ACC.Refuel(), "insufficient fuel to reach threshold waits");
            Equal(40, ship.Fuel, "waiting leaves tank intact");
            Equal(99, stores[ship.FuelType], "waiting leaves stock intact");
            stores[ship.FuelType] = 109;
            Equal(true, ship.ACC.Refuel(), "threshold plus remainder accepted");
            Equal(50, ship.Fuel, "only whole gauge units loaded");
            Equal(9, stores[ship.FuelType], "fractional stock remainder stays in stores");
            Equal(false, ship.ACC.Refuelling, "waiting state clears");
            ship.Fuel = 0;
            stores[ship.FuelType] = 2519;
            Equal(true, ship.ACC.Refuel(), "full refill accepted");
            Equal(250, ship.Fuel, "full gauge");
            Equal(19, stores[ship.FuelType], "full refill consumes 2500 stock");
            ship.ACC.Refuelling = true;
            Equal(true, ship.ACC.Refuel(), "already sufficient fuel");
            Equal(false, ship.ACC.Refuelling, "already sufficient clears stale waiting flag");
        }

        private async Task DfccFuelFitting(Ship_Types type)
        {
            var bay = await DfccFuelBay(type, false);
            var ship = (InterStellarShip)bay.Ship;
            var stores = bay.ResourceList.Stores;
            stores[ship.FuelType] = 25;
            for (var unit = 0; unit < 25; unit++) Press(bay, "Fuel/FuelGauge/Plus/RepeatingButton");
            Equal("25", bay.GetNode<Label>("Fuel/InShip").Text, "fuel display populated before conversion");
            ship.Modules[0].ModuleType = Module_Types.Tool;
            var dfcc = GameCore.SingletonInstance.GameData.GetItem(ItemTypes.d__f__c__c);
            dfcc.Research.Researched = true;
            dfcc.Locked = false;
            stores[ItemTypes.d__f__c__c] = 1;
            Press(bay, "Buttons/ShipNav/Nav_Torso1");
            Press(bay, ShipParts + "Torso1/SpriteHolder/Buttons/ActivatePod");
            stores[ship.FuelType] = 49976;
            PressEquipmentNamed(bay, dfcc.ShortName);
            Equal(false, ship.DFCC, "blocked fuel return cannot change hull mode");
            Equal(25, ship.Fuel, "blocked fitting leaves tank intact");
            Equal(1, stores[ItemTypes.d__f__c__c], "blocked fitting leaves equipment stock intact");
            Equal(ItemTypes.none, ship.Modules[0].ItemStored, "blocked fitting leaves mounting intact");
            if (OverlayManager.Instance.IsOpen) OverlayManager.Instance.CloseOverlay();
            await InputFrames();
            stores[ship.FuelType] = 49975;
            PressEquipmentNamed(bay, dfcc.ShortName);
            Equal(true, ship.DFCC, "DFCC fitted");
            Equal(0, ship.Fuel, "old tank emptied before cost changes");
            Equal(50000, stores[ship.FuelType], "old fuel returned at old one to one value");
            Equal(0, stores[ItemTypes.d__f__c__c], "one DFCC consumed");
            Equal(ItemTypes.d__f__c__c, ship.Modules[0].ItemStored, "equipment installed");
            Equal("0", bay.GetNode<Label>("Fuel/InShip").Text, "fitting refreshes tank readout immediately");
            Equal("50000", bay.GetNode<Label>("Fuel/InStock").Text, "fitting refreshes returned fuel stock immediately");
            await InputFrames();
            await CaptureDisplayEvidence("dfcc-fitted-" + type);
            Cursor.Unlock();
        }

        private async Task DfccFuelDismantle(Ship_Types type)
        {
            var bay = await DfccFuelBay(type);
            var ship = (Ship)bay.Ship;
            var stores = bay.ResourceList.Stores;
            ship.Fuel = 2;
            ship.Modules[0].ModuleType = Module_Types.Supply;
            ship.Modules[0].ItemStored = ship.FuelType;
            ship.Modules[0].ItemCount = 3;
            stores[ship.FuelType] = 49978;
            Press(bay, "Buttons/Nav_Dismantle");
            Equal(true, Save.Ships.Contains(ship), "combined tank and cargo cannot overflow");
            Equal(49978, stores[ship.FuelType], "failed dismantle is atomic");
            Equal(0, Save.News.GetNews(100).Count(n => n.Contains("Scrapped")), "failed dismantle has no success report");
            OverlayManager.Instance.CloseOverlay();
            await InputFrames();
            stores[ship.FuelType] = 49977;
            Press(bay, "Buttons/Nav_Dismantle");
            Equal(0, Save.Ships.Count, "exact combined capacity permits dismantle");
            Equal(50000, stores[ship.FuelType], "tank returns twenty but cargo remains three stock");
            Equal(1, Save.News.GetNews(100).Count(n => n.Contains(ship.Name) && n.Contains("Scrapped")), "successful dismantle reports the named ship once");
        }

        private void DfccFuelBurn()
        {
            foreach (var drones in new[] { 0, 1, 192 })
            {
                var ship = DfccAccShip(Ship_Types.IOS);
                ship.DroneCount = drones;
                ship.ACC = null;
                ship.Fuel = 100;
                ship.ShipState = Ship_States.UnDocked;
                Equal(true, ship.EngageEngine(), "DFCC ship departs");
                var before = Save.CurrentDay++;
                ShipInterior.UpdateShips(before, Save.CurrentDay);
                Equal(99, ship.Fuel, "gauge drain unchanged regardless of drones");
            }
        }

        private void DfccFuelSave()
        {
            var ship = DfccAccShip(Ship_Types.IOS);
            ship.Fuel = 40;
            var loaded = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            GameCore.SingletonInstance.GameData.ActiveSaveFile = loaded;
            ship = (InterStellarShip)loaded.Ships.Single(s => s.ShipID == ship.ShipID);
            Equal(40, ship.Fuel, "existing tank range preserved");
            loaded.BaseGameData.Planets[ship.PlanetLocation].Station.Resources.Stores[ship.FuelType] = 100;
            Equal(true, ship.ACC.Refuel(), "restored ACC can reach threshold");
            Equal(50, ship.Fuel, "new refuelling uses ten stock per unit");
        }

        private async Task DfccNormalFuel()
        {
            foreach (var type in new[] { Ship_Types.Shuttle, Ship_Types.IOS })
            {
                var bay = await EmptyAssemblyBay(type);
                var stores = bay.ResourceList.Stores;
                stores[AssemblyChassis(type)] = 1;
                Press(bay, "Buttons/Nav_Create_" + type);
                stores[bay.Ship.FuelType] = 1;
                Press(bay, "Fuel/FuelGauge/Plus/RepeatingButton");
                Equal(1, bay.Ship.Fuel, "normal hull loads one to one");
                Equal(0, stores[bay.Ship.FuelType], "normal fuel cost unchanged");
                Press(bay, "Fuel/FuelGauge/Minus/RepeatingButton");
                Equal(0, bay.Ship.Fuel, "normal hull unloads one to one");
                Equal(1, stores[bay.Ship.FuelType], "normal fuel return unchanged");
            }
        }
    }
}
