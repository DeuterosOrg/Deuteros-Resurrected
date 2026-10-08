using System;
using System.Collections.Generic;
using Deuteros.Code.Objects.GameData;
using Deuteros.Code.Utility;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Code
{
    public partial class GameCore
    {
        public SaveStorage Storage { get; set; } = new SaveStorage(ProjectSettings.GlobalizePath("user://saves"));
        private SaveFile autosaveWorld;
        private int autosaveInterval;
        private double autosaveElapsed;

        private void UpdateAutosave(double delta)
        {
            var interval = Platform.Helpers.SettingsManager.Instance.GetSetting("gameplay/autosave").AsString() switch
            {
                "5 minutes" => 300, "10 minutes" => 600, "15 minutes" => 900, _ => 0
            };
            var save = GameData.ActiveSaveFile;
            if (autosaveWorld != save || autosaveInterval != interval)
            {
                autosaveWorld = save;
                autosaveInterval = interval;
                autosaveElapsed = 0;
            }
            if (interval == 0 || !double.IsFinite(delta) || delta <= 0 || StoryDisplayBlocked()
                || currentScene == Scenes.SaveScreen) return;
            autosaveElapsed += delta;
            if (autosaveElapsed < interval) return;
            autosaveElapsed = 0;
            try
            {
                Storage.Write(SaveStorage.AutoSlot, save);
                HoverText = "Autosaved.";
                HoverTextIsStatus = true;
            }
            catch (Exception error) when (SaveStorage.IsSaveError(error))
            {
                ShowError(_currentScreen as Platform.Base.BaseSubScene, "Autosave failed; previous save kept.");
            }
        }

        private void QuickSave()
        {
            try
            {
                Storage.Write(SaveStorage.QuickSlot, GameData.ActiveSaveFile);
                if (_currentScreen is Platform.Screens.SaveScreen screen) screen.RefreshSlots();
                HoverText = "Quick saved.";
                HoverTextIsStatus = true;
            }
            catch (Exception error) when (SaveStorage.IsSaveError(error))
            {
                ShowError(_currentScreen as Platform.Base.BaseSubScene, "Quick save failed; previous save kept.");
            }
        }

        public void LoadSavedGame(SaveFile save)
        {
            // Parse and validate the entire file before calling this world-switch boundary.
            SaveStorage.Validate(save);
            save.TimeSkip = save.TimeSkipDay = false;
            save.TimeSkipStart = 0;
            GameData.ActiveSaveFile = save;
            HoverText = "";
            // Recreate the current view so it cannot retain references into the old world.
            ChangeScene(Scenes.Overview, new List<SceneVariables>());
            _menuScreen.DayTick(save.CurrentDay, save.CurrentDay);
            _menuScreen.UpdateAnimations();
        }
    }
}
