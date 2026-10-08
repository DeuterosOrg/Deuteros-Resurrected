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
