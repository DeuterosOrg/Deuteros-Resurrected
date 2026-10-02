using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Objects.ModuleTextFrame;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Platform.Screens;
using Godot;
using static Deuteros.Code.Enums;
using TextWindow = Deuteros.Code.Platform.Screens.ModuleScenes.ModuleTextFrame;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        public async Task RunPilotWarningRegressions()
        {
            foreach (var hull in new[] { Ship_Types.Shuttle, Ship_Types.IOS, Ship_Types.SCG })
                await CheckAsync($"OF deployment without a pilot warns and preserves all {hull} state", () => MissingDeploymentPilot(hull));
            await CheckAsync("OF deployment with an empty pilot crew warns without deploying", () => MissingDeploymentPilot(Ship_Types.IOS, true));
            await CheckAsync("A qualified pilot deploys one OF section and releases its message window", QualifiedDeploymentPilot);
            await CheckAsync("OF pilot warning does not bypass deployment location and state rules", IneligibleDeploymentWarning);
        }

        private async Task<ShipInterior> DeploymentInterior(Ship_Types hull, bool pilot, Ship_States state = Ship_States.UnDocked,
            bool ground = false, bool complete = false, bool asteroid = false)
        {
            await NavigationBay(hull);
            var ship = Save.Ships[0];
            ship.ShipState = state;
            ship.LocationView = false;
            ship.Pilot = pilot ? new Staff { Type = StaffType.Marines, Leader = "Deployment", Count = 1 } : null;
            if (ship is Shuttle shuttle) shuttle.OnGround = ground;
            if (asteroid) ship.PlanetLocation = StellarBodies.asteroids;
            Save.CurrentPlanet = ship.PlanetLocation;
            var station = Save.BaseGameData.Planets[ship.PlanetLocation].Station;
            station.BuildParts = complete ? 8 : 2;
            station.Built = complete;
            ship.Modules[0].ModuleType = Module_Types.Tool;
            ship.Modules[0].ItemStored = ItemTypes.of_frame;
            ship.Modules[0].ItemCount = 1;
            GameCore.SingletonInstance.ShipSelected = ship.ShipID;
            GameCore.SingletonInstance.ChangeScene(Scenes.ShipInterior, new List<SceneVariables>());
            await InputFrames();
            return ActiveScreen<ShipInterior>();
        }

        private async Task MissingDeploymentPilot(Ship_Types hull, bool emptyCrew = false)
        {
            var interior = await DeploymentInterior(hull, false);
            if (emptyCrew) interior.Ship.Pilot = new Staff { Type = StaffType.Marines, Leader = "Empty", Count = 0 };
            var text = Save.BaseGameData.ModuleFrameTexts[ModuleFrameText.Station_Deploy];
            var originalLines = text.Lines;
            if (emptyCrew) text.Lines = new List<Line>();
            var core = GameCore.SingletonInstance;
            var wasProcessing = core.IsProcessing();
            // Compare UI mutations while the independent real-time SDM clock is held steady.
            core.SetProcess(false);
            var before = Deuteros.Code.Utility.SaveStorage.Serialize(Save);
            try
            {
                Press(interior, "Modules/00");
                Equal(true, OverlayManager.Instance.IsOpen, "missing pilot warning opens");
                Equal(true, GetTree().Paused, "warning pauses the game");
                var dialog = OverlayManager.Instance.GetNode("GlobalOverlay/Center").GetChild(0);
                var warning = dialog.GetNode<RichTextLabel>("Labels/WarningBody");
                Equal("Orbital Factory Section", dialog.GetNode<Label>("Labels/ToolType").Text, "warning identifies OF equipment");
                Equal("1", dialog.GetNode<Label>("Window/Background/Number").Text, "warning identifies selected pod");
                Equal(true, warning.Text.Contains("Pilot"), "warning identifies required crew rank");
                Equal(before, Deuteros.Code.Utility.SaveStorage.Serialize(Save), "warning changes no saved ship station cargo or crew state");
                await RightClick(Vector2.Zero);
                Equal(Scenes.ShipInterior, GameCore.SingletonInstance.currentScene, "warning blocks overview shortcut");
                Press(dialog, "Dismiss");
                await InputFrames();
                Equal(false, OverlayManager.Instance.IsOpen, "warning dismisses");
                Equal(false, GetTree().Paused, "dismiss restores running state");
                GetTree().Paused = true;
                Press(interior, "Modules/00");
                Equal(true, OverlayManager.Instance.IsOpen, "warning can reopen");
                using var escape = new InputEventAction { Action = "ui_cancel", Pressed = true };
                GetViewport().PushInput(escape, true);
                await InputFrames();
                Equal(false, OverlayManager.Instance.IsOpen, "Escape dismisses warning");
                Equal(true, GetTree().Paused, "prior pause is restored");
                Equal(before, Deuteros.Code.Utility.SaveStorage.Serialize(Save), "repeated warning preserves all state");
            }
            finally
            {
                text.Lines = originalLines;
                if (OverlayManager.Instance.IsOpen) OverlayManager.Instance.CloseOverlay();
                await InputFrames();
                GetTree().Paused = false;
                core.SetProcess(wasProcessing);
            }
        }

        private async Task QualifiedDeploymentPilot()
        {
            var interior = await DeploymentInterior(Ship_Types.IOS, true);
            var ship = interior.Ship;
            var station = interior.CurrentPlanet.Station;
            var originalPilot = ship.Pilot;
            var fuel = ship.Fuel;
            var text = Save.BaseGameData.ModuleFrameTexts[ModuleFrameText.Station_Deploy];
            var originalLines = text.Lines;
            TextWindow window = null;
            void CaptureWindow(Node node) { if (node is TextWindow message) window = message; }
            GetTree().NodeAdded += CaptureWindow;
            var placed = 0;
            void PiecePlaced(StellarBodies body) { if (body == ship.PlanetLocation) placed++; }
            GameCore.SingletonInstance.StationPiecePlaced += PiecePlaced;
            try
            {
                // Exercise the real deployment callback and message lifecycle without typing/audio delays.
                text.Lines = new List<Line>();
                Press(interior, "Modules/00");
                await InputFrames();
                Equal(3, station.BuildParts, "one section deployed");
                Equal(false, station.Built, "partial station remains incomplete");
                Equal(ItemTypes.none, ship.Modules[0].ItemStored, "frame consumed");
                Equal(0, ship.Modules[0].ItemCount, "frame count cleared");
                Equal(1, placed, "one placement notification");
                Equal(originalPilot, ship.Pilot, "crew retained");
                Equal(fuel, ship.Fuel, "deployment does not burn extra fuel");
                Equal(false, OverlayManager.Instance.IsOpen, "qualified pilot gets no warning");
                Equal(false, GameCore.SingletonInstance.GetNode<InputBlocker>("InputBlocker").Blocked, "deployment unlocks input");
                Equal(true, window != null, "normal deployment message was shown");
                Equal(false, GodotObject.IsInstanceValid(window), "finished deployment message freed");
            }
            finally
            {
                text.Lines = originalLines;
                GameCore.SingletonInstance.StationPiecePlaced -= PiecePlaced;
                GetTree().NodeAdded -= CaptureWindow;
                if (GodotObject.IsInstanceValid(window)) window.QueueFree();
                await InputFrames();
            }
        }

        private async Task IneligibleDeploymentWarning()
        {
            foreach (var scenario in new[] { "ground", "transit", "complete", "asteroid" })
            {
                var interior = await DeploymentInterior(scenario == "ground" ? Ship_Types.Shuttle : Ship_Types.IOS,
                    false, scenario == "transit" ? Ship_States.InTransit : Ship_States.UnDocked,
                    scenario == "ground", scenario == "complete", scenario == "asteroid");
                var core = GameCore.SingletonInstance;
                var wasProcessing = core.IsProcessing();
                core.SetProcess(false);
                try
                {
                    var before = Deuteros.Code.Utility.SaveStorage.Serialize(Save);
                    Press(interior, "Modules/00");
                    await InputFrames();
                    Equal(false, OverlayManager.Instance.IsOpen, "no irrelevant pilot warning in " + scenario);
                    Equal(before, Deuteros.Code.Utility.SaveStorage.Serialize(Save), "ineligible deployment preserves state in " + scenario);
                }
                finally
                {
                    core.SetProcess(wasProcessing);
                }
            }
        }
    }
}
