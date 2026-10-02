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
using MenuControl = Deuteros.Code.Platform.MenuButton;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        public async Task RunInteriorBacklogRegressions()
        {
            await CheckAsync("Interior entry derives menus from Earth shuttle orbit without scene flags", InteriorEntryMenus);
            await CheckAsync("Interior menus follow shuttle takeoff and completed landing", InteriorShuttleMenus);
            await CheckAsync("Interior menus disable on departure and restore at the arrival station", InteriorTravelMenus);
            await CheckAsync("Renaming each ship updates both displays and survives a save round trip", RenameShipPersistence);
            await CheckAsync("Rename rejects blank input and supports bounded names without truncating old names", RenameValidation);
            await CheckAsync("Rename cancel Escape and input locks preserve the ship and restore pause", RenameCancellation);
        }

        private async Task<ShipInterior> OpenInterior(Ship_Types type, bool orbit = false)
        {
            await NavigationBay(type);
            var ship = Save.Ships[0];
            if (ship is Shuttle shuttle && orbit) shuttle.OnGround = false;
            ship.LocationView = false;
            Save.CurrentDay = Math.Max(Save.CurrentDay, 1);
            GameCore.SingletonInstance.ShipSelected = ship.ShipID;
            GameCore.SingletonInstance.ChangeScene(Scenes.ShipInterior, new List<SceneVariables>());
            await InputFrames();
            return ActiveScreen<ShipInterior>();
        }

        private void InteriorMenuContext(ShipInterior interior, bool ground)
        {
            var menu = ActiveScreen<MainMenu>();
            var production = menu.GetNode<MenuControl>("MainButtons/A1");
            Equal(false, production.Disabled, "production action available");
            Equal(true, production.SceneVariables.Contains(ground ? SceneVariables.Ground : SceneVariables.Orbit), "production context");
            Equal(interior.Ship.Name, menu.Location.Text, "header stays ship name");
            Equal(interior.Ship.PlanetLocation, Save.CurrentPlanet, "active planet follows ship");
        }

        private async Task InteriorEntryMenus()
        {
            var interior = await OpenInterior(Ship_Types.Shuttle, true);
            InteriorMenuContext(interior, false);
            AdvanceTickDay();
            InteriorMenuContext(interior, false);
        }

        private async Task InteriorShuttleMenus()
        {
            var interior = await OpenInterior(Ship_Types.Shuttle);
            InteriorMenuContext(interior, true);
            Press(interior, "TakeOff");
            Equal(Ship_States.TakingOff, interior.Ship.ShipState, "takeoff begins");
            InteriorMenuContext(interior, false);
            for (int i = 0; i < 5; i++) AdvanceTickDay();
            Equal(Ship_States.UnDocked, interior.Ship.ShipState, "shuttle reaches orbit");
            InteriorMenuContext(interior, false);
            Press(interior, "Land");
            AdvanceTickDay();
            InteriorMenuContext(interior, false);
            AdvanceTickDay();
            Equal(true, ((Shuttle)interior.Ship).OnGround, "landing finishes");
            InteriorMenuContext(interior, true);
        }

        private async Task InteriorTravelMenus()
        {
            var interior = await OpenInterior(Ship_Types.IOS);
            var ship = interior.Ship;
            ship.DestinationPlanetLocation = StellarBodies.the_moon;
            var moon = Save.BaseGameData.Planets[StellarBodies.the_moon];
            moon.ActiveMethanoid = false;
            moon.Station.Built = true;
            moon.Station.BuildParts = 8;
            Press(interior, "TakeOff");
            AdvanceTickDay();
            Press(interior, "EngineControls/EngageEngine");
            Equal(Ship_States.InTransit, ship.ShipState, "departure begins");
            Equal(true, ActiveScreen<MainMenu>().MenuButtons.All(b => b == null), "menus disabled immediately in transit");
            var blocker = GameCore.SingletonInstance.GetNode<InputBlocker>("InputBlocker");
            blocker.SetBlocked(true);
            try
            {
                var days = ship.TravelTimeRemain();
                for (int i = 0; i < days; i++) AdvanceTickDay();
                Equal(StellarBodies.the_moon, ship.PlanetLocation, "arrived at moon");
                InteriorMenuContext(interior, false);
                Equal(moon, interior.CurrentPlanet, "local planet follows arrival");
                var icon = interior.GetNode<TextureButton>("Location/SmallLocation").TextureNormal;
                Equal(true, icon != null, "moon arrival has a location icon");
                Equal("004488", icon.GetImage().GetPixel(23, 0).ToHtml(false), "moon inherits Earth sky colour while retaining its local station");
                Equal(true, blocker.Blocked, "refresh retains screen lock");
            }
            finally { blocker.SetBlocked(false); }
            Press(interior, "Dock");
            AdvanceTickDay();
            Equal(Ship_States.Docked, ship.ShipState, "docking completes");
            InteriorMenuContext(interior, false);
        }

        private Control OpenRename(ShipInterior interior)
        {
            var button = interior.GetNodeOrNull<Button>("TextLayout/RenameShip");
            Equal(true, button != null, "ship name is a rename button");
            button.EmitSignal(Control.SignalName.MouseEntered);
            Equal("Rename ship", GameCore.HoverText, "discoverable rename hover");
            button.EmitSignal(BaseButton.SignalName.Pressed);
            Equal(true, OverlayManager.Instance.IsOpen, "rename opens an overlay");
            Equal(true, GetTree().Paused, "rename pauses simulation");
            return OverlayManager.Instance.GetNode<Control>("GlobalOverlay/Center/RenameShip");
        }

        private async Task RenameShipPersistence()
        {
            foreach (var type in new[] { Ship_Types.Shuttle, Ship_Types.IOS, Ship_Types.SCG })
            {
                var interior = await OpenInterior(type);
                try
                {
                    var dialog = OpenRename(interior);
                    if (type == Ship_Types.SCG)
                    {
                        interior.Ship.Modules[3].ModuleType = Module_Types.Supply;
                        interior.Ship.Modules[3].ItemStored = ItemTypes.iron;
                        interior.Ship.Modules[3].ItemCount = 17;
                        interior.Ship.Modules[4].ModuleType = Module_Types.Tool;
                        interior.Ship.Modules[4].ItemStored = ItemTypes.grapple;
                        interior.Ship.Modules[4].ItemCount = 1;
                    }
                    var edit = dialog.GetNode<LineEdit>("NameEdit");
                    Equal(8, edit.GetThemeFontSize("font_size"), "edit uses readable project font size");
                    edit.Text = "  Pioneer " + type + "  ";
                    Press(dialog, "Confirm");
                    await InputFrames();
                    var name = "Pioneer " + type;
                    Equal(name, interior.Ship.Name, "model renamed");
                    Equal(name, interior.GetNode<Label>("TextLayout/ShipName").Text, "interior renamed");
                    Equal(name, ActiveScreen<MainMenu>().Location.Text, "header renamed");
                    if (type == Ship_Types.SCG)
                    {
                        Equal("17 " + ItemTypes.iron.ToScreenString(" "), interior.GetNode<Label>("TextLayout/CargoValue4").Text, "fourth SCG pod cargo displayed");
                        Equal(ItemTypes.grapple.ToScreenString(" "), interior.GetNode<Label>("TextLayout/CargoValue5").Text, "fifth SCG pod equipment displayed");
                    }
                    Equal(false, GetTree().Paused, "confirmation restores pause");
                    Equal(name, SaveStorage.Deserialize(SaveStorage.Serialize(Save)).Ships.Single(s => s.ShipID == interior.Ship.ShipID).Name, "save persists name");
                }
                finally { if (OverlayManager.Instance.IsOpen) OverlayManager.Instance.CloseOverlay(); await InputFrames(); }
            }
        }

        private async Task RenameValidation()
        {
            var interior = await OpenInterior(Ship_Types.IOS);
            var original = new string('W', 40);
            interior.Ship.Name = original;
            try
            {
                var dialog = OpenRename(interior);
                var edit = dialog.GetNode<LineEdit>("NameEdit");
                Equal(original, edit.Text, "existing long name is never truncated on opening");
                edit.Text = "   ";
                Press(dialog, "Confirm");
                Equal(true, OverlayManager.Instance.IsOpen, "blank input keeps dialog open");
                Equal(original, interior.Ship.Name, "blank cannot alter name");
                Equal(true, dialog.GetNode<Label>("Validation").Text.Length > 0, "inline validation explains rejection");
                edit.Text = new string('W', 41);
                Press(dialog, "Confirm");
                Equal(true, OverlayManager.Instance.IsOpen, "overlong input keeps dialog open");
                Equal(original, interior.Ship.Name, "overlong cannot alter name");
                edit.Text = new string('W', 24);
                edit.EmitSignal(LineEdit.SignalName.TextSubmitted, edit.Text);
                await InputFrames();
                Equal(new string('W', 24), interior.Ship.Name, "maximum new name accepted via Enter");
                Equal(true, interior.GetNode<Label>("TextLayout/ShipName").ClipText, "long names cannot cover neighbouring fields");
            }
            finally { if (OverlayManager.Instance.IsOpen) OverlayManager.Instance.CloseOverlay(); await InputFrames(); }
        }

        private async Task RenameCancellation()
        {
            var interior = await OpenInterior(Ship_Types.IOS);
            var original = interior.Ship.Name;
            try
            {
                var dialog = OpenRename(interior);
                dialog.GetNode<LineEdit>("NameEdit").Text = "Cancelled";
                await RightClick(Vector2.Zero);
                Equal(Scenes.ShipInterior, GameCore.SingletonInstance.currentScene, "right click cannot navigate through rename");
                Equal(true, OverlayManager.Instance.IsOpen, "right click keeps dialog");
                Press(dialog, "Cancel");
                await InputFrames();
                Equal(original, interior.Ship.Name, "cancel preserves name");
                Equal(false, GetTree().Paused, "cancel restores running state");
                GetTree().Paused = true;
                dialog = OpenRename(interior);
                dialog.GetNode<LineEdit>("NameEdit").Text = "Escape";
                using var escape = new InputEventAction { Action = "ui_cancel", Pressed = true };
                GetViewport().PushInput(escape, true);
                await InputFrames();
                Equal(false, OverlayManager.Instance.IsOpen, "Escape closes rename");
                Equal(original, interior.Ship.Name, "Escape preserves name");
                Equal(true, GetTree().Paused, "pre-existing pause preserved");
                GetTree().Paused = false;
                GlobalInput.LockUi();
                Press(interior, "TextLayout/RenameShip");
                Equal(false, OverlayManager.Instance.IsOpen, "UI lock prevents opening rename");
                GlobalInput.UnlockUi();
                var blocker = GameCore.SingletonInstance.GetNode<InputBlocker>("InputBlocker");
                blocker.SetBlocked(true);
                try
                {
                    Press(interior, "TextLayout/RenameShip");
                    Equal(false, OverlayManager.Instance.IsOpen, "screen lock prevents opening rename");
                }
                finally { blocker.SetBlocked(false); }
                Cursor.LockToRect(new Rect2(Vector2.Zero, new Vector2(10, 10)));
                try
                {
                    Press(interior, "TextLayout/RenameShip");
                    Equal(false, OverlayManager.Instance.IsOpen, "existing ship modal prevents rename");
                }
                finally { Cursor.Unlock(); }
            }
            finally { GlobalInput.UnlockUi(); if (OverlayManager.Instance.IsOpen) OverlayManager.Instance.CloseOverlay(); await InputFrames(); GetTree().Paused = false; }
        }
    }
}
