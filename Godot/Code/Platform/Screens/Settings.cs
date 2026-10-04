using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Helpers;
using Godot;
using System;
using System.Linq;
using static Deuteros.Code.Enums;

public partial class Settings : Control
{
    private GameConfig Preferences => GameCore.SingletonInstance.Config;
    private Control preferencesPage;
    private Control cheatsPage;
    private Button sound;
    private Button fullscreen;
    private Button infinite;
    private HSlider volume;
    private Label volumeText;
    private Label status;
    private Label confirmText;
    private OptionButton windowScale;
    private OptionButton preset;
    private Button confirmPreset;
    private Button cancelPreset;
    private bool refreshing;

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(288, 180);
        AddChild(new ColorRect { Name = "Background", Size = CustomMinimumSize,
            Color = new Color("#171b17"), MouseFilter = MouseFilterEnum.Stop });
        Label(this, "Title", "SETTINGS", 12, 6, 264);
        Button(this, "SettingsTab", "Audio / display", 12, 22, 158, () => ShowPage(false));
        Button(this, "CheatsTab", "Cheats", 177, 22, 99, () => ShowPage(true));
        preferencesPage = new Control { Name = "Preferences" };
        cheatsPage = new Control { Name = "Cheats" };
        AddChild(preferencesPage);
        AddChild(cheatsPage);

        Label(preferencesPage, "SoundLabel", "Sound", 12, 47, 100);
        sound = Button(preferencesPage, "Sound", "", 190, 44, 86, () =>
        {
            Preferences.SoundEnabled = !Preferences.SoundEnabled;
            ApplyPreferences();
        });
        Label(preferencesPage, "VolumeLabel", "Volume", 12, 68, 76);
        volume = new HSlider { Name = "Volume", Position = new Vector2(94, 66), Size = new Vector2(133, 14),
            MinValue = 0, MaxValue = 100, Step = 5 };
        preferencesPage.AddChild(volume);
        volumeText = Label(preferencesPage, "VolumeText", "", 237, 68, 40);
        volume.ValueChanged += value =>
        {
            if (refreshing) return;
            Preferences.Volume = (int)value;
            ApplyPreferences();
        };
        Label(preferencesPage, "SizeLabel", "Window size", 12, 92, 140);
        windowScale = new OptionButton { Name = "WindowScale", Position = new Vector2(174, 87), Size = new Vector2(102, 18) };
        for (var scale = 2; scale <= 4; scale++) windowScale.AddItem($"{320 * scale}x{200 * scale}", scale);
        preferencesPage.AddChild(windowScale);
        windowScale.ItemSelected += index =>
        {
            Preferences.WindowScale = windowScale.GetItemId((int)index);
            ApplyPreferences(true);
        };
        Label(preferencesPage, "FullscreenLabel", "Fullscreen", 12, 115, 132);
        fullscreen = Button(preferencesPage, "Fullscreen", "", 190, 110, 86, () =>
        {
            Preferences.Fullscreen = !Preferences.Fullscreen;
            ApplyPreferences(true);
        });
        Button(preferencesPage, "Defaults", "Restore defaults", 12, 134, 180, () =>
        {
            Preferences.ResetPreferences();
            ApplyPreferences(true);
        });

        Label(cheatsPage, "SessionLabel", "Session cheats", 12, 47, 240);
        infinite = Button(cheatsPage, "InfiniteResources", "", 12, 64, 264, () =>
        {
            GameCore.SingletonInstance.InfiniteResources = !GameCore.SingletonInstance.InfiniteResources;
            RefreshControls();
        });
        preset = new OptionButton { Name = "Preset", Position = new Vector2(12, 88), Size = new Vector2(264, 18) };
        foreach (var name in new[] { "Shuttle setup", "7 Earth station parts", "Orbital production", "IOS modules", "Enable MTX", "Titan station" }) preset.AddItem(name);
        cheatsPage.AddChild(preset);
        Button(cheatsPage, "ApplyPreset", "Apply progression preset", 12, 112, 264, () =>
        {
            GetNode<Button>("Resume").Hide();
            status.Text = "";
            confirmText.Text = "Change progress? Save first.";
            confirmPreset.Show();
            cancelPreset.Show();
        });

        confirmText = Label(this, "Confirmation", "", 12, 134, 264);
        confirmPreset = Button(this, "ConfirmPreset", "Apply", 126, 156, 70, ApplyPreset);
        cancelPreset = Button(this, "CancelPreset", "Cancel", 202, 156, 74, ClearConfirmation);
        status = Label(this, "Status", "", 12, 156, 184);
        Button(this, "Resume", "Resume", 202, 156, 74, () => OverlayManager.Instance.CloseOverlay());
        RefreshControls();
        ShowPage(false);
    }

    private static Label Label(Node parent, string name, string text, float x, float y, float width)
    {
        var label = new Label { Name = name, Text = text, Position = new Vector2(x, y), Size = new Vector2(width, 12),
            ClipText = true, MouseFilter = MouseFilterEnum.Ignore };
        parent.AddChild(label);
        return label;
    }

    private static Button Button(Node parent, string name, string text, float x, float y, float width, Action action)
    {
        var button = new Button { Name = name, Text = text, Position = new Vector2(x, y), Size = new Vector2(width, 18) };
        button.Pressed += action;
        parent.AddChild(button);
        return button;
    }

    private void ShowPage(bool cheats)
    {
        preferencesPage.Visible = !cheats;
        cheatsPage.Visible = cheats;
        ClearConfirmation();
        status.Text = "";
    }

    private void ClearConfirmation()
    {
        confirmText.Text = "";
        confirmPreset.Hide();
        cancelPreset.Hide();
        GetNode<Button>("Resume").Show();
    }

    private void RefreshControls()
    {
        refreshing = true;
        sound.Text = Preferences.SoundEnabled ? "On" : "Off";
        volume.Value = Preferences.Volume;
        volumeText.Text = Preferences.Volume + "%";
        windowScale.Select(Preferences.WindowScale - 2);
        windowScale.Disabled = Preferences.Fullscreen;
        fullscreen.Text = Preferences.Fullscreen ? "On" : "Off";
        infinite.Text = "Infinite supplies: " + (GameCore.SingletonInstance.InfiniteResources ? "On" : "Off");
        refreshing = false;
    }

    private void ApplyPreferences(bool display = false)
    {
        Preferences.ApplyAudio();
        if (display) Preferences.ApplyDisplay();
        status.Text = Preferences.Save() == Godot.Error.Ok ? "Settings saved." : "Applied; not saved.";
        RefreshControls();
    }

    private void ApplyPreset()
    {
        // These existing shortcuts can create a marine team; preflight before any other mutation.
        var earth = GameCore.Earth;
        var mayCreateCrew = preset.Selected <= 3 || (preset.Selected == 4 && !earth.Station.Built
            && earth.Station.BuildParts < 7 && !GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Contains(Game_Unlocks.Shuttle_Unlock));
        if (mayCreateCrew && !earth.PlanetResources.Staff.Any(team => team == null || team.Type == StaffType.Marines))
        {
            ClearConfirmation();
            status.Text = "Free a crew slot first.";
            return;
        }
        var actions = new Action[] { SkipToShuttles_Pressed, EarthStationTo7_Pressed, ProdInEarthOrbit_Pressed,
            IOSModulesReady_Pressed, ActivateMTX_Pressed, BuildTitanStation_Pressed };
        var core = GameCore.SingletonInstance;
        var previousScene = core.currentScene;
        actions[preset.Selected]();
        // These shortcuts change several systems at once without advancing a simulation day.
        // Rebuild the current view, but preserve any discovery bulletin opened by the preset.
        if (core.currentScene == previousScene && core.currentScene != Scenes.Bulletins)
            core.ChangeScene(core.currentScene, new(core.SceneVariables));
        OverlayManager.Instance.CloseOverlay();
    }
}
