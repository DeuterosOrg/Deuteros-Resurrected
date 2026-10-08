using System;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Utility;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task SettingsConfirmLaunch()
        {
            await WithSettings(async settings =>
            {
                var core = GameCore.SingletonInstance;
                core.SetProcess(false);
                var manager = SettingsManager.Instance;
                var overlays = OverlayManager.Instance;
                Equal(false, manager.GetDefault("gameplay/confirm_launch").AsBool(), "launch confirmation is opt-in");
                settings.ConfirmLaunchRow.StepValue(1);
                Press(settings, "%ApplyButton");
                using (var disk = new GameConfig(manager.Config.FilePath))
                    Equal(true, disk.GetValue("gameplay", "confirm_launch").AsBool(), "confirmation preference persists");
                Press(settings, "%CloseButton");
                await InputFrames();

                foreach (var hull in new[] { Ship_Types.Shuttle, Ship_Types.IOS, Ship_Types.SCG })
                {
                    var interior = await OpenInterior(hull);
                    var ship = interior.Ship;
                    ship.Engine = true;
                    ship.Fuel = 250;
                    ship.ShipState = Ship_States.Docked;
                    var before = SaveStorage.Serialize(Save);
                    Press(interior, "TakeOff");
                    Equal(true, overlays.IsOpen, "enabled preference asks before manual departure");
                    Equal(before, SaveStorage.Serialize(Save), "asking leaves crew fuel cargo travel and world unchanged");
                    Equal(true, GetTree().Paused, "confirmation owns a simulation pause");
                    var dialog = overlays.GetNode<Control>("GlobalOverlay/GameArea/Center/ConfirmLaunch");
                    Equal(true, dialog.GetNode<Label>("Message").Text.Contains(ship.Name), "prompt identifies selected ship");
                    Equal(true, dialog.GetNode<Button>("Cancel").HasFocus(), "Cancel has initial keyboard focus");
                    if (hull == Ship_Types.IOS) await CaptureDisplayEvidence("confirm-launch");
                    Press(dialog, "Cancel");
                    await InputFrames();
                    Equal(before, SaveStorage.Serialize(Save), "Cancel leaves world unchanged");
                    Press(interior, "TakeOff");
                    await PushGameKey(Key.Escape);
                    Equal(false, overlays.IsOpen, "Escape cancels the prompt");
                    Equal(before, SaveStorage.Serialize(Save), "Escape leaves world unchanged");
                    Press(interior, "TakeOff");
                    dialog = overlays.GetNode<Control>("GlobalOverlay/GameArea/Center/ConfirmLaunch");
                    Press(dialog, "Confirm");
                    var departed = SaveStorage.Serialize(Save);
                    Press(dialog, "Confirm");
                    Equal(departed, SaveStorage.Serialize(Save), "duplicate confirmation changes no world or crew state");
                    await InputFrames();
                    Equal(hull == Ship_Types.Shuttle ? Ship_States.TakingOff : Ship_States.Launching,
                        ship.ShipState, "confirmation performs the original departure");
                    Equal(false, GetTree().Paused, "confirmation releases its pause");

                    ship.ShipState = Ship_States.UnDocked;
                    ship.DestinationPlanetLocation = StellarBodies.the_moon;
                    ship.DestinationStarLocation = StellarBodies.the_sun;
                    interior.UpdateState();
                    Press(interior, "EngineControls/EngageEngine");
                    Equal(true, overlays.IsOpen, "course engagement also confirms");
                    dialog = overlays.GetNode<Control>("GlobalOverlay/GameArea/Center/ConfirmLaunch");
                    ship.DestinationPlanetLocation = StellarBodies.mars;
                    Press(dialog, "Confirm");
                    await InputFrames();
                    Equal(Ship_States.UnDocked, ship.ShipState, "changed course cannot reuse an old confirmation");
                    interior.UpdateState();
                    Press(interior, "EngineControls/EngageEngine");
                    dialog = overlays.GetNode<Control>("GlobalOverlay/GameArea/Center/ConfirmLaunch");
                    Press(dialog, "Confirm");
                    await InputFrames();
                    Equal(Ship_States.InTransit, ship.ShipState, "confirmed course uses existing travel rules");
                    Equal(true, ship.EngineEngaged, "confirmed course engages engines");
                }

                var manual = await OpenInterior(Ship_Types.IOS);
                manual.Ship.Engine = true;
                manual.Ship.Fuel = 250;
                manual.Ship.ShipState = Ship_States.Docked;
                Press(manual, "TakeOff");
                var stale = overlays.GetNode<Control>("GlobalOverlay/GameArea/Center/ConfirmLaunch");
                var previous = Save;
                core.GameData.ActiveSaveFile = CoreData.CreateNewSaveFile();
                Press(stale, "Confirm");
                await InputFrames();
                Equal(Ship_States.Docked, manual.Ship.ShipState, "loaded world invalidates retained departure");
                core.GameData.ActiveSaveFile = previous;

                Press(manual, "TakeOff");
                stale = overlays.GetNode<Control>("GlobalOverlay/GameArea/Center/ConfirmLaunch");
                var oldShip = manual.Ship;
                core.ChangeScene(Scenes.Overview, new());
                await InputFrames();
                Press(stale, "Confirm");
                await InputFrames();
                Equal(Ship_States.Docked, oldShip.ShipState, "departed screen cannot execute its retained confirmation");
                manual = await OpenInterior(Ship_Types.IOS);
                manual.Ship.Engine = true;
                manual.Ship.Fuel = 250;
                manual.Ship.ShipState = Ship_States.Docked;

                var cursor = core.GetNode<GlobalInput>("GameContainer/GameViewport/VirtualCursorView");
                cursor.LockToRect(new Rect2(0,0,320,200));
                Press(manual, "TakeOff");
                Equal(false, overlays.IsOpen, "cursor modal prevents a second command");
                Equal(Ship_States.Docked, manual.Ship.ShipState, "cursor modal prevents departure");
                cursor.Unlock();
                GetTree().Paused = true;
                Press(manual, "TakeOff");
                Press(overlays.GetNode("GlobalOverlay/GameArea/Center/ConfirmLaunch"), "Cancel");
                await InputFrames();
                Equal(true, GetTree().Paused, "Cancel preserves a prior independent pause");
                GetTree().Paused = false;
                manual.Ship.Fuel = 0;
                Press(manual, "TakeOff");
                Press(overlays.GetNode("GlobalOverlay/GameArea/Center/ConfirmLaunch"), "Confirm");
                await InputFrames();
                Equal(Ship_States.Docked, manual.Ship.ShipState, "confirmation cannot bypass missing fuel");
                manual.Ship.Fuel = 250;
                manual.Ship.DestinationPlanetLocation = StellarBodies.the_moon;
                manual.Ship.Modules[0].ModuleType = Module_Types.Supply;
                manual.Ship.ACC = new ACC { Ship = manual.Ship, Source = StellarBodies.earth,
                    Destination = StellarBodies.the_moon, SourceItems = new(), DestinationItems = new(),
                    CurrentSource = ItemTypes.iron, CurrentDestination = ItemTypes.iron };
                manual.Ship.ACC.Activate();
                Equal(Ship_States.Launching, manual.Ship.ShipState, "ACC activation departs without confirmation");
                Equal(false, overlays.IsOpen, "automation does not open a confirmation");
                manual.Ship.ShipState = Ship_States.UnDocked;
                manual.Ship.ACC.Update(Ship_States.Launching);
                Equal(Ship_States.InTransit, manual.Ship.ShipState, "ACC engages its course without confirmation");
                Equal(false, overlays.IsOpen, "ACC course transition does not open a confirmation");
                manual.Ship.ACC.Active = false;
                manual.Ship.ShipState = Ship_States.Docked;
                manager.SetSetting("gameplay/confirm_launch", false);
                Press(manual, "TakeOff");
                Equal(Ship_States.Launching, manual.Ship.ShipState, "Off retains direct departure");
                manual.Ship.ShipState = Ship_States.Docked;
                manager.SetSetting("gameplay/confirm_launch", "invalid");
                Press(manual, "TakeOff");
                Equal(Ship_States.Launching, manual.Ship.ShipState, "malformed preference falls back to opt-in Off");
                manager.Revert();
                settings = OpenSettings();
                Equal(true, settings.ConfirmLaunchRow.SettingValue.AsBool(), "reopen restores saved On");
                settings.ConfirmLaunchRow.StepValue(-1);
                Press(settings, "%CancelButton");
                await InputFrames();
                Equal(true, manager.GetSetting("gameplay/confirm_launch").AsBool(), "Cancel restores saved preference");
            });
        }
    }
}
