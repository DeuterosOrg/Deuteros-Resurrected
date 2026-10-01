using System.Collections.Generic;
using Deuteros.Code.Objects.GameData;
using Deuteros.Code.Utility;
using static Deuteros.Code.Enums;

namespace Deuteros.Code
{
    public partial class GameCore
    {
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
