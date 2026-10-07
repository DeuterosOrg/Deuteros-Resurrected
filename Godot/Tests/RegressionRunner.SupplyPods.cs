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
        private async Task RunSupplyPodRegressions()
        {
            foreach (var type in new[] { Ship_Types.Shuttle, Ship_Types.IOS, Ship_Types.SCG })
                await CheckAsync($"Ditch selected supply pod on {type} preserves stores and other pods", () => SupplyPodDitch(type, Ship_States.Docked));
            foreach (var state in new[] { Ship_States.UnDocked, Ship_States.InTransit, Ship_States.Docking, Ship_States.Launching })
                await CheckAsync($"Ditch supply cargo while {state} preserves journey and ACC", () => SupplyPodDitch(Ship_Types.IOS, state));
            await CheckAsync("Ditch supply cargo during AMA mining leaves mining equipment intact", () => SupplyPodDitch(Ship_Types.IOS, Ship_States.Docked, true));
            await CheckAsync("Cargo controls respect input locks and cancellation", SupplyPodLocks);
            await CheckAsync("Cargo dialog cannot ditch replaced cargo or outlive its ship screen", SupplyPodStale);
            await CheckAsync("Closed cargo dialog rejects retained Ditch signals", SupplyPodClosedSignal);
        }

        private async Task<ShipInterior> SupplyPodInterior(Ship_Types type)
        {
            var interior = await OpenInterior(type);
            foreach (var module in interior.Ship.Modules)
            {
                module.ModuleType = Module_Types.Supply;
                module.ItemStored = ItemTypes.iron;
                module.ItemCount = 12;
            }
            return interior;
        }

        private async Task<Control> OpenSupplyPods(ShipInterior interior)
        {
            var button = interior.GetNodeOrNull<Button>("TextLayout/CargoActions");
            Equal(true, button != null, "interior exposes the cargo action button");
            var point = button.GetGlobalRect().GetCenter();
            PushGameInput(new InputEventMouseMotion { Position = point, GlobalPosition = point });
            foreach (var pressed in new[] { true, false })
                PushGameInput(new InputEventMouseButton { Position = point, GlobalPosition = point, ButtonIndex = MouseButton.Left, Pressed = pressed });
            await InputFrames();
            Equal(true, OverlayManager.Instance.IsOpen, "cargo dialog opens");
            return OverlayManager.Instance.GetNode<Control>("GlobalOverlay/GameArea/Center/SupplyPods");
        }

        private async Task SupplyPodDitch(Ship_Types type, Ship_States state, bool mining = false)
        {
            var interior = await SupplyPodInterior(type);
            var ship = interior.Ship;
            ship.ShipState = state;
            ship.ACC = new Deuteros.Code.Objects.ACC { Ship = ship, Active = true, Source = StellarBodies.earth, Destination = StellarBodies.mars,
                SourceItems = new List<ItemTypes>(), DestinationItems = new List<ItemTypes>(),
                CurrentSource = ItemTypes.iron, CurrentDestination = ItemTypes.iron };
            var index = ship.Modules.Count - 1;
            var selected = ship.Modules[index];
            selected.ItemStored = ItemTypes.titanium;
            selected.ItemCount = 250;
            if (type == Ship_Types.SCG)
            {
                ship.Modules[1].ModuleType = Module_Types.Cryo;
                ship.Modules[1].ItemStored = ItemTypes.none;
                ship.Modules[1].ItemCount = 0;
                ship.Modules[2].ModuleType = Module_Types.Tool;
                ship.Modules[2].ItemStored = ItemTypes.grapple;
                ship.Modules[2].ItemCount = 1;
            }
            if (mining)
            {
                ship.PlanetLocation = StellarBodies.asteroids;
                ship.Modules[0].ModuleType = Module_Types.Tool;
                ship.Modules[0].ItemStored = ItemTypes.a__m__a;
                ship.Modules[0].LastMinedDay = 19;
            }
            var ground = GameCore.Earth.PlanetResources.Stores.Items.ToArray();
            var orbit = GameCore.Earth.Station.Resources.Stores.Items.ToArray();
            var fuel = ship.Fuel;
            var start = ship.StartTravelDay;
            try
            {
                await CaptureDisplayEvidence("supply-cargo-entry-" + type);
                var dialog = await OpenSupplyPods(interior);
                var action = dialog.GetNode<Button>($"Rows/Pod{index}/Ditch");
                Equal(false, action.Disabled, "loaded supply pod can be emptied");
                if (mining) Equal(true, dialog.GetNode<Button>("Rows/Pod0/Ditch").Disabled, "tool cannot be ditched by supply controls");
                if (type == Ship_Types.SCG)
                {
                    Equal(true, dialog.GetNode<Button>("Rows/Pod1/Ditch").Disabled, "cryo cannot be ditched");
                    Press(dialog, "Rows/Pod2/Ditch");
                    Equal(ItemTypes.grapple, ship.Modules[2].ItemStored, "tool rejects even a retained signal");
                }
                await CaptureDisplayEvidence("supply-pods-" + type + (mining ? "-mining" : "-" + state));
                var point = action.GetGlobalRect().GetCenter();
                GetViewport().PushInput(new InputEventMouseMotion { Position = point, GlobalPosition = point }, true);
                foreach (var pressed in new[] { true, false })
                    GetViewport().PushInput(new InputEventMouseButton { Position = point, GlobalPosition = point, ButtonIndex = MouseButton.Left, Pressed = pressed }, true);
                await InputFrames();
                Equal(0, selected.ItemCount, "selected cargo discarded");
                Equal(ItemTypes.none, selected.ItemStored, "empty cargo type cleared");
                Equal(Module_Types.Supply, selected.ModuleType, "pod remains fitted");
                Equal("Empty", interior.GetNode<Label>($"TextLayout/CargoValue{index + 1}").Text, "interior cargo refreshes");
                Equal(true, action.Disabled, "empty pod cannot repeat the action");
                Equal(ground.Length, GameCore.Earth.PlanetResources.Stores.Items.Count, "no new ground store entries");
                Equal(orbit.Length, GameCore.Earth.Station.Resources.Stores.Items.Count, "no new orbital store entries");
                foreach (var pair in ground) Equal(pair.Value, GameCore.Earth.PlanetResources.Stores[pair.Key], "ground stores unchanged");
                foreach (var pair in orbit) Equal(pair.Value, GameCore.Earth.Station.Resources.Stores[pair.Key], "orbital stores unchanged");
                if (index > 0) Equal(12, ship.Modules[0].ItemCount, "neighbor cargo untouched");
                if (mining)
                {
                    Equal(ItemTypes.a__m__a, ship.Modules[0].ItemStored, "AMA stays fitted");
                    Equal((uint)19, ship.Modules[0].LastMinedDay, "mining schedule unchanged");
                }
                Equal(state, ship.ShipState, "journey state unchanged");
                Equal(start, ship.StartTravelDay, "departure time unchanged");
                Equal(fuel, ship.Fuel, "fuel unchanged");
                Equal(true, ship.ACC.Active, "ACC remains active");
                Equal(StellarBodies.mars, ship.ACC.Destination, "ACC route unchanged");
                var restored = SaveStorage.Deserialize(SaveStorage.Serialize(Save)).Ships.Single(s => s.ShipID == ship.ShipID).Modules[index];
                Equal(0, restored.ItemCount, "save retains empty quantity");
                Equal(ItemTypes.none, restored.ItemStored, "save retains empty type");
                Press(dialog, "Close");
                await InputFrames();
                Equal(false, GetTree().Paused, "close restores simulation");
            }
            finally { if (OverlayManager.Instance.IsOpen) OverlayManager.Instance.CloseOverlay(); await InputFrames(); }
        }

        private async Task SupplyPodLocks()
        {
            var interior = await SupplyPodInterior(Ship_Types.IOS);
            try
            {
                var dialog = await OpenSupplyPods(interior);
                using var cancel = new InputEventAction { Action = "ui_cancel", Pressed = true };
                OverlayManager.Instance._UnhandledInput(cancel);
                await InputFrames();
                Equal(12, interior.Ship.Modules[0].ItemCount, "Escape keeps cargo");
                Equal(false, GetTree().Paused, "Escape restores simulation");
                GlobalInput.LockUi();
                Press(interior, "TextLayout/CargoActions");
                Equal(false, OverlayManager.Instance.IsOpen, "UI lock blocks cargo actions");
                GlobalInput.UnlockUi();
                GameCore.LockScreen();
                try
                {
                    Press(interior, "TextLayout/CargoActions");
                    Equal(false, OverlayManager.Instance.IsOpen, "screen lock blocks cargo actions");
                }
                finally { GameCore.UnLockScreen(); }
            }
            finally { GlobalInput.UnlockUi(); if (OverlayManager.Instance.IsOpen) OverlayManager.Instance.CloseOverlay(); await InputFrames(); }
        }

        private async Task SupplyPodClosedSignal()
        {
            var interior = await SupplyPodInterior(Ship_Types.IOS);
            try
            {
                var dialog = await OpenSupplyPods(interior);
                Press(dialog, "Close");
                Press(dialog, "Rows/Pod0/Ditch");
                Equal(12, interior.Ship.Modules[0].ItemCount, "closing invalidates queued actions immediately");
            }
            finally { if (OverlayManager.Instance.IsOpen) OverlayManager.Instance.CloseOverlay(); await InputFrames(); }
        }

        private async Task SupplyPodStale()
        {
            var interior = await SupplyPodInterior(Ship_Types.IOS);
            try
            {
                var dialog = await OpenSupplyPods(interior);
                interior.Ship.Modules[0].ItemStored = ItemTypes.gold;
                Press(dialog, "Rows/Pod0/Ditch");
                Equal(12, interior.Ship.Modules[0].ItemCount, "changed cargo needs a fresh view");
                Equal(ItemTypes.gold, interior.Ship.Modules[0].ItemStored, "replacement preserved");
                GameCore.SingletonInstance.ChangeScene(Scenes.Overview, new List<SceneVariables>());
                await InputFrames();
                Equal(false, OverlayManager.Instance.IsOpen, "scene exit closes cargo dialog");
                Equal(false, GetTree().Paused, "scene exit restores simulation");
            }
            finally { if (OverlayManager.Instance.IsOpen) OverlayManager.Instance.CloseOverlay(); await InputFrames(); }
        }
    }
}
