using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Objects.ModuleTextFrame;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Platform.Screens;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task RunTradeDecisionRegressions()
        {
            await CheckAsync("Trade waits for consent and accepts cargo exactly once", TradeAcceptOnce);
            foreach (var count in new[] { 0, 5, 15 })
                await CheckAsync($"Trade refusal preserves cargo and floors counter from {count}", () => TradeDecline(count));
            await CheckAsync("Trade exchanges all supported mineral pairs without changing quantities", TradeMineralPairs);
            await CheckAsync("Trade preserves unsupported modules and ignores empty cargo", TradeUnsupportedCargo);
            await CheckAsync("Closing trade choice abandons it without mutating cargo or counter", TradeAbandon);
            await CheckAsync("Leaving ship interior cancels pending trade and releases overlay", TradeSceneExit);
            foreach (var count in new[] { 16, 18 })
                await CheckAsync($"Trade count {count} enters war before offering an exchange", () => TradeWarBoundary(count));
            await CheckAsync("Peaceful trading cannot run again after war", TradeAfterWar);
            await CheckAsync("Leaving during alien typing cancels playback and releases screen lock", TradeTypingExit);
            await CheckAsync("Trade choice accepts pointer and keyboard activation", TradePhysicalInputRoutes);
            await CheckAsync("Interrupting accepted trade response preserves the complete exchange", TradeResponseExit);
            await CheckAsync("The sixteenth accepted trade reaches war on the next encounter", TradeSixteenthAcceptance);
            await CheckAsync("A stale trade offer cannot overwrite changed cargo", TradeStaleCargo);
        }

        private async Task WithTrade(Func<IOS, ShipInterior, Task> test)
        {
            InitializeUi();
            var ship = UnknownObjectShip();
            ship.ShipState = Ship_States.Docked;
            ship.Engine = true;
            ship.Fuel = 100;
            ship.Modules[0].ItemStored = ItemTypes.commspod;
            ship.Modules[0].HeldItem = null;
            foreach (var module in ship.Modules.Skip(1))
            {
                module.ModuleType = Module_Types.Supply;
                module.ItemStored = ItemTypes.iron;
                module.ItemCount = 123;
                module.HeldItem = null;
            }
            Save.Ships.Clear();
            Save.Ships.Add(ship);
            Save.MethanoidTradeCount = 5;
            Save.AtWar = false;
            GameCore.Earth.ActiveMethanoid = true;
            GameCore.Earth.Station.Built = true;
            var keys = new[] { ModuleFrameText.Methanoid_TradeQuestion, ModuleFrameText.Methanoid_Trade1,
                ModuleFrameText.Methanoid_Trade2, ModuleFrameText.Methanoid_No_Trade,
                ModuleFrameText.Methanoid_No_Cargo, ModuleFrameText.Methanoid_War_Warning };
            var frames = keys.Select(key => Save.BaseGameData.ModuleFrameTexts[key]).ToArray();
            var lines = frames.Select(frame => frame.Lines).ToArray();
            try
            {
                foreach (var frame in frames) frame.Lines = new List<Line>();
                var core = GameCore.SingletonInstance;
                core.ShipSelected = ship.ShipID;
                core.ChangeScene(Scenes.ShipInterior, new List<SceneVariables>());
                await InputFrames();
                await test(ship, ActiveScreen<ShipInterior>());
            }
            finally
            {
                OverlayManager.Instance.CloseOverlay();
                GameCore.SingletonInstance.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
                await InputFrames();
                for (var i = 0; i < frames.Length; i++) frames[i].Lines = lines[i];
                GameCore.Earth.ActiveMethanoid = false;
                await DrainStoppedAudio();
            }
        }

        private async Task<Control> OpenTradeChoice(ShipInterior interior)
        {
            Press(interior, "Modules/01");
            await InputFrames();
            var dialog = OverlayManager.Instance.GetNodeOrNull<Control>("GlobalOverlay/Center/TradeDecision");
            Equal(true, dialog != null, "trade waits for an explicit decision");
            return dialog;
        }

        private async Task TradeAcceptOnce() => await WithTrade(async (ship, interior) =>
        {
            var dialog = await OpenTradeChoice(interior);
            var question = dialog.GetNode<Label>("Question");
            Equal(true, question.GetCombinedMinimumSize().X <= dialog.Size.X - 20, "question fits panel at native scale");
            Equal(ItemTypes.iron, ship.Modules[1].ItemStored, "question does not exchange cargo");
            Equal(5, Save.MethanoidTradeCount, "question does not count as consent");
            Equal(100, ship.Fuel, "unaccepted offer does not refuel");
            Equal(true, GetTree().Paused, "pending decision pauses underlying simulation");
            Press(interior, "Modules/01");
            Equal(dialog, OverlayManager.Instance.GetNode<Control>("GlobalOverlay/Center/TradeDecision"), "repeat module input keeps the same decision");
            await CaptureDisplayEvidence("methanoid-trade-choice");
            Press(dialog, "Accept");
            Press(dialog, "Accept");
            await InputFrames();
            Equal(6, Save.MethanoidTradeCount, "accept counted once");
            Equal(250, ship.Fuel, "accepted original trade fills the fuel gauge");
            Equal(ItemTypes.silica, ship.Modules[1].ItemStored, "accepted exchange");
            Equal(123, ship.Modules[1].ItemCount, "quantity preserved");
            Equal(ItemTypes.commspod, ship.Modules[0].ItemStored, "equipment preserved");
            Equal(false, OverlayManager.Instance.IsOpen, "decision dismissed");
            Equal(false, GetTree().Paused, "simulation resumes");
        });

        private async Task TradeDecline(int count) => await WithTrade(async (ship, interior) =>
        {
            Save.MethanoidTradeCount = count;
            var dialog = await OpenTradeChoice(interior);
            Press(dialog, "Decline");
            Press(dialog, "Accept");
            await InputFrames();
            Equal(Math.Max(0, count - 1), Save.MethanoidTradeCount, "original decline decrements once with zero floor");
            Equal(ItemTypes.iron, ship.Modules[1].ItemStored, "decline preserves item");
            Equal(123, ship.Modules[1].ItemCount, "decline preserves amount");
            Equal(false, Save.AtWar, "decline does not trigger war");
            Equal(100, ship.Fuel, "refusal gives no fuel gift");
        });

        private async Task TradeMineralPairs() => await WithTrade(async (ship, interior) =>
        {
            var expected = new[] { 0, 14, 5, 4, 3, 2, 8, 9, 6, 7, 13, 12, 11, 10, 1, 15, 16 };
            foreach (var id in Enumerable.Range(1, 16))
            {
                ship.ShipState = Ship_States.Docked;
                Save.MethanoidTradeCount = 0;
                ship.Modules[1].ItemStored = (ItemTypes)id;
                ship.Modules[1].ItemCount = id + 1;
                var dialog = await OpenTradeChoice(ActiveScreen<ShipInterior>());
                Press(dialog, "Accept");
                await InputFrames();
                Equal((ItemTypes)expected[id], ship.Modules[1].ItemStored, "original lookup for item " + id);
                Equal(id + 1, ship.Modules[1].ItemCount, "exact quantity for item " + id);
                Equal(1, Save.MethanoidTradeCount, "one accepted encounter including unchanged fuels");
            }
        });

        private async Task TradeUnsupportedCargo() => await WithTrade(async (ship, interior) =>
        {
            foreach (var module in ship.Modules.Skip(1)) { module.ItemStored = ItemTypes.derrick; module.ItemCount = 1; }
            Press(interior, "Modules/01");
            await InputFrames();
            Equal(false, OverlayManager.Instance.IsOpen, "equipment is not a mineral offer");
            Equal(5, Save.MethanoidTradeCount, "unsupported cargo does not count");
            Equal(ItemTypes.derrick, ship.Modules[1].ItemStored, "unsupported item preserved");
            ship.ShipState = Ship_States.Docked;
            ship.Modules[1].ItemStored = ItemTypes.iron;
            ship.Modules[1].ItemCount = 0;
            Press(ActiveScreen<ShipInterior>(), "Modules/01");
            await InputFrames();
            Equal(false, OverlayManager.Instance.IsOpen, "empty cargo is not an offer");
            Equal(5, Save.MethanoidTradeCount, "empty cargo does not count");
            Equal(100, ship.Fuel, "ineligible cargo gives no fuel gift");
        });

        private async Task TradeAbandon() => await WithTrade(async (ship, interior) =>
        {
            await OpenTradeChoice(interior);
            using var cancel = new InputEventAction { Action = "ui_cancel", Pressed = true };
            OverlayManager.Instance._UnhandledInput(cancel);
            await InputFrames();
            Equal(5, Save.MethanoidTradeCount, "closing is not an explicit decision");
            Equal(ItemTypes.iron, ship.Modules[1].ItemStored, "closing preserves cargo");
            Equal(Ship_States.Docked, ship.ShipState, "abandoned choice does not launch ship");
            await OpenTradeChoice(interior);
        });

        private async Task TradeSceneExit() => await WithTrade(async (ship, interior) =>
        {
            await OpenTradeChoice(interior);
            GameCore.SingletonInstance.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await InputFrames();
            Equal(false, OverlayManager.Instance.IsOpen, "scene exit removes its decision");
            Equal(false, GetTree().Paused, "scene exit restores processing");
            Equal(5, Save.MethanoidTradeCount, "detached scene cannot settle trade");
            Equal(100, ship.Fuel, "detached scene gives no fuel gift");
            Equal(ItemTypes.iron, ship.Modules[1].ItemStored, "detached scene leaves cargo intact");
        });

        private async Task TradeWarBoundary(int count) => await WithTrade(async (ship, interior) =>
        {
            Save.MethanoidTradeCount = count;
            Press(interior, "Modules/01");
            await InputFrames();
            Equal(true, Save.AtWar, "unsigned threshold includes counts above sixteen");
            Equal(false, OverlayManager.Instance.IsOpen, "war entry offers no exchange");
            Equal(ItemTypes.iron, ship.Modules[1].ItemStored, "war leaves cargo unchanged");
            Equal(UnknownItemTypes.Blazer, ((UnknownItem)ship.Modules[0].HeldItem).ItemType, "existing war gift delivered");
        });

        private async Task TradeAfterWar() => await WithTrade(async (ship, interior) =>
        {
            Save.AtWar = true;
            Save.MethanoidTradeCount = 16;
            Press(interior, "Modules/01");
            await InputFrames();
            Equal(ItemTypes.commspod, ship.Modules[0].ItemStored, "war gift is not repeated");
            Equal(null, ship.Modules[0].HeldItem, "no duplicate prototype");
            Equal(16, Save.MethanoidTradeCount, "no peaceful count change");
            Equal(false, OverlayManager.Instance.IsOpen, "no wartime offer");
        });

        private async Task TradeTypingExit() => await WithTrade(async (ship, interior) =>
        {
            Save.BaseGameData.ModuleFrameTexts[ModuleFrameText.Methanoid_TradeQuestion].Lines =
                new List<Line> { new Line("Trading question still typing", Colors.White, false, false) };
            Press(interior, "Modules/01");
            await InputFrames();
            Equal(true, GameCore.SingletonInstance.GetNode<InputBlocker>("InputBlocker").Blocked, "typing owns a lock");
            GameCore.SingletonInstance.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await ToSignal(GetTree().CreateTimer(0.3), SceneTreeTimer.SignalName.Timeout);
            Equal(false, GameCore.SingletonInstance.GetNode<InputBlocker>("InputBlocker").Blocked, "cancelled typing releases its lock");
            Equal(5, Save.MethanoidTradeCount, "cancelled text does not settle trade");
            Equal(false, OverlayManager.Instance.IsOpen, "cancelled text cannot open a late choice");
        });

        private async Task TradePhysicalInputRoutes() => await WithTrade(async (ship, interior) =>
        {
            var dialog = await OpenTradeChoice(interior);
            var point = dialog.GetNode<Button>("Accept").GetGlobalRect().GetCenter();
            var motion = new InputEventMouseMotion { Position = point, GlobalPosition = point };
            GetViewport().PushInput(motion, true);
            foreach (var pressed in new[] { true, false })
            {
                var click = new InputEventMouseButton { Position = point, GlobalPosition = point,
                    ButtonIndex = MouseButton.Left, Pressed = pressed };
                GetViewport().PushInput(click, true);
            }
            await InputFrames();
            Equal(6, Save.MethanoidTradeCount, "pointer accepts visible control");
            ship.ShipState = Ship_States.Docked;
            dialog = await OpenTradeChoice(ActiveScreen<ShipInterior>());
            dialog.GetNode<Button>("Decline").GrabFocus();
            foreach (var pressed in new[] { true, false })
            {
                using var key = new InputEventKey { Keycode = Key.Enter, Pressed = pressed };
                GetViewport().PushInput(key, true);
            }
            await InputFrames();
            Equal(5, Save.MethanoidTradeCount, "keyboard declines focused control");
            Equal(ItemTypes.silica, ship.Modules[1].ItemStored, "declining second visit preserves accepted cargo");
        });

        private async Task TradeResponseExit() => await WithTrade(async (ship, interior) =>
        {
            foreach (var key in new[] { ModuleFrameText.Methanoid_Trade1, ModuleFrameText.Methanoid_Trade2 })
                Save.BaseGameData.ModuleFrameTexts[key].Lines = new List<Line>
                    { new Line("Your cargo has been exchanged", Colors.White, false, false) };
            var dialog = await OpenTradeChoice(interior);
            Press(dialog, "Accept");
            Equal(6, Save.MethanoidTradeCount, "consent commits count before response");
            Equal(250, ship.Fuel, "fuel settles with cargo before response playback");
            Equal(ItemTypes.silica, ship.Modules[1].ItemStored, "consent commits cargo before response");
            GameCore.SingletonInstance.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await ToSignal(GetTree().CreateTimer(0.3), SceneTreeTimer.SignalName.Timeout);
            Equal(6, Save.MethanoidTradeCount, "interruption does not count twice");
            Equal(250, ship.Fuel, "interruption preserves accepted fuel");
            Equal(123, ship.Modules[1].ItemCount, "interruption preserves exact cargo amount");
            Equal(false, GameCore.SingletonInstance.GetNode<InputBlocker>("InputBlocker").Blocked, "response interruption releases lock");
        });

        private async Task TradeSixteenthAcceptance() => await WithTrade(async (ship, interior) =>
        {
            Save.MethanoidTradeCount = 15;
            var dialog = await OpenTradeChoice(interior);
            Press(dialog, "Accept");
            await InputFrames();
            Equal(16, Save.MethanoidTradeCount, "sixteenth exchange counted");
            Equal(false, Save.AtWar, "accepted trade finishes before next entry gate");
            ship.ShipState = Ship_States.Docked;
            Press(ActiveScreen<ShipInterior>(), "Modules/01");
            await InputFrames();
            Equal(true, Save.AtWar, "next encounter enters war");
            Equal(ItemTypes.silica, ship.Modules[1].ItemStored, "war does not exchange again");
        });

        private async Task TradeStaleCargo() => await WithTrade(async (ship, interior) =>
        {
            var dialog = await OpenTradeChoice(interior);
            ship.Modules[1].ItemStored = ItemTypes.gold;
            ship.Modules[1].ItemCount = 17;
            Press(dialog, "Accept");
            await InputFrames();
            Equal(ItemTypes.gold, ship.Modules[1].ItemStored, "stale snapshot cannot overwrite new goods");
            Equal(17, ship.Modules[1].ItemCount, "changed quantity preserved");
            Equal(5, Save.MethanoidTradeCount, "invalidated offer is not an acceptance");
            Equal(100, ship.Fuel, "stale offer gives no fuel gift");
            Equal(false, OverlayManager.Instance.IsOpen, "invalidated decision closes");
        });
        private async Task ModuleDialogueColours()
        {
            InitializeUi();
            var frame = GD.Load<PackedScene>("res://PreFabs/ShipModuleWindows/ModuleTextFrame.tscn")
                .Instantiate<Deuteros.Code.Platform.Screens.ModuleScenes.ModuleTextFrame>();
            AddChild(frame);
            try
            {
                var intro = Save.BaseGameData.ModuleFrameTexts[ModuleFrameText.Methanoid_Intro].Lines[0];
                var text = new TextFrame(ModuleFrameText.Methanoid_Intro);
                text.Lines.Add(new Line(intro.Text, intro.TextColor, false, true, 0));
                text.Lines.Add(new Line("Cargo {0}", Colors.White, false, true, 0));
                await frame.PlayText(text, new List<string> { "7" }, 2, 0, 0);
                Equal("[color=#aaccee]" + intro.Text + "[/color]\r\nCargo 7\r\n", frame.Text.Text,
                    "alien dialogue keeps its light blue instead of becoming black");
                Equal(intro.Text + "\r\nCargo 7\r\n", frame.Text.GetParsedText(), "formatting does not leak into visible dialogue");
                Equal("2", frame.WindowNumber.Text, "window retains the selected pod number");
                await InputFrames();
                await CaptureDisplayEvidence("module-dialogue-colours");
                foreach (var (colour, expected) in new[] { (Save.BaseGameData.Red, "ff0000"),
                    (Save.BaseGameData.Green, "008800"), (Save.BaseGameData.Blue, "002288"),
                    (Save.BaseGameData.Yellow, "ffff00"), (Save.BaseGameData.Beige, "99aa77") })
                    Equal("[color=#" + expected + "]7[/color]\r\n", new Line("{0}", colour, false).GetText(new List<string> { "7" }),
                        "shared colour conversion preserves channel bytes");
            }
            finally
            {
                frame.QueueFree();
                await InputFrames();
                await DrainStoppedAudio();
            }
        }
    }
}
