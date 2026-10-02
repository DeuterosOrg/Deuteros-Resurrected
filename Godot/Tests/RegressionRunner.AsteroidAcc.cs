using System.Linq;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Utility;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private void RunAsteroidAccRegressions()
        {
            CheckUi("Grapple-only engaged ACC stops at a scan without moving cargo or clearing settings", AsteroidAccDisengages);
            CheckUi("Grapple-only ACC waits for a scan and preserves non-engaged modes", AsteroidAccWaits);
            CheckUi("AMA-equipped ACC still mines selected large asteroids", AsteroidAccMines);
            CheckUi("Daily asteroid scanning disengages grapple-only ACC", AsteroidAccDayTick);
            CheckUi("Disengaged asteroid ACC survives saving and permits manual grapple capture", AsteroidAccSaveAndCapture);
        }

        private IOS AsteroidAccShip()
        {
            var ship = UnknownObjectShip();
            Save.Ships.Clear();
            Save.Ships.Add(ship);
            ship.PlanetLocation = StellarBodies.asteroids;
            ship.DestinationPlanetLocation = StellarBodies.earth;
            ship.StarLocation = ship.DestinationStarLocation = StellarBodies.the_sun;
            ship.Engine = true;
            ship.Fuel = 100;
            ship.Modules[1].ModuleType = Module_Types.Supply;
            ship.Modules[1].ItemStored = ItemTypes.iron;
            ship.Modules[1].ItemCount = 12;
            ship.ACC = new Deuteros.Code.Objects.ACC
            {
                Ship = ship, Source = StellarBodies.earth, Destination = StellarBodies.asteroids,
                SourceItems = new() { ItemTypes.iron }, DestinationItems = new() { ItemTypes.titanium },
                CurrentSource = ItemTypes.iron, CurrentDestination = ItemTypes.titanium
            };
            ship.ACC.Activate();
            Equal(true, ship.ACC.Active, "valid asteroid ACC engages");
            return ship;
        }

        private static Asteroid SmallAccAsteroid() => new Asteroid
        {
            Type = ItemTypes.silver, Class = 2, Mass = 100, MassName = "Small"
        };

        private void AsteroidAccDisengages()
        {
            var ship = AsteroidAccShip();
            // Unselected and too small for AMA: the original stop precedes both filters.
            var asteroid = SmallAccAsteroid();
            ship.ItemScanResults = asteroid;
            var stores = GameCore.Earth.Station.Resources.Stores;
            stores[ItemTypes.iron] = 123;
            ship.ACC.Update(Ship_States.UnDocked);
            Equal(false, ship.ACC.Active, "scan without AMA disengages ACC");
            Equal(false, ship.ACC.CycleMode, "no finishing cycle starts");
            Equal(false, ship.ACC.Refuelling, "no refuelling wait starts");
            Equal(Ship_States.UnDocked, ship.ShipState, "ship remains at asteroids");
            Equal(false, ship.EngineEngaged, "no automatic departure");
            Equal(100, ship.Fuel, "fuel unchanged");
            Equal(12, ship.Modules[1].ItemCount, "cargo unchanged");
            Equal(123, stores[ItemTypes.iron], "no stores credit");
            Equal(asteroid, ship.ItemScanResults, "scan remains available for the player");
            Equal(true, ship.Modules.All(m => m.HeldItem == null), "nothing captured automatically");
            Equal(ItemTypes.titanium, ship.ACC.DestinationItems.Single(), "filter retained");
            Equal(ItemTypes.iron, ship.ACC.SourceItems.Single(), "source filter retained");
            Equal(ItemTypes.titanium, ship.ACC.CurrentDestination, "cursor retained");
        }

        private void AsteroidAccWaits()
        {
            var ship = AsteroidAccShip();
            ship.ACC.Update(Ship_States.UnDocked);
            Equal(true, ship.ACC.Active, "no scan does not stop ACC");
            ship.ItemScanResults = SmallAccAsteroid();
            ship.ACC.Active = false;
            ship.ACC.CycleMode = true;
            ship.ACC.Update(Ship_States.UnDocked);
            Equal(true, ship.ACC.CycleMode, "finishing mode is not the original engage branch");
            ship.ACC.Active = true;
            ship.ACC.Update(Ship_States.UnDocked);
            Equal(true, ship.ACC.CycleMode, "current UI cycle flag combination is left for cycle investigation");
            Equal(true, ship.ACC.Active, "cycle request is not treated as normal engage");
            ship.ACC.CycleMode = ship.ACC.Active = false;
            ship.ACC.Update(Ship_States.UnDocked);
            Equal(false, ship.ACC.Active, "manual mode remains disengaged");
            ship.PlanetLocation = StellarBodies.earth;
            ship.ACC.Active = true;
            ship.ACC.Update(Ship_States.UnDocked);
            Equal(true, ship.ACC.Active, "non-asteroid orbit does not use the asteroid stop rule");
        }

        private void AsteroidAccMines()
        {
            var ship = AsteroidAccShip();
            ship.Modules[0].ItemStored = ItemTypes.a__m__a;
            ship.Modules[1].ItemStored = ItemTypes.none;
            ship.Modules[1].ItemCount = 0;
            ship.ItemScanResults = new Asteroid { Type = ItemTypes.titanium, Class = 6, Mass = 10000, MassName = "Large" };
            ship.ACC.Update(Ship_States.UnDocked);
            Equal(true, ship.ACC.Active, "AMA retains automation");
            Equal(Ship_States.Docking, ship.ShipState, "selected large asteroid starts mining approach");
        }

        private void AsteroidAccDayTick()
        {
            var ship = AsteroidAccShip();
            // An existing scan guarantees a result, whether the scanner retains or replaces it.
            ship.ItemScanResults = SmallAccAsteroid();
            var before = Save.CurrentDay++;
            ShipInterior.UpdateShips(before, Save.CurrentDay);
            Equal(false, ship.ACC.Active, "real daily scanner reaches disengage path");
            Equal(Ship_States.UnDocked, ship.ShipState, "no automatic docking or return");
            Equal(true, ship.ItemScanResults is Asteroid, "scanner result retained");
        }

        private void AsteroidAccSaveAndCapture()
        {
            var ship = AsteroidAccShip();
            ship.ItemScanResults = SmallAccAsteroid();
            ship.ACC.Update(Ship_States.UnDocked);
            var restored = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            GameCore.SingletonInstance.GameData.ActiveSaveFile = restored;
            ship = (IOS)restored.Ships.Single();
            Equal(false, ship.ACC.Active, "disengagement survives save/load");
            CaptureScannedObject(ship);
            Equal(100, ((Asteroid)ship.Modules[0].HeldItem).Mass, "manual grapple still captures the scan");
            Equal(null, ship.ItemScanResults, "manual capture consumes the scan");
        }
    }
}
