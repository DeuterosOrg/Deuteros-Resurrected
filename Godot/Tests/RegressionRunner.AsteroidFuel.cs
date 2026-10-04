using System;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Godot;
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
        private void AsteroidFuelTick(uint day)
        {
            Save.CurrentDay = day;
            ShipInterior.UpdateShips(day - 1, day);
        }

        private void AsteroidFuelCadence()
        {
            GameCore.SingletonInstance.SetProcess(false);
            var miner = AmaCargoShip();
            miner.Fuel = 4;
            var scanner = NavigationShip();
            scanner.PlanetLocation = StellarBodies.asteroids;
            scanner.ShipState = Ship_States.UnDocked;
            scanner.Pilot = null;
            scanner.Fuel = 4;
            Save.Ships.Add(scanner);
            var scg = new SCG { ShipType = Ship_Types.SCG, ShipState = Ship_States.UnDocked,
                StarLocation = StellarBodies.the_sun, PlanetLocation = StellarBodies.asteroids,
                Modules = new(), Fuel = 4 };
            Save.Ships.Add(scg);
            var docked = NavigationShip();
            docked.ShipState = Ship_States.Docked;
            docked.Fuel = 4;
            Save.Ships.Add(docked);
            var ids = new[] { miner.ShipID, scanner.ShipID, scg.ShipID };
            var dockedId = docked.ShipID;
            Save.CurrentDay = 0;
            for (uint day = 1; day <= 638; day++)
            {
                miner = (IOS)Save.Ships.Single(s => s.ShipID == ids[0]);
                miner.Modules[1].ItemCount = 0; // Retain mining instead of departing with a full hold.
                AsteroidFuelTick(day);
                var expected = day < 128 ? 4 : day < 383 ? 3 : day < 638 ? 2 : 1;
                foreach (var id in ids)
                    Equal(expected, Save.Ships.Single(s => s.ShipID == id).Fuel, "shared original fuel cadence at update " + day);
                Equal(4, Save.Ships.Single(s => s.ShipID == dockedId).Fuel, "station-docked hull does not burn asteroid fuel");
                if (day == 127 || day == 382)
                    GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            }
            ShipInterior.UpdateShips(638, 638);
            Equal(1, Save.Ships.Single(s => s.ShipID == ids[0]).Fuel, "nonadvancing notification cannot burn another unit");
        }

        private void AsteroidFuelExhaustion()
        {
            GameCore.SingletonInstance.SetProcess(false);
            foreach (var mining in new[] { false, true })
            foreach (var finishing in new[] { false, true })
            {
                var ship = AmaCargoShip();
                ship.Modules[1].ItemCount = 0;
                ship.ShipState = mining ? Ship_States.Docked : Ship_States.UnDocked;
                ship.Pilot = null;
                ship.Fuel = 1;
                ship.EngineEngaged = true;
                ship.ACC.Active = !finishing;
                ship.ACC.CycleMode = finishing;
                ship.ACC.Refuelling = true;
                AsteroidFuelTick(128);
                Equal(0, ship.Fuel, "scan or mining spends its last scheduled unit");
                Equal(false, ship.ACC.Active || ship.ACC.CycleMode || ship.ACC.Refuelling, "both ACC modes stop on exhaustion");
                Equal(false, ship.EngineEngaged, "exhaustion clears engine feedback");
                Equal(true, ship.ACC.SourceItems.Count + ship.ACC.DestinationItems.Count > 0, "stopping preserves cargo selection");
                for (uint day = 129; day <= 136; day++)
                {
                    ship.Modules[1].ItemCount = 0;
                    AsteroidFuelTick(day);
                }
                Equal(true, Save.Ships.Contains(ship), "existing scanner or miner does not take generic orbit loss");
                Equal(mining ? Ship_States.Docked : Ship_States.UnDocked, ship.ShipState, "zero-fuel activity retains its original state");
                if (mining) Equal(true, ship.Modules[1].ItemCount > 0, "already active mining still dispatches after exhaustion");
            }
        }

        private void AsteroidEmptyApproach()
        {
            GameCore.SingletonInstance.SetProcess(false);
            var ship = AmaCargoShip();
            ship.Modules[1].ItemCount = 0;
            ship.ShipState = Ship_States.UnDocked;
            ship.Fuel = 1;
            ship.Dock();
            AsteroidFuelTick(101);
            Equal(1, ship.Fuel, "odd shared phase does not charge approach fuel");
            Equal(Ship_States.Docking, ship.ShipState, "first approach update stays in progress");
            AsteroidFuelTick(102);
            Equal(0, ship.Fuel, "even shared phase charges approach");
            Equal(Ship_States.Docking, ship.ShipState, "empty tank cannot enter mining");
            var id = ship.ShipID;
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            ship = (IOS)Save.Ships.Single(s => s.ShipID == id);
            for (uint day = 103; day <= 110; day++) AsteroidFuelTick(day);
            Equal(Ship_States.Docking, ship.ShipState, "exhausted approach remains incomplete after reload");
            Equal(0, ship.Modules[1].ItemCount, "failed approach cannot mine free ore");
            Equal(true, Save.Ships.Contains(ship), "approach exhaustion is not scanning-arrival stranding");
        }

        private void AsteroidEmptyLaunch()
        {
            GameCore.SingletonInstance.SetProcess(false);
            var ship = AmaCargoShip();
            ship.Fuel = 1;
            ship.TakeOff();
            AsteroidFuelTick(101);
            Equal(0, ship.Fuel, "launch spends its last unit immediately");
            var id = ship.ShipID;
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            ship = (IOS)Save.Ships.Single(s => s.ShipID == id);
            AsteroidFuelTick(102);
            Equal(Ship_States.Launching, ship.ShipState, "fuel loss doubles pending two-update departure");
            AsteroidFuelTick(103);
            Equal(Ship_States.Launching, ship.ShipState, "third update still has one launch update remaining");
            AsteroidFuelTick(104);
            Equal(Ship_States.UnDocked, ship.ShipState, "fourth update reaches stranded scanning arrival");
            var scan = ship.ItemScanResults;
            for (uint day = 105; day <= 109; day++)
            {
                AsteroidFuelTick(day);
                Equal(true, Save.Ships.Contains(ship), "stranded hull retains all six grace updates");
                Equal(true, ReferenceEquals(scan, ship.ItemScanResults), "stranded arrival cannot scan or automate");
            }
            AsteroidFuelTick(110);
            Equal(false, Save.Ships.Contains(ship), "sixth post-arrival update destroys stranded hull");
        }

        private async Task AsteroidFuelStatus()
        {
            var interior = await OpenInterior(Ship_Types.IOS);
            var core = GameCore.SingletonInstance;
            core.SetProcess(false);
            var ship = (IOS)interior.Ship;
            ship.PlanetLocation = StellarBodies.asteroids;
            ship.ShipState = Ship_States.UnDocked;
            ship.Pilot = null;
            ship.Fuel = 1;
            Save.CurrentDay = 127;
            core._Process(GameClock.NormalIntervalSeconds);
            Equal(0, ship.Fuel, "natural fractional consumption charges the saved update phase");
            interior.UpdateState();
            Equal(true, interior.GetNode<Label>("TextLayout/Status").Text.StartsWith("Scanning"), "empty scanning must not claim the hull is falling");
            await CaptureDisplayEvidence("asteroid-empty-scanning");
            ship.PlanetLocation = StellarBodies.earth;
            ship.DestinationPlanetLocation = StellarBodies.asteroids;
            ship.DestinationStarLocation = StellarBodies.the_sun;
            ship.Fuel = 100;
            Equal(true, ship.EngageEngine(), "ordinary trip starts through shared engine control");
            ship.Fuel = ship.TravelTimeRemain();
            while (ship.ShipState == Ship_States.InTransit) AsteroidFuelTick(Save.CurrentDay + 1);
            Equal(0, ship.Fuel, "last transit fuel is consumed at arrival");
            interior.UpdateState();
            Equal(true, interior.GetNode<Label>("TextLayout/Status").Text.StartsWith("Stranded At"), "empty arrival warns of the loss countdown");
            await CaptureDisplayEvidence("asteroid-empty-arrival");
        }

        private void AsteroidFuelSavesAndArrival()
        {
            GameCore.SingletonInstance.SetProcess(false);
            var ship = AmaCargoShip();
            ship.Modules[1].ItemCount = 0;
            ship.ShipState = Ship_States.UnDocked;
            ship.Dock();
            AsteroidFuelTick(101);
            var saved = SaveStorage.Serialize(Save);
            var active = Save;
            foreach (var invalid in new JToken[] { JValue.CreateNull(), new JValue(-2), new JValue(5) })
            {
                var bad = JObject.Parse(saved);
                ((JObject)bad["Game"]["Ships"].First())["AsteroidActionTicks"] = invalid;
                var rejected = false;
                try { GameCore.SingletonInstance.LoadSavedGame(SaveStorage.Deserialize(bad.ToString())); }
                catch (Exception error) when (error is InvalidDataException || error is Newtonsoft.Json.JsonException) { rejected = true; }
                Equal(true, rejected, "malformed asteroid countdown rejected before replacing world");
                Equal(true, ReferenceEquals(active, Save), "failed load preserves the active world");
            }
            var legacy = JObject.Parse(saved);
            ((JObject)legacy["Game"]["Ships"].First()).Property("AsteroidActionTicks").Remove();
            foreach (var loadedDay in new[] { 101u, 103u })
            {
                legacy["Game"]["CurrentDay"] = loadedDay;
                GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(legacy.ToString());
                ship = (IOS)Save.Ships.Single();
                AsteroidFuelTick(loadedDay + 1);
                Equal(Ship_States.Docked, ship.ShipState, "legacy mid-approach or overdue save completes on its next update");
            }

            ship.ShipState = Ship_States.UnDocked;
            ship.PlanetLocation = StellarBodies.earth;
            ship.DestinationPlanetLocation = StellarBodies.asteroids;
            ship.Fuel = 100;
            Equal(true, ship.EngageEngine(), "normal asteroid-bound route starts");
            ship.Fuel = ship.TravelTimeRemain();
            while (ship.ShipState == Ship_States.InTransit) AsteroidFuelTick(Save.CurrentDay + 1);
            Equal(0, ship.Fuel, "arrival uses the final unit");
            var arrival = Save.CurrentDay;
            AsteroidFuelTick(arrival + 1);
            AsteroidFuelTick(arrival + 2);
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            ship = (IOS)Save.Ships.Single();
            for (var day = arrival + 3; day <= arrival + 5; day++) AsteroidFuelTick(day);
            Equal(true, Save.Ships.Contains(ship), "reloaded arrival retains its fifth grace update");
            AsteroidFuelTick(arrival + 6);
            Equal(false, Save.Ships.Contains(ship), "reloaded arrival expires on its sixth update");
        }
    }
}
