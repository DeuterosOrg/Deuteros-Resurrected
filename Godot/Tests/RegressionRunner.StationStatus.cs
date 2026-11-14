using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Utility;
using Deuteros.Code.Platform.Screens;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task RunStationStatusRegressions()
        {
            await CheckAsync("Station readouts show local product shuttle and deployed derricks", StationLocalStatus);
            await CheckAsync("Station product readout follows production completion on the same day", StationProductionCompletion);
            await CheckAsync("Station shuttle readout follows real takeoff landing and removal", StationShuttleStatus);
            foreach (var ground in new[] { true, false })
                await CheckAsync($"Production AOC panel follows local automation mode on ground={ground}", () => AocPanelMode(ground));
            CheckUi("Completing an AOC replaces staff readouts without reopening Production", AocPanelCompletion);
            await CheckAsync("Deposit analysis shows the selected body's station and clears it for absent stations", DepositStationSelection);
            CheckUi("Course selection does not display the deposit-analysis station graphic", CourseMapHasNoDepositStation);
            CheckUi("Deposit station graphic follows construction completion and loss", DepositStationUpdates);
        }

        private async Task<Node> OpenStation(StellarBodies planet)
        {
            InitializeUi();
            Save.CurrentPlanet = planet;
            Save.BaseGameData.Planets[planet].Station.Built = true;
            Save.BaseGameData.Planets[planet].Station.BuildParts = 8;
            GameCore.SingletonInstance.ChangeScene(Scenes.Station, new List<SceneVariables> { SceneVariables.Orbit });
            await InputFrames();
            return ActiveScreen<Node>();
        }

        private async Task StationLocalStatus()
        {
            Save.Ships.Clear();
            GameCore.Earth.PlanetResources.Derricks = 9;
            var moon = Save.BaseGameData.Planets[StellarBodies.the_moon];
            moon.PlanetResources.Derricks = 3;
            var screen = await OpenStation(StellarBodies.the_moon);
            Equal("Product\nNone", screen.GetNode<Label>("Labels/Product").Text, "idle local factory");
            Equal("Shuttle\nNone", screen.GetNode<Label>("Labels/Shuttle").Text, "no local shuttle");
            Equal("3 Derricks\nDeployed", screen.GetNode<Label>("Labels/Derricks").Text, "local deployed count, not Earth or stores");
            await CaptureDisplayEvidence("station-status");
            moon.PlanetResources.Stores[ItemTypes.derrick] = 100;
            moon.PlanetResources.Derricks = 0;
            AdvanceTickDay();
            Equal("0 Derricks\nDeployed", screen.GetNode<Label>("Labels/Derricks").Text, "stored derricks are not deployed");
            var restored = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            Equal(0, restored.BaseGameData.Planets[StellarBodies.the_moon].PlanetResources.Derricks, "display does not mutate save state");
        }

        private async Task StationProductionCompletion()
        {
            var earth = GameCore.Earth;
            var item = GameCore.SingletonInstance.GameData.GetItem(ItemTypes.derrick);
            earth.Factory.Builder = new Staff { Type = StaffType.Production, Count = 1, Leader = "Ground" };
            earth.Factory.ProductionQueue.Add(new ProductionItem(GameCore.SingletonInstance.GameData.GetItem(ItemTypes.s_drive)) { Active = true });
            earth.Station.Factory.AOC = true;
            earth.Station.Factory.ProductionQueue.Add(new ProductionItem(item)
            {
                Active = true, Production_Complete = 3, Production_Value = 255
            });
            var screen = await OpenStation(StellarBodies.earth);
            Equal("Product\n" + item.ShortName, screen.GetNode<Label>("Labels/Product").Text, "orbital product instead of ground recipe");
            AdvanceTickDay();
            Equal("Product\nNone", screen.GetNode<Label>("Labels/Product").Text, "completion visible after simulation in the same tick");
            Equal(1, earth.Station.Resources.Stores[ItemTypes.derrick], "actual production completed");
        }

        private async Task StationShuttleStatus()
        {
            Save.Ships.Clear();
            var ship = new Shuttle
            {
                ShipType = Ship_Types.Shuttle, PlanetLocation = StellarBodies.earth,
                StarLocation = StellarBodies.the_sun, ShipState = Ship_States.Docked,
                OnGround = true, Engine = true, Fuel = 100,
                Modules = new List<ShipModule> { new ShipModule() }
            };
            Save.Ships.Add(ship);
            var screen = await OpenStation(StellarBodies.earth);
            var label = screen.GetNode<Label>("Labels/Shuttle");
            Equal("Shuttle\nIdle", label.Text, "docked shuttle idle");
            ship.TakeOff();
            AdvanceTickDay();
            Equal("Shuttle\nTakeoff", label.Text, "actual ascent shown");
            await CaptureDisplayEvidence("station-shuttle-ascent");
            for (int i = 0; i < 4; i++) AdvanceTickDay();
            Equal(Ship_States.UnDocked, ship.ShipState, "takeoff completed");
            Equal("Shuttle\nIdle", label.Text, "idle in orbit");
            ship.Land();
            AdvanceTickDay();
            Equal("Shuttle\nLanding", label.Text, "actual descent shown");
            AdvanceTickDay();
            Equal("Shuttle\nIdle", label.Text, "landed idle");
            Save.Ships.Remove(ship);
            AdvanceTickDay();
            Equal("Shuttle\nNone", label.Text, "removed shuttle no longer displayed");
        }

        private Production OpenPanelProduction(bool ground)
        {
            var panel = GD.Load<PackedScene>("res://Screens/Production.tscn").Instantiate<Production>();
            panel.SceneVariables = new List<SceneVariables> { ground ? SceneVariables.Ground : SceneVariables.Orbit };
            // Background playback has separate native acceptance; this fixture verifies panel state.
            panel.GetNode("SoundController").Free();
            AddChild(panel);
            panel.DrawData();
            return panel;
        }

        private static void AssertAocPanel(Production panel, bool automated)
        {
            Equal(automated, panel.GetNode<TextureRect>("Sprites/AocPanel").Visible, "AOC artwork visibility");
            Equal(!automated, panel.GetNode<TextureRect>("Sprites/TeamFrameImage").Visible, "staff frame visibility");
            foreach (var label in new[] { "StaffNameLabel", "StaffRankLabel", "StaffCountLabel" })
                Equal(!automated, panel.GetNode<Label>("Labels/" + label).Visible, "staff readout visibility: " + label);
            Equal(!automated, panel.GetNode<TextureButton>("RemoveStaff").Visible, "staff removal hidden during automation");
        }

        private async Task AocPanelMode(bool ground)
        {
            InitializeUi();
            Save.CurrentPlanet = StellarBodies.earth;
            GameCore.Earth.Station.Built = true;
            var factory = ground ? GameCore.Earth.Factory : GameCore.Earth.Station.Factory;
            var other = ground ? GameCore.Earth.Station.Factory : GameCore.Earth.Factory;
            factory.AOC = false;
            other.AOC = true;
            var panel = OpenPanelProduction(ground);
            try
            {
                AssertAocPanel(panel, false);
                factory.AOC = true;
                panel.DrawData();
                AssertAocPanel(panel, true);
                await CaptureDisplayEvidence(ground ? "aoc-ground" : "aoc-orbit");
                factory.ProductionQueue.Add(new ProductionItem(GameCore.SingletonInstance.GameData.GetItem(ItemTypes.derrick)) { Active = true });
                panel.DrawData();
                AssertAocPanel(panel, true);
                factory.AOC = false;
                factory.ProductionQueue.Clear();
                panel.DrawData();
                AssertAocPanel(panel, false);
            }
            finally { panel.Free(); }
        }

        private void AocPanelCompletion()
        {
            Save.CurrentPlanet = StellarBodies.earth;
            GameCore.Earth.Station.Built = true;
            var factory = GameCore.Earth.Station.Factory;
            var staff = new Staff { Type = StaffType.Production, Count = 200, Leader = "Factory" };
            staff.AddAction(12);
            factory.Builder = staff;
            factory.ProductionQueue.Add(new ProductionItem(GameCore.SingletonInstance.GameData.GetItem(ItemTypes.a__o__c))
                { Active = true, Production_Complete = 3, Production_Value = 255 });
            var panel = OpenPanelProduction(false);
            try
            {
                AssertAocPanel(panel, false);
                AdvanceTickDay();
                Equal(true, factory.AOC, "normal production installs automation");
                Equal<Staff>(null, factory.Builder, "staff returned by completed automation");
                AssertAocPanel(panel, true);
            }
            finally { panel.Free(); }
        }

        private StarMap OpenDepositMap()
        {
            GameCore.SingletonInstance.ChangeScene(Scenes.ResourceMap, new List<SceneVariables>());
            return ActiveScreen<Node>().GetNode<StarMap>("StarMap");
        }

        private async Task DepositStationSelection()
        {
            InitializeUi();
            GameCore.Earth.Station.Built = true;
            GameCore.Earth.Station.BuildParts = 8;
            var moon = Save.BaseGameData.Planets[StellarBodies.the_moon];
            moon.Station.Built = false;
            moon.Station.BuildParts = 0;
            var map = OpenDepositMap();
            map.LoadMap(StellarBodies.earth);
            var icon = map.GetNode<TextureRect>("ResourceStation");
            Equal(true, icon.Visible, "built station appears below selected planet");
            Equal("res://Sprites/Buttons/Overview/Station_1.png", icon.Texture.ResourcePath, "existing platform artwork reused");
            map.PlanetGoBack.EmitSignal(BaseButton.SignalName.Pressed);
            Equal(StellarBodies.the_sun, map.CurrentLocation, "return to system chart");
            Equal(true, icon.Visible, "selected planet retains its station in system view");
            await CaptureDisplayEvidence("deposit-station");
            var earthIndex = Save.BaseGameData.Planets.Values.Where(p => p.ParentStar == StellarBodies.the_sun && !p.IsMoon)
                .OrderBy(p => p.Order).Select(p => p.PlanetId).ToList().IndexOf(StellarBodies.earth);
            map.PlanetButtons[earthIndex].EmitSignal(BaseButton.SignalName.Pressed);
            map.MoonButtons.First(button => button.Visible).EmitSignal(BaseButton.SignalName.Pressed);
            Equal(StellarBodies.the_moon, map.SelectedMoon, "actual moon control selects satellite");
            Equal(false, icon.Visible, "moon does not inherit Earth's station");
            map.LoadMap(StellarBodies.the_sun);
            Equal(false, icon.Visible, "system summary clears selected station");
        }

        private void CourseMapHasNoDepositStation()
        {
            GameCore.Earth.Station.Built = true;
            var map = OpenUi<StarMap>("res://PreFabs/StarMap.tscn");
            try
            {
                map.LoadMap(StellarBodies.earth);
                Equal(false, map.GetNode<TextureRect>("ResourceStation").Visible, "navigation-only map hides deposit artwork");
            }
            finally { map.Free(); }
        }

        private void DepositStationUpdates()
        {
            GameCore.Earth.Station.Built = false;
            GameCore.Earth.Station.BuildParts = 1;
            var map = OpenDepositMap();
            map.LoadMap(StellarBodies.earth);
            var icon = map.GetNode<TextureRect>("ResourceStation");
            Equal(true, icon.Visible, "incomplete station remains visible");
            Equal("res://Sprites/Buttons/Overview/Station_UnderConstruction.png", icon.Texture.ResourcePath, "construction state");
            GameCore.Earth.Station.Built = true;
            GameCore.Earth.Station.BuildParts = 8;
            AdvanceTickDay();
            Equal("res://Sprites/Buttons/Overview/Station_1.png", icon.Texture.ResourcePath, "completed state refreshed without selecting again");
            GameCore.Earth.Station.Built = false;
            GameCore.Earth.Station.BuildParts = 0;
            AdvanceTickDay();
            Equal(false, icon.Visible, "destroyed station disappears");
        }

        private Color ReadGamePixel(Image image, Vector2 gamePosition)
        {
            var position = Deuteros.Code.Platform.Helpers.GameViewportContainer.Instance.GetGlobalTransformWithCanvas() * gamePosition;
            position *= image.GetSize() / GetViewport().GetVisibleRect().Size;
            return image.GetPixel((int)position.X, (int)position.Y);
        }

        private async Task CaptureDisplayEvidence(string name)
        {
            var directory = OS.GetEnvironment("DEUTEROS_SCREENSHOT_DIR");
            if (string.IsNullOrEmpty(directory)) return;
            Equal(false, DisplayServer.GetName() == "headless", "screenshots require a native renderer");
            System.IO.Directory.CreateDirectory(directory);
            await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            using var screenshot = GetViewport().GetTexture().GetImage();
            Equal(Godot.Error.Ok, screenshot.SavePng(System.IO.Path.Combine(directory, name + ".png")), "visual evidence saved");
        }
    }
}
