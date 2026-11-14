using Deuteros.Code.Objects;
using Deuteros.Code.Platform;
using Deuteros.Code.Platform.Screens.ShipBayScenes;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        public void RunPaletteRegressions()
        {
            CheckUi("Production staff roster uses the original dark blue", () =>
            {
                var roster = OpenUi<StaffList>("res://PreFabs/StaffList.tscn");
                try
                {
                    roster.UpdateStaff(new[] { new Staff { Type = StaffType.Production, Leader = "TEST", Count = 20 }, null, null, null });
                    Equal(new Color("#002288"), roster.GetNode<ColorRect>("Staff/Backgrounds/01").Color, "roster production colour");
                }
                finally { roster.Free(); }
            });
            CheckUi("Production cryopod text uses dark blue and marine text stays red", () =>
            {
                var torso = OpenUi<Torso>("res://Screens/Ships/Torso.tscn");
                try
                {
                    var staff = new Staff { Type = StaffType.Production, Leader = "TEST", Count = 20 };
                    torso.ChangeModule(new ShipModule { ModuleType = Module_Types.Cryo, StaffStored = staff });
                    Equal(new Color("#002288"), torso.Contents.GetThemeColor("font_color"), "cryopod production colour");
                    staff.Type = StaffType.Marines;
                    torso.UpdateState();
                    Equal(new Color("#ff0000"), torso.Contents.GetThemeColor("font_color"), "cryopod marine colour");
                }
                finally { torso.Free(); }
            });
            Check("Shared UI colours preserve their declared RGB values", () =>
            {
                var data = Save.BaseGameData;
                Equal(new Color("#ff0000"), data.Red, "red");
                Equal(new Color("#008800"), data.Green, "green");
                Equal(new Color("#99aa77"), data.Beige, "beige");
                Equal(new Color("#556633"), data.Dark_Beige, "dark beige");
                Equal(new Color("#ffff00"), data.Yellow, "yellow");
                Equal(new Color("#aaccee"), data.LightBlue, "light blue");
            });
        }
    }
}
