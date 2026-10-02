using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Platform.Screens;
using Godot;
using static Deuteros.Code.Enums;
using AccController = Deuteros.Code.Objects.ACC;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task RunHullTravelRegressions()
        {
            CheckUi("IOS cannot engage an interstellar course", () => HullEngine(false, false));
            CheckUi("Spoofed star metadata cannot bypass the IOS hull restriction", () => HullEngine(false, true));
            CheckUi("IOS local travel canonicalizes destination metadata", HullLocalTravel);
            CheckUi("SCG can engage an interstellar course with canonical stars", () => HullEngine(true, true));
            CheckUi("Unknown destination cannot start a ship journey", HullUnknownDestination);
            foreach (var returning in new[] { false, true })
                await CheckAsync($"IOS course rejection preserves ACC endpoints returning={returning}", () => HullCourse(false, returning));
            await CheckAsync("SCG course selection accepts another star", () => HullCourse(true, false));
            CheckUi("Invalid IOS ACC route cannot load cargo fuel or launch", HullAccActivation);
            CheckUi("Previously active invalid IOS ACC stops before resupply", HullAccResume);
            await CheckAsync("ACC Cycle control cannot reactivate an invalid IOS route", HullAccCycle);
            CheckUi("SCG completes outbound and return travel with consistent star metadata", HullRoundTrip);
            CheckUi("SCG arrival resolves when equal orbital indexes produce an elapsed deadline", HullElapsedArrival);
        }

        private Ship HullShip(bool scg)
        {
            Ship ship = scg ? new SCG { ShipType = Ship_Types.SCG } : NavigationShip();
            ship.PlanetLocation = StellarBodies.earth;
            ship.StarLocation = StellarBodies.the_sun;
            ship.DestinationPlanetLocation = StellarBodies.atlantic;
            ship.DestinationStarLocation = StellarBodies.proxima;
            ship.ShipState = Ship_States.UnDocked;
            ship.Engine = true;
            ship.Fuel = 100;
            ship.Pilot = new Staff { Leader = "Route", Type = StaffType.Marines, Count = 10 };
            ship.Pilot.AddAction(9);
            ship.Modules = new List<ShipModule> { new ShipModule { ModuleType = Module_Types.Supply, ItemStored = ItemTypes.iron, ItemCount = 12 } };
            Save.Ships.Clear();
            Save.Ships.Add(ship);
            Save.CurrentDay = 100;
            return ship;
        }

        private void HullEngine(bool scg, bool spoof)
        {
            var ship = HullShip(scg);
            if (spoof)
            {
                ship.StarLocation = StellarBodies.proxima;
                ship.DestinationStarLocation = StellarBodies.the_sun;
            }
            Equal(scg, ship.EngageEngine(), "only SCG departs for another star");
            Equal(scg ? Ship_States.InTransit : Ship_States.UnDocked, ship.ShipState, "rejection does not start transit");
            Equal(scg, ship.EngineEngaged, "engine follows acceptance");
            Equal(scg ? (uint)100 : 0, ship.StartTravelDay, "rejection preserves timestamp");
            Equal(scg ? (int)StaffLevel_Marines.Captain : (int)StaffLevel_Marines.Pilot, ship.Pilot.GetLevel(), "rejection does not award a pilot action");
            Equal(100, ship.Fuel, "engaging or rejecting does not consume fuel immediately");
            if (scg)
            {
                Equal(StellarBodies.the_sun, ship.StarLocation, "origin derived from actual body");
                Equal(StellarBodies.proxima, ship.DestinationStarLocation, "destination derived from actual body");
            }
        }

        private void HullLocalTravel()
        {
            var ship = HullShip(false);
            ship.DestinationPlanetLocation = StellarBodies.the_moon;
            ship.DestinationStarLocation = StellarBodies.proxima;
            Equal(true, ship.EngageEngine(), "local moon travel remains available");
            Equal(StellarBodies.the_sun, ship.DestinationStarLocation, "stale destination star corrected");
        }

        private void HullUnknownDestination()
        {
            var ship = HullShip(true);
            ship.DestinationPlanetLocation = StellarBodies.none;
            Equal(false, ship.EngageEngine(), "unknown body is rejected");
            Equal(Ship_States.UnDocked, ship.ShipState, "no invalid transit state");
        }

        private async Task HullCourse(bool scg, bool returning)
        {
            var interior = await OpenInterior(scg ? Ship_Types.SCG : Ship_Types.IOS);
            var ship = interior.Ship;
            ship.DestinationPlanetLocation = StellarBodies.mars;
            ship.DestinationStarLocation = StellarBodies.the_sun;
            ship.ACC = new AccController { Ship = ship, Source = returning ? StellarBodies.mars : StellarBodies.earth,
                Destination = returning ? StellarBodies.earth : StellarBodies.mars };
            Save.Unlocks.Add(Game_Unlocks.Interstellar_Travel);
            var source = ship.ACC.Source;
            var destination = ship.ACC.Destination;
            try
            {
                Press(interior, "SetCourse");
                var map = interior.GetNode("StarMap").GetChildren().OfType<StarMap>().Single();
                map.LoadMap(StellarBodies.atlantic);
                using var click = new InputEventMouseButton { ButtonIndex = MouseButton.Right, Pressed = true };
                interior._Input(click);
                await InputFrames();
                Equal(scg ? StellarBodies.atlantic : StellarBodies.mars, ship.DestinationPlanetLocation, "course selection honors hull");
                Equal(scg ? StellarBodies.proxima : StellarBodies.the_sun, ship.DestinationStarLocation, "destination metadata agrees");
                Equal(source, ship.ACC.Source, "invalid choice preserves source endpoint");
                Equal(scg ? StellarBodies.atlantic : destination, ship.ACC.Destination, "invalid choice preserves destination endpoint");
                Equal(false, Cursor.IsLocked, "course window releases cursor");
                Equal(0, interior.GetNode("StarMap").GetChildCount(), "course window closes");
                Equal(!scg, OverlayManager.Instance.IsOpen, "IOS rejection gives feedback");
                if (!scg)
                {
                    var message = OverlayManager.Instance.GetNode("GlobalOverlay/Center").GetChild(0).GetNode<Label>("ErrorButton/OuterColorRect/InnerColorRect/ErrorLabel");
                    Equal(true, message.Text.Contains("SCG"), "restriction identifies required hull");
                    await CaptureDisplayEvidence("ios-interstellar-course-rejected");
                }
            }
            finally { OverlayManager.Instance.CloseOverlay(); await InputFrames(); }
        }

        private Ship HullAccShip()
        {
            var ship = HullShip(false);
            ship.ShipState = Ship_States.Docked;
            ship.FuelType = ItemTypes.meh_fuel;
            ship.Fuel = 0;
            ship.ACC = new AccController { Ship = ship, Source = StellarBodies.earth, Destination = StellarBodies.atlantic,
                SourceItems = new List<ItemTypes> { ItemTypes.iron }, DestinationItems = new List<ItemTypes>(),
                CurrentSource = ItemTypes.iron, CurrentDestination = ItemTypes.iron };
            GameCore.Earth.Station.Resources.Stores[ItemTypes.meh_fuel] = 250;
            GameCore.Earth.Station.Resources.Stores[ItemTypes.iron] = 50;
            return ship;
        }

        private void HullAccUnchanged(Ship ship)
        {
            Equal(false, ship.ACC.Active, "invalid ACC is inactive");
            Equal(false, ship.ACC.CycleMode, "invalid ACC cannot cycle");
            Equal(false, ship.ACC.Refuelling, "invalid ACC is not waiting to resume refuelling");
            Equal(Ship_States.Docked, ship.ShipState, "invalid ACC stays docked");
            Equal(0, ship.Fuel, "invalid route takes no fuel");
            Equal(250, GameCore.Earth.Station.Resources.Stores[ItemTypes.meh_fuel], "station fuel conserved");
            Equal(50, GameCore.Earth.Station.Resources.Stores[ItemTypes.iron], "station cargo conserved");
            Equal(12, ship.Modules[0].ItemCount, "aboard cargo conserved");
        }

        private void HullAccActivation()
        {
            var ship = HullAccShip();
            ship.ACC.Activate();
            HullAccUnchanged(ship);
        }

        private void HullAccResume()
        {
            var ship = HullAccShip();
            ship.ACC.Active = ship.ACC.CycleMode = ship.ACC.Refuelling = true;
            ship.ACC.Update(Ship_States.Docked);
            HullAccUnchanged(ship);
        }

        private async Task HullAccCycle()
        {
            InitializeUi();
            var ship = HullAccShip();
            var core = GameCore.SingletonInstance;
            core.ShipSelected = ship.ShipID;
            core.ChangeScene(Scenes.ShipInterior, new List<SceneVariables>());
            await InputFrames();
            var interior = ActiveScreen<ShipInterior>();
            Press(interior, "OpenACC");
            var panel = interior.GetNode("ACCScreen").GetChildren().OfType<Deuteros.Code.Platform.Screens.ACC>().Single();
            panel.GetNode<TextureButton>("Window/Buttons/Cycle").EmitSignal(BaseButton.SignalName.Pressed);
            HullAccUnchanged(ship);
            Cursor.Unlock();
        }

        private void HullRoundTrip()
        {
            var ship = HullShip(true);
            foreach (var destination in new[] { StellarBodies.atlantic, StellarBodies.earth })
            {
                Equal(destination, ship.DestinationPlanetLocation, "return leg retained");
                Equal(true, ship.EngageEngine(), "SCG departs");
                for (var day = 0; day < 100 && ship.ShipState == Ship_States.InTransit; day++)
                {
                    var before = Save.CurrentDay++;
                    ShipInterior.UpdateShips(before, Save.CurrentDay);
                }
                Equal(Ship_States.UnDocked, ship.ShipState, "SCG arrives within current bounded route duration");
                Equal(destination, ship.PlanetLocation, "SCG reaches selected body");
                Equal(Save.BaseGameData.Planets[destination].ParentStar, ship.StarLocation, "arrival body and star agree");
            }
        }

        private void HullElapsedArrival()
        {
            var ship = HullShip(true);
            ship.DestinationPlanetLocation = StellarBodies.cerberus;
            ship.DestinationStarLocation = StellarBodies.centauri;
            Equal(Save.BaseGameData.Planets[StellarBodies.earth].Order,
                Save.BaseGameData.Planets[StellarBodies.cerberus].Order, "different stars have matching local orbital indexes");
            Equal(true, ship.EngageEngine(), "SCG starts valid route");
            var before = Save.CurrentDay++;
            ShipInterior.UpdateShips(before, Save.CurrentDay);
            Equal(Ship_States.UnDocked, ship.ShipState, "elapsed deadline cannot strand SCG in transit");
            Equal(StellarBodies.cerberus, ship.PlanetLocation, "elapsed route reaches destination");
            Equal(StellarBodies.centauri, ship.StarLocation, "arrival star agrees with body");
        }
    }
}
