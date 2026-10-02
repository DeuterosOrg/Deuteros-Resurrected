using System.Linq;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Screens;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private void RunAmaCargoRegressions()
        {
            CheckUi("AMA leaves incompatible cargo intact and launches when no pod accepts ore", AmaIncompatibleCargo);
            CheckUi("ACC returns from a selected asteroid when no pod accepts its mineral", AmaAccIncompatibleCargo);
            CheckUi("AMA treats zero-count supply pods as empty regardless of their previous mineral", AmaEmptyCargo);
            CheckUi("AMA skips incompatible and full pods and caps the first usable pod", AmaCompatibleCargo);
        }

        private IOS AmaCargoShip()
        {
            var ship = AsteroidAccShip();
            ship.ACC.Active = false;
            ship.ShipState = Ship_States.Docked;
            Save.CurrentDay = 100;
            ship.Modules[0].ItemStored = ItemTypes.a__m__a;
            ship.Modules[0].LastMinedDay = 95;
            ship.Modules[1].ItemStored = ItemTypes.iron;
            ship.Modules[1].ItemCount = 12;
            ship.Modules[2].ModuleType = Module_Types.Supply;
            ship.Modules[2].ItemStored = ItemTypes.copper;
            ship.Modules[2].ItemCount = 20;
            ship.ItemScanResults = new Asteroid { Type = ItemTypes.titanium, Class = 6, Mass = 10000, MassName = "Large" };
            return ship;
        }

        private void AmaIncompatibleCargo()
        {
            var ship = AmaCargoShip();
            ShipInterior.UpdateShips(99, 100);
            Equal(Ship_States.Launching, ship.ShipState, "no compatible cargo capacity leaves asteroid");
            Equal(12, ship.Modules[1].ItemCount, "iron retained");
            Equal(ItemTypes.iron, ship.Modules[1].ItemStored, "iron not overwritten");
            Equal(20, ship.Modules[2].ItemCount, "copper retained");
            Equal(ItemTypes.copper, ship.Modules[2].ItemStored, "copper not overwritten");
            Equal(true, ship.AsteroidScanResults.HasBeenMined, "same asteroid not immediately remined");
            Equal((uint)0, ship.Modules[0].LastMinedDay, "mining cycle released");
        }

        private void AmaAccIncompatibleCargo()
        {
            var ship = AmaCargoShip();
            ship.ShipState = Ship_States.UnDocked;
            ship.ACC.Active = true;
            ship.ACC.Update(Ship_States.UnDocked);
            Equal(Ship_States.InTransit, ship.ShipState, "selected mineral without usable pod returns home");
            Equal(StellarBodies.earth, ship.DestinationPlanetLocation, "return destination preserved");
            Equal(12, ship.Modules[1].ItemCount, "ACC preserves iron");
            Equal(20, ship.Modules[2].ItemCount, "ACC preserves copper");
            Equal(true, ship.ACC.Active, "automation continues return route");
        }

        private void AmaEmptyCargo()
        {
            var ship = AmaCargoShip();
            ship.Modules[1].ItemCount = 0;
            ShipInterior.UpdateShips(99, 100);
            Equal(Ship_States.Docked, ship.ShipState, "empty pod allows mining");
            Equal(ItemTypes.titanium, ship.Modules[1].ItemStored, "empty pod takes scanned mineral");
            Equal(true, ship.Modules[1].ItemCount > 0, "ore deposited");
            Equal(20, ship.Modules[2].ItemCount, "other cargo retained");
        }

        private void AmaCompatibleCargo()
        {
            var ship = AmaCargoShip();
            ship.Modules[2].ItemStored = ItemTypes.titanium;
            ship.Modules[2].ItemCount = 245;
            ShipInterior.UpdateShips(99, 100);
            Equal(12, ship.Modules[1].ItemCount, "incompatible earlier pod skipped");
            Equal(250, ship.Modules[2].ItemCount, "matching pod capped at capacity");
            Save.CurrentDay = 105;
            ShipInterior.UpdateShips(104, 105);
            Equal(Ship_States.Launching, ship.ShipState, "full matching pod plus incompatible partial pod ends mining");
            Equal(250, ship.Modules[2].ItemCount, "full pod not overwritten");
            Equal(12, ship.Modules[1].ItemCount, "unrelated partial pod still retained");
        }
    }
}
