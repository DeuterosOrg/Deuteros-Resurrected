using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.UI.Rows;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Deuteros.Code.Enums;

namespace Deuteros.UI.Settings;

public partial class SettingsScreen : Control
{
    // Header & categories
    public Button CloseButton { get; private set; }
    public Label TitleLabel { get; private set; }
    public Button DisplayTab { get; private set; }
    public Button AudioTab { get; private set; }
    public Button ControlsTab { get; private set; }
    public Button GameplayTab { get; private set; }
    public Button ModernTab { get; private set; }
    public Button DebugTab { get; private set; }
    public ScrollContainer SettingsScroll { get; private set; }

    // Display
    public VBoxContainer DisplayList { get; private set; }
    public CycleSettingRow ResolutionRow { get; private set; }
    public CycleSettingRow WindowModeRow { get; private set; }
    public CycleSettingRow PixelScalingRow { get; private set; }
    public SliderSettingRow ScanlinesRow { get; private set; }
    public CycleSettingRow InterfaceScaleRow { get; private set; }
    public ToggleSettingRow VSyncRow { get; private set; }
    public CycleSettingRow FrameLimitRow { get; private set; }

    // Audio
    public VBoxContainer AudioList { get; private set; }
    public SliderSettingRow MasterVolumeRow { get; private set; }
    public SliderSettingRow MusicVolumeRow { get; private set; }
    public SliderSettingRow EffectsVolumeRow { get; private set; }
    public SliderSettingRow InterfaceVolumeRow { get; private set; }
    public ToggleSettingRow ClassicAudioRow { get; private set; }
    public ToggleSettingRow MuteUnfocusedRow { get; private set; }

    // Controls
    public VBoxContainer ControlsList { get; private set; }
    public KeybindSettingRow PauseKeyRow { get; private set; }
    public KeybindSettingRow SpeedUpKeyRow { get; private set; }
    public KeybindSettingRow SlowDownKeyRow { get; private set; }
    public KeybindSettingRow NextLocationKeyRow { get; private set; }
    public KeybindSettingRow ResearchKeyRow { get; private set; }
    public KeybindSettingRow ProductionKeyRow { get; private set; }
    public KeybindSettingRow QuickSaveKeyRow { get; private set; }
    public ToggleSettingRow EdgeScrollRow { get; private set; }
    public SliderSettingRow PointerSpeedRow { get; private set; }

    // Gameplay
    public VBoxContainer GameplayList { get; private set; }
    public CycleSettingRow GameSpeedRow { get; private set; }
    public ToggleSettingRow AutoPauseRow { get; private set; }
    public CycleSettingRow EventAlertsRow { get; private set; }
    public CycleSettingRow AutosaveRow { get; private set; }
    public ToggleSettingRow TooltipsRow { get; private set; }
    public ToggleSettingRow ConfirmLaunchRow { get; private set; }

    // Modern
    public VBoxContainer ModernList { get; private set; }
    public ToggleSettingRow BulletinSkipRow { get; private set; }
    public VBoxContainer DebugList { get; private set; }

    // Readout & footer
    public Label SelectedLabel { get; private set; }
    public Label DescriptionLabel { get; private set; }
    public Label CurrentValue { get; private set; }
    public Label DefaultValue { get; private set; }
    public ColorRect StatusLight { get; private set; }
    public Label StatusLabel { get; private set; }
    public Label CategoryLabel { get; private set; }
    public Button RestoreButton { get; private set; }
    public Button CancelButton { get; private set; }
    public Button ApplyButton { get; private set; }
    public Control ConfirmOverlay { get; private set; }
    public Button ConfirmKeepButton { get; private set; }
    public Button ConfirmDiscardButton { get; private set; }
    public Button ConfirmApplyButton { get; private set; }

    private const string PendingStatusText = "Unsaved Changes";
    private static readonly Color PendingStatusColour = new Color(0.9098f, 0.8157f, 0.251f);

    private readonly Dictionary<Button, (VBoxContainer List, string Caption)> _categories = new Dictionary<Button, (VBoxContainer List, string Caption)>();
    private readonly List<SettingRow> _rows = new List<SettingRow>();
    private readonly Dictionary<SettingRow, Variant> _defaults = new Dictionary<SettingRow, Variant>();
    private readonly Dictionary<SettingRow, Variant> _applied = new Dictionary<SettingRow, Variant>();
    private int pendingPreset = -1;
    private Button _currentTab;
    private SettingRow _selectedRow;
    private string _appliedStatusText;
    private Color _appliedStatusColour;

    public override void _Ready()
    {
        // Header & categories
        CloseButton = GetNode<Button>("%CloseButton");
        TitleLabel = GetNode<Label>("%TitleLabel");
        DisplayTab = GetNode<Button>("%DisplayTab");
        AudioTab = GetNode<Button>("%AudioTab");
        ControlsTab = GetNode<Button>("%ControlsTab");
        GameplayTab = GetNode<Button>("%GameplayTab");
        ModernTab = GetNode<Button>("%ModernTab");
        DebugTab = GetNode<Button>("%DebugTab");
        SettingsScroll = GetNode<ScrollContainer>("%SettingsScroll");

        // Display
        DisplayList = GetNode<VBoxContainer>("%DisplayList");
        ResolutionRow = GetNode<CycleSettingRow>("%ResolutionRow");
        WindowModeRow = GetNode<CycleSettingRow>("%WindowModeRow");
        PixelScalingRow = GetNode<CycleSettingRow>("%PixelScalingRow");
        ScanlinesRow = GetNode<SliderSettingRow>("%ScanlinesRow");
        InterfaceScaleRow = GetNode<CycleSettingRow>("%InterfaceScaleRow");
        VSyncRow = GetNode<ToggleSettingRow>("%VSyncRow");
        FrameLimitRow = GetNode<CycleSettingRow>("%FrameLimitRow");

        // Audio
        AudioList = GetNode<VBoxContainer>("%AudioList");
        MasterVolumeRow = GetNode<SliderSettingRow>("%MasterVolumeRow");
        MusicVolumeRow = GetNode<SliderSettingRow>("%MusicVolumeRow");
        EffectsVolumeRow = GetNode<SliderSettingRow>("%EffectsVolumeRow");
        InterfaceVolumeRow = GetNode<SliderSettingRow>("%InterfaceVolumeRow");
        ClassicAudioRow = GetNode<ToggleSettingRow>("%ClassicAudioRow");
        MuteUnfocusedRow = GetNode<ToggleSettingRow>("%MuteUnfocusedRow");

        // Controls
        ControlsList = GetNode<VBoxContainer>("%ControlsList");
        PauseKeyRow = GetNode<KeybindSettingRow>("%PauseKeyRow");
        SpeedUpKeyRow = GetNode<KeybindSettingRow>("%SpeedUpKeyRow");
        SlowDownKeyRow = GetNode<KeybindSettingRow>("%SlowDownKeyRow");
        NextLocationKeyRow = GetNode<KeybindSettingRow>("%NextLocationKeyRow");
        ResearchKeyRow = GetNode<KeybindSettingRow>("%ResearchKeyRow");
        ProductionKeyRow = GetNode<KeybindSettingRow>("%ProductionKeyRow");
        QuickSaveKeyRow = GetNode<KeybindSettingRow>("%QuickSaveKeyRow");
        EdgeScrollRow = GetNode<ToggleSettingRow>("%EdgeScrollRow");
        PointerSpeedRow = GetNode<SliderSettingRow>("%PointerSpeedRow");

        // Gameplay
        GameplayList = GetNode<VBoxContainer>("%GameplayList");
        GameSpeedRow = GetNode<CycleSettingRow>("%GameSpeedRow");
        AutoPauseRow = GetNode<ToggleSettingRow>("%AutoPauseRow");
        EventAlertsRow = GetNode<CycleSettingRow>("%EventAlertsRow");
        AutosaveRow = GetNode<CycleSettingRow>("%AutosaveRow");
        TooltipsRow = GetNode<ToggleSettingRow>("%TooltipsRow");
        ConfirmLaunchRow = GetNode<ToggleSettingRow>("%ConfirmLaunchRow");

        // Modern
        ModernList = GetNode<VBoxContainer>("%ModernList");
        BulletinSkipRow = GetNode<ToggleSettingRow>("%BulletinSkipRow");
        DebugList = GetNode<VBoxContainer>("%DebugList");

        // Readout & footer
        SelectedLabel = GetNode<Label>("%SelectedLabel");
        DescriptionLabel = GetNode<Label>("%DescriptionLabel");
        CurrentValue = GetNode<Label>("%CurrentValue");
        DefaultValue = GetNode<Label>("%DefaultValue");
        StatusLight = GetNode<ColorRect>("%StatusLight");
        StatusLabel = GetNode<Label>("%StatusLabel");
        CategoryLabel = GetNode<Label>("%CategoryLabel");
        RestoreButton = GetNode<Button>("%RestoreButton");
        CancelButton = GetNode<Button>("%CancelButton");
        ApplyButton = GetNode<Button>("%ApplyButton");
        ConfirmOverlay = GetNode<Control>("%ConfirmOverlay");
        ConfirmKeepButton = GetNode<Button>("%ConfirmKeepButton");
        ConfirmDiscardButton = GetNode<Button>("%ConfirmDiscardButton");
        ConfirmApplyButton = GetNode<Button>("%ConfirmApplyButton");

        _appliedStatusText = StatusLabel.Text;
        _appliedStatusColour = StatusLight.Color;

        SetUpCategories();
        SetUpRows();
        SetUpButtons();
        SetUpCheats();

        DisplayTab.ButtonPressed = true;
        ShowCategory(DisplayTab);
        UpdateStatus();
        CallDeferred(MethodName.FocusSelectedRow);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!@event.IsActionPressed("ui_cancel")) return;

        GetViewport().SetInputAsHandled();

        if (ConfirmOverlay.Visible)
            HideConfirm();
        else
            RequestClose();
    }

    private void SetUpCategories()
    {
        _categories[DisplayTab] = (DisplayList, CategoryLabel.Text);
        _categories[AudioTab] = (AudioList, "Sound & Music");
        _categories[ControlsTab] = (ControlsList, "Controls & Keys");
        _categories[GameplayTab] = (GameplayList, "Gameplay");
        _categories[ModernTab] = (ModernList, "Modernisations");
        _categories[DebugTab] = (DebugList, "Debug Tools");

        DebugTab.Visible = OS.IsDebugBuild();

        foreach (var tab in _categories.Keys)
            tab.Toggled += toggledOn => OnTabToggled(tab, toggledOn);
    }

    private void SetUpRows()
    {
        var settings = SettingsManager.Instance;
        var rowGroup = new ButtonGroup();

        foreach (var row in _categories.Values.SelectMany(category => category.List.GetChildren().OfType<SettingRow>()))
        {
            _rows.Add(row);

            row.LabelButton.ButtonGroup = rowGroup;
            row.LabelButton.Toggled += toggledOn => OnRowToggled(row, toggledOn);
            row.LabelButton.FocusEntered += () => OnRowFocused(row);
            row.LabelButton.GuiInput += inputEvent => OnRowGuiInput(row, inputEvent);
            row.ValueChanged += () => OnRowValueChanged(row);

            if (string.IsNullOrEmpty(row.SettingKey))
                continue;

            if (row is CycleSettingRow cycle && settings.GetOptions(row.SettingKey) is string[] options)
                cycle.Options = options;

            var inspectorValue = row.SettingValue;
            _defaults[row] = settings.GetDefault(row.SettingKey, inspectorValue);
            row.SettingValue = settings.GetSetting(row.SettingKey, inspectorValue);

            if (row is CycleSettingRow { SelectedIndex: < 0 })
                row.SettingValue = _defaults[row];

            _applied[row] = row.SettingValue;
        }
    }

    private void SetUpButtons()
    {
        CloseButton.Pressed += RequestClose;
        RestoreButton.Pressed += OnRestoreButtonPressed;
        CancelButton.Pressed += DiscardAndClose;
        ApplyButton.Pressed += () => ApplyChanges();
        ConfirmKeepButton.Pressed += HideConfirm;
        ConfirmDiscardButton.Pressed += DiscardAndClose;
        ConfirmApplyButton.Pressed += OnConfirmApplyButtonPressed;
    }

    private void OnTabToggled(Button tab, bool toggledOn)
    {
        if (toggledOn)
            ShowCategory(tab);
    }

    private void ShowCategory(Button tab)
    {
        _currentTab = tab;

        foreach (var category in _categories)
            category.Value.List.Visible = category.Key == tab;

        CategoryLabel.Text = _categories[tab].Caption;
        SettingsScroll.ScrollVertical = 0;

        var firstRow = RowsIn(tab).FirstOrDefault();

        if (firstRow != null)
            firstRow.LabelButton.ButtonPressed = true;

        SelectRow(firstRow);
    }

    private IEnumerable<SettingRow> RowsIn(Button tab)
    {
        return _rows.Where(row => row.GetParent() == _categories[tab].List);
    }

    private void OnRowToggled(SettingRow row, bool toggledOn)
    {
        if (toggledOn)
            SelectRow(row);
    }

    private void OnRowFocused(SettingRow row)
    {
        row.LabelButton.ButtonPressed = true;
    }

    private void OnRowGuiInput(SettingRow row, InputEvent inputEvent)
    {
        var direction = inputEvent.IsActionPressed("ui_left", true) ? -1 : inputEvent.IsActionPressed("ui_right", true) ? 1 : 0;

        if (direction != 0 && row.StepValue(direction))
            row.LabelButton.AcceptEvent();
    }

    private void OnRowValueChanged(SettingRow row)
    {
        var settings = SettingsManager.Instance;

        if (!IsStored(row))
        {
            row.LabelButton.ButtonPressed = true;
            SelectRow(row);
            return;
        }

        if (row is KeybindSettingRow)
            ResolveKeyConflicts(row);

        settings.SetSetting(row.SettingKey, row.SettingValue);
        settings.Apply(row.SettingKey);

        row.LabelButton.ButtonPressed = true;
        SelectRow(row);
        UpdateStatus();
    }

    private void ResolveKeyConflicts(SettingRow row)
    {
        var settings = SettingsManager.Instance;
        var previousKey = settings.GetSetting(row.SettingKey, row.SettingValue);

        foreach (var other in _rows.Where(other => other != row && other is KeybindSettingRow && IsStored(other) && IsSameValue(other, other.SettingValue, row.SettingValue)))
        {
            other.SettingValue = previousKey;
            settings.SetSetting(other.SettingKey, previousKey);
        }
    }

    private void OnRestoreButtonPressed()
    {
        var settings = SettingsManager.Instance;

        foreach (var row in RowsIn(_currentTab).Where(IsStored))
        {
            row.SettingValue = _defaults[row];
            settings.SetSetting(row.SettingKey, row.SettingValue);
        }

        settings.ApplyAll();
        UpdateReadout();
        UpdateStatus();
    }

    private bool ApplyChanges()
    {
        var error = SettingsManager.Instance.Save();
        if (error != Godot.Error.Ok)
        {
            HideConfirm();
            StatusLabel.Text = "Could not save settings: " + error;
            return false;
        }

        foreach (var row in _rows.Where(IsStored))
            _applied[row] = row.SettingValue;

        UpdateStatus();
        return true;
    }

    private void OnConfirmApplyButtonPressed()
    {
        if (pendingPreset >= 0)
        {
            ApplyPreset();
            return;
        }
        if (ApplyChanges()) Close();
    }

    private void DiscardAndClose()
    {
        SettingsManager.Instance.Revert();
        Close();
    }

    private void RequestClose()
    {
        if (HasPendingChanges())
            ShowConfirm();
        else
            Close();
    }

    private void Close()
    {
        OverlayManager.Instance.CloseOverlay();
    }

    private void ShowConfirm()
    {
        pendingPreset = -1;
        GetNode<Label>("%ConfirmTitle").Text = "Unsaved Changes";
        GetNode<Label>("%ConfirmMessage").Text = "Apply your changes before closing?";
        ConfirmKeepButton.Text = "Keep Editing";
        ConfirmApplyButton.Text = "Apply & Close";
        ConfirmDiscardButton.Visible = true;
        ConfirmOverlay.Visible = true;
        ConfirmKeepButton.GrabFocus();
    }

    private void HideConfirm()
    {
        pendingPreset = -1;
        ConfirmOverlay.Visible = false;
        FocusSelectedRow();
    }

    private void FocusSelectedRow()
    {
        _selectedRow?.LabelButton.GrabFocus();
    }

    private void SelectRow(SettingRow row)
    {
        _selectedRow = row;
        UpdateReadout();
    }

    private void UpdateReadout()
    {
        SelectedLabel.Text = _selectedRow?.LabelText ?? "";
        DescriptionLabel.Text = _selectedRow?.Description ?? "";
        CurrentValue.Text = _selectedRow?.FormatValue(_selectedRow.SettingValue) ?? "";
        DefaultValue.Text = _selectedRow != null && _defaults.TryGetValue(_selectedRow, out var defaultValue) ? _selectedRow.FormatValue(defaultValue) : "";
    }

    private void UpdateStatus()
    {
        var pending = HasPendingChanges();

        StatusLight.Color = pending ? PendingStatusColour : _appliedStatusColour;
        StatusLabel.Text = pending ? PendingStatusText : _appliedStatusText;
        ApplyButton.Disabled = !pending;
    }

    private bool HasPendingChanges()
    {
        return _rows.Where(IsStored).Any(row => !IsSameValue(row, row.SettingValue, _applied[row]));
    }

    private bool IsStored(SettingRow row)
    {
        return _applied.ContainsKey(row);
    }

    private static bool IsSameValue(SettingRow row, Variant first, Variant second)
    {
        return row.FormatValue(first) == row.FormatValue(second);
    }

    #region Cheats

    public ToggleSettingRow InfiniteResourcesRow { get; private set; }
    public ActionSettingRow SkipToShuttlesRow { get; private set; }
    public ActionSettingRow EarthStationTo7Row { get; private set; }
    public ActionSettingRow EarthOrbitProductionRow { get; private set; }
    public ActionSettingRow IOSModulesReadyRow { get; private set; }
    public ActionSettingRow ActivateMTXRow { get; private set; }
    public ActionSettingRow BuildTitanStationRow { get; private set; }

    private void SetUpCheats()
    {
        InfiniteResourcesRow = GetNode<ToggleSettingRow>("%InfiniteResourcesRow");
        SkipToShuttlesRow = GetNode<ActionSettingRow>("%SkipToShuttlesRow");
        EarthStationTo7Row = GetNode<ActionSettingRow>("%EarthStationTo7Row");
        EarthOrbitProductionRow = GetNode<ActionSettingRow>("%EarthOrbitProductionRow");
        IOSModulesReadyRow = GetNode<ActionSettingRow>("%IOSModulesReadyRow");
        ActivateMTXRow = GetNode<ActionSettingRow>("%ActivateMTXRow");
        BuildTitanStationRow = GetNode<ActionSettingRow>("%BuildTitanStationRow");

        InfiniteResourcesRow.SettingValue = GameCore.SingletonInstance?.InfiniteResources ?? false;

        InfiniteResourcesRow.ValueChanged += InfiniteResources_ValueChanged;
        SkipToShuttlesRow.ActionButton.Pressed += () => RequestPreset(0);
        EarthStationTo7Row.ActionButton.Pressed += () => RequestPreset(1);
        EarthOrbitProductionRow.ActionButton.Pressed += () => RequestPreset(2);
        IOSModulesReadyRow.ActionButton.Pressed += () => RequestPreset(3);
        ActivateMTXRow.ActionButton.Pressed += () => RequestPreset(4);
        BuildTitanStationRow.ActionButton.Pressed += () => RequestPreset(5);
    }

    private void RequestPreset(int choice)
    {
        ShowConfirm();
        pendingPreset = choice;
        GetNode<Label>("%ConfirmTitle").Text = "Change progress?";
        GetNode<Label>("%ConfirmMessage").Text = "This changes your current game. Save first.";
        ConfirmKeepButton.Text = "Cancel";
        ConfirmApplyButton.Text = "Apply Preset";
        ConfirmDiscardButton.Visible = false;
    }

    private void ApplyPreset()
    {
        var choice = pendingPreset;
        HideConfirm();
        var core = GameCore.SingletonInstance;
        var earth = GameCore.Earth;
        var mayCreateCrew = choice <= 3 || (choice == 4 && !earth.Station.Built
            && earth.Station.BuildParts < 7 && !core.GameData.ActiveSaveFile.Unlocks.Contains(Game_Unlocks.Shuttle_Unlock));
        if (mayCreateCrew && !earth.PlanetResources.Staff.Any(team => team == null || team.Type == StaffType.Marines))
        {
            StatusLabel.Text = "Free a crew slot first.";
            return;
        }
        Action[] actions = { SkipToShuttles_Pressed, EarthStationTo7_Pressed, ProdInEarthOrbit_Pressed,
            IOSModulesReady_Pressed, ActivateMTX_Pressed, BuildTitanStation_Pressed };
        var previousScene = core.currentScene;
        actions[choice]();
        if (core.currentScene == previousScene && core.currentScene != Scenes.Bulletins)
            core.ChangeScene(core.currentScene, new(core.SceneVariables));
        RequestClose();
    }

    private void InfiniteResources_ValueChanged()
    {
        GameCore.SingletonInstance.InfiniteResources = InfiniteResourcesRow.SettingValue.AsBool();
    }

    #endregion
}
