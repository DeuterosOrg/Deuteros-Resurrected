using Settings = Deuteros.UI.Settings.SettingsScreen;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Utility;
using Godot;
using static Deuteros.Code.Enums;
using StoresView = Deuteros.Code.Platform.Screens.Store;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        public async Task RunSettingsPresetRegressions()
        {
            foreach (var choice in new[] { 1, 2, 3, 4, 5 })
                await CheckAsync($"Progression preset {choice} refreshes its view and repeat is a no-op", () => SettingsPresetRepeat(choice));
        }

        private void ConfirmSettingsPreset(Settings screen, int choice)
        {
            screen.DebugTab.ButtonPressed = true;
            var rows = new[] { screen.SkipToShuttlesRow, screen.EarthStationTo7Row, screen.EarthOrbitProductionRow,
                screen.IOSModulesReadyRow, screen.ActivateMTXRow, screen.BuildTitanStationRow };
            Press(rows[choice], "%ActionButton");
            Press(screen, "%ConfirmApplyButton");
        }

        private async Task SettingsPresetRepeat(int choice)
        {
            await WithSettings(async screen =>
            {
                // Settings is already open; establish the real underlying scene without audio.
                // The test does not navigate after applying; the preset must refresh the resumed view.
                var scene = choice == 3 || choice == 4 ? Scenes.Store : Scenes.Overview;
                GameCore.SingletonInstance.ChangeScene(scene, scene == Scenes.Store
                    ? new List<SceneVariables> { SceneVariables.Orbit } : new List<SceneVariables>());
                if (choice == 4)
                {
                    // Keep the real bulletin trigger/renderer but shorten this fixture's message.
                    // Fresh-game absence of a researcher is intentional: presets must handle it.
                    Save.BaseGameData.BulletinTexts[BulletinTypes.Matter_Transmitter].BulletinText = "MTX ready.";
                }
                ConfirmSettingsPreset(screen, choice);
                await InputFrames();
                Equal(false, OverlayManager.Instance.IsOpen, "confirmed preset closes settings");
                Equal(false, GetTree().Paused, "confirmed preset resumes game");

                if (choice == 1 || choice == 2 || choice == 5)
                {
                    var planet = Save.BaseGameData.Planets[choice == 5 ? StellarBodies.titan : StellarBodies.earth];
                    Equal(choice == 1 ? 7 : 8, planet.Station.BuildParts, "requested station progress");
                    Equal(choice != 1, planet.Station.Built, "requested station completion");
                    if (choice != 1) Equal(true, planet.Station.Factory.AOC, "automated orbital factory");
                    var overview = ActiveRecipeScreen<global::Overview>();
                    var station = overview.GetNode<Node>("Stations").GetChildren()
                        .SelectMany(column => column.GetChildren()).OfType<TextureButton>()
                        .SingleOrDefault(button => button.Visible && button.HasMeta("planetid")
                            && button.GetMeta("planetid").AsInt32() == (int)planet.PlanetId);
                    Equal(true, station != null, "existing overview immediately shows new station");
                    Equal(choice == 1 ? "Station_UnderConstruction.png" : "Station_0.png",
                        System.IO.Path.GetFileName(station.TextureNormal.ResourcePath), "station icon matches preset completion");
                }
                else if (choice == 3)
                {
                    Equal(true, Save.Unlocks.Contains(Game_Unlocks.IOS_Attachments), "IOS modules unlocked");
                    var stores = ActiveRecipeScreen<StoresView>();
                    foreach (var type in new[] { ItemTypes.a__m__a, ItemTypes.a__o__c, ItemTypes.bandaid, ItemTypes.grapple, ItemTypes.r_frame })
                        Equal(true, stores.Buttons.Any(button => button.ObjectData?.ItemType == type), "existing stores exposes " + type);
                }
                else
                {
                    Equal(true, Save.Unlocks.Contains(Game_Unlocks.Mass_Tranceiver), "MTX unlocked");
                    Equal(true, GameCore.Earth.Station.MtxInstalled, "MTX installed on Earth station");
                    if (GameCore.SingletonInstance.currentScene == Scenes.Bulletins)
                    {
                        var bulletin = ActiveRecipeScreen<global::Bulletins>();
                        Equal(true, bulletin.GetNode<RichTextLabel>("Labels/BulletinLabel").Text.Length > 0,
                            "fresh-game discovery bulletin renders without a researcher");
                        bulletin.LetterDelayMs = 0;
                        var blocker = GameCore.SingletonInstance.GetNode<InputBlocker>("GameContainer/GameViewport/InputBlocker");
                        for (var frame = 0; frame < 300 && blocker.Blocked; frame++)
                            await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                        Equal(false, blocker.Blocked, "discovery bulletin completes and releases input");
                        GameCore.SingletonInstance.ChangeScene(Scenes.Store, new List<SceneVariables> { SceneVariables.Orbit });
                        await InputFrames();
                    }
                    var stores = ActiveRecipeScreen<StoresView>();
                    stores.SwitchStoreType.EmitSignal(BaseButton.SignalName.ButtonUp);
                    Equal(true, stores.MTX.Visible, "MTX controls usable after preset");
                    stores.MTX.GetNode<Button>("Config/Buttons/SwitchStore").EmitSignal(BaseButton.SignalName.Pressed);
                }

                // Compare the preset's mutations while holding real-time simulation still.
                // Frames needed to close the overlay otherwise advance the saved SDM timer phase.
                var core = GameCore.SingletonInstance;
                var wasProcessing = core.IsProcessing();
                core.SetProcess(false);
                try
                {
                    var beforeRepeat = SaveStorage.Serialize(Save);
                    screen = OpenSettings();
                    ConfirmSettingsPreset(screen, choice);
                    await InputFrames();
                    Equal(beforeRepeat, SaveStorage.Serialize(Save), "repeat does not duplicate ships, teams, research, unlocks or stock");
                }
                finally { core.SetProcess(wasProcessing); }
                Equal(false, OverlayManager.Instance.IsOpen, "repeat closes settings");
                Equal(false, GetTree().Paused, "repeat restores input");
            });
        }
    }
}
