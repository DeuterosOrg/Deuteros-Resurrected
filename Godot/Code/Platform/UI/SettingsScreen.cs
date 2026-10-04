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
        ApplyButton.Pressed += ApplyChanges;
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

    private void ApplyChanges()
    {
        SettingsManager.Instance.Save();

        foreach (var row in _rows.Where(IsStored))
            _applied[row] = row.SettingValue;

        UpdateStatus();
    }

    private void OnConfirmApplyButtonPressed()
    {
        ApplyChanges();
        Close();
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
        ConfirmOverlay.Visible = true;
        ConfirmKeepButton.GrabFocus();
    }

    private void HideConfirm()
    {
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
        SkipToShuttlesRow.ActionButton.Pressed += SkipToShuttles_Pressed;
        EarthStationTo7Row.ActionButton.Pressed += EarthStationTo7_Pressed;
        EarthOrbitProductionRow.ActionButton.Pressed += ProdInEarthOrbit_Pressed;
        IOSModulesReadyRow.ActionButton.Pressed += IOSModulesReady_Pressed;
        ActivateMTXRow.ActionButton.Pressed += ActivateMTX_Pressed;
        BuildTitanStationRow.ActionButton.Pressed += BuildTitanStation_Pressed;
    }

    private void InfiniteResources_ValueChanged()
    {
        GameCore.SingletonInstance.InfiniteResources = InfiniteResourcesRow.SettingValue.AsBool();
    }

    private void BuildTitanStation_Pressed()
    {
        if (!GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[StellarBodies.titan].Station.Built)
        {
            var titan = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[StellarBodies.titan];
            titan.PlanetResources.Derricks = 8;
            titan.BaseBuildParts = 2;
            titan.Station.Built = true;
            titan.Station.BuildParts = 8;
            titan.Station.Factory.AOC = true;
            titan.Station.MtxInstalled = true;
        }
    }

    private void ActivateMTX_Pressed()
    {
        if (!GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Contains(Enums.Game_Unlocks.Mass_Tranceiver))
        {
            GameCore.SingletonInstance.TriggerAlienTechDiscovery(Enums.ItemTypes.m__t__x);
            GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[Enums.StellarBodies.earth].Station.MtxInstalled = true;
        }
    }

    private void IOSModulesReady_Pressed()
    {
        var gameData = GameCore.SingletonInstance.GameData;
        var earth = (Earth)gameData.ActiveSaveFile.BaseGameData.Planets[Enums.StellarBodies.earth];

        if (!GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Contains(Enums.Game_Unlocks.IOS_Attachments))
        {
            if (!earth.Station.Built)
                ProdInEarthOrbit_Pressed();

            GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Add(Enums.Game_Unlocks.IOS_Attachments);

            GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.a__m__a).Research.Locked = false;
            GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.a__o__c).Research.Locked = false;
            GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.bandaid).Research.Locked = false;
            GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.grapple).Research.Locked = false;
            GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.r_frame).Research.Locked = false;

            gameData.GetItem(Enums.ItemTypes.a__m__a).Research.Researched = true;
            gameData.GetItem(Enums.ItemTypes.a__m__a).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
            gameData.GetItem(Enums.ItemTypes.a__m__a).Locked = false;

            gameData.GetItem(Enums.ItemTypes.a__o__c).Research.Researched = true;
            gameData.GetItem(Enums.ItemTypes.a__o__c).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
            gameData.GetItem(Enums.ItemTypes.a__o__c).Locked = false;

            gameData.GetItem(Enums.ItemTypes.bandaid).Research.Researched = true;
            gameData.GetItem(Enums.ItemTypes.bandaid).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
            gameData.GetItem(Enums.ItemTypes.bandaid).Locked = false;

            gameData.GetItem(Enums.ItemTypes.grapple).Research.Researched = true;
            gameData.GetItem(Enums.ItemTypes.grapple).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
            gameData.GetItem(Enums.ItemTypes.grapple).Locked = false;

            gameData.GetItem(Enums.ItemTypes.r_frame).Research.Researched = true;
            gameData.GetItem(Enums.ItemTypes.r_frame).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
            gameData.GetItem(Enums.ItemTypes.r_frame).Locked = false;
        }
    }

    private void ProdInEarthOrbit_Pressed()
    {
        var gameData = GameCore.SingletonInstance.GameData;
        var earth = (Earth)gameData.ActiveSaveFile.BaseGameData.Planets[Enums.StellarBodies.earth];

        //Make sure we have a station at all
        if (earth.Station.BuildParts < 7)
        {
            EarthStationTo7_Pressed();
        }

        earth.Station.Built = true;
        earth.Station.BuildParts = 8;
        earth.Station.Factory.AOC = true;

        GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Add(Enums.Game_Unlocks.First_Station_Segment);
        GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Add(Enums.Game_Unlocks.Space_Stations);

        GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.i_chassis).Research.Locked = false;
        GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.i_drive).Research.Locked = false;
        GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.a__c__c).Research.Locked = false;
    }

    private void EarthStationTo7_Pressed()
    {
        var gameData = GameCore.SingletonInstance.GameData;
        var earth = (Earth)gameData.ActiveSaveFile.BaseGameData.Planets[Enums.StellarBodies.earth];

        if (earth.Station.BuildParts < 7)
        {
            earth.Station.BuildParts = 7;

            if (!gameData.GetItem(Enums.ItemTypes.a__c__c).Research.Researched)
            {
                gameData.GetItem(Enums.ItemTypes.a__c__c).Research.Researched = true;
                gameData.GetItem(Enums.ItemTypes.a__c__c).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
                gameData.GetItem(Enums.ItemTypes.a__c__c).Locked = false;

                gameData.GetItem(Enums.ItemTypes.i_chassis).Research.Locked = false;
                gameData.GetItem(Enums.ItemTypes.i_drive).Research.Locked = false;
                gameData.GetItem(Enums.ItemTypes.a__c__c).Research.Locked = false;
            }

            earth.PlanetResources.Stores[Enums.ItemTypes.supply_pod] = Math.Max(1, earth.PlanetResources.Stores[Enums.ItemTypes.supply_pod]);
            earth.PlanetResources.Stores[Enums.ItemTypes.a__c__c] = Math.Max(1, earth.PlanetResources.Stores[Enums.ItemTypes.a__c__c]);

            GameCore.SingletonInstance.TriggerStationPiecePlaced(Enums.StellarBodies.earth);
        }

        if (!GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Contains(Enums.Game_Unlocks.Shuttle_Unlock))
        {
            SkipToShuttles_Pressed();
        }
    }

    private void SkipToShuttles_Pressed()
    {
        var gameData = GameCore.SingletonInstance.GameData;

        if (!gameData.GetItem(Enums.ItemTypes.s_chassis).Research.Researched)
        {
            gameData.GetItem(Enums.ItemTypes.s_chassis).Research.Researched = true;
            gameData.GetItem(Enums.ItemTypes.s_chassis).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
            gameData.GetItem(Enums.ItemTypes.s_chassis).Locked = false;
        }

        if (!gameData.GetItem(Enums.ItemTypes.s_drive).Research.Researched)
        {
            gameData.GetItem(Enums.ItemTypes.s_drive).Research.Researched = true;
            gameData.GetItem(Enums.ItemTypes.s_drive).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
            gameData.GetItem(Enums.ItemTypes.s_drive).Locked = false;
        }

        if (!gameData.GetItem(Enums.ItemTypes.meh_fuel).Research.Researched)
        {
            gameData.GetItem(Enums.ItemTypes.meh_fuel).Research.Researched = true;
            gameData.GetItem(Enums.ItemTypes.meh_fuel).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
            gameData.GetItem(Enums.ItemTypes.meh_fuel).Locked = false;
        }

        if (!gameData.GetItem(Enums.ItemTypes.of_frame).Research.Researched)
        {
            gameData.GetItem(Enums.ItemTypes.of_frame).Research.Researched = true;
            gameData.GetItem(Enums.ItemTypes.of_frame).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
            gameData.GetItem(Enums.ItemTypes.of_frame).Locked = false;
        }

        if (!gameData.GetItem(Enums.ItemTypes.tool_pod).Research.Researched)
        {
            gameData.GetItem(Enums.ItemTypes.tool_pod).Research.Researched = true;
            gameData.GetItem(Enums.ItemTypes.tool_pod).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
            gameData.GetItem(Enums.ItemTypes.tool_pod).Locked = false;
        }

        if (!gameData.GetItem(Enums.ItemTypes.supply_pod).Research.Researched)
        {
            gameData.GetItem(Enums.ItemTypes.supply_pod).Research.Researched = true;
            gameData.GetItem(Enums.ItemTypes.supply_pod).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
            gameData.GetItem(Enums.ItemTypes.supply_pod).Locked = false;
        }

        if (!gameData.GetItem(Enums.ItemTypes.cryo_pod).Research.Researched)
        {
            gameData.GetItem(Enums.ItemTypes.cryo_pod).Research.Researched = true;
            gameData.GetItem(Enums.ItemTypes.cryo_pod).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
            gameData.GetItem(Enums.ItemTypes.cryo_pod).Locked = false;
        }

        var earth = (Earth)gameData.ActiveSaveFile.BaseGameData.Planets[Enums.StellarBodies.earth];

        if (earth.ResearchStaff == null || earth.ResearchStaff.Count == 0)
        {
            earth.ResearchStaff = new Staff();
            earth.ResearchStaff.Leader = "Von Braun";
            earth.ResearchStaff.Count = 250;
            earth.ResearchStaff.AddAction(20);

            earth.ResearchStaff.Type = Enums.StaffType.Research;
        }

        if (earth.Factory.Builder == null || earth.Factory.Builder.Count == 0)
        {
            earth.Factory.Builder = new Staff();
            earth.Factory.Builder.Leader = "Bob";
            earth.Factory.Builder.Count = 200;
            earth.Factory.Builder.AddAction(20);
            earth.Factory.Builder.Type = Enums.StaffType.Production;
        }

        if (!earth.PlanetResources.Staff.Any(T => T != null && T.Type == Enums.StaffType.Marines))
        {
            var newMarine = new Staff();
            newMarine.Leader = GameCore.SingletonInstance.GameData.GetNextPersonName();
            newMarine.Count = 41;
            newMarine.AddAction(30);
            newMarine.Type = Enums.StaffType.Marines;

            earth.PlanetResources.AddStaff(newMarine);
        }

        earth.PlanetResources.Derricks = Math.Max(8, earth.PlanetResources.Derricks);
        earth.PlanetResources.Stores[Enums.ItemTypes.s_chassis] = Math.Max(1, earth.PlanetResources.Stores[Enums.ItemTypes.s_chassis]);
        earth.PlanetResources.Stores[Enums.ItemTypes.s_drive] = Math.Max(1, earth.PlanetResources.Stores[Enums.ItemTypes.s_drive]);
        earth.PlanetResources.Stores[Enums.ItemTypes.of_frame] = Math.Max(8, earth.PlanetResources.Stores[Enums.ItemTypes.of_frame]);

        if (!GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Any(T => T.ShipType == Ship_Types.Shuttle && T.PlanetLocation == StellarBodies.earth))
        {
            var newShuttle = new Shuttle();
            newShuttle.StartTravelDay = 0;
            newShuttle.StarLocation = Enums.StellarBodies.the_sun;
            newShuttle.Modules = new List<ShipModule>();
            newShuttle.Modules.Add(new ShipModule());
            newShuttle.ShipState = Ship_States.Docked;
            newShuttle.Fuel = 250;
            newShuttle.Engine = true;
            newShuttle.FuelType = Enums.ItemTypes.meh_fuel;
            newShuttle.OnGround = true;
            newShuttle.Pilot = null;
            newShuttle.PlanetLocation = Enums.StellarBodies.earth;
            newShuttle.ShipType = Enums.Ship_Types.Shuttle;
            newShuttle.LocationView = false;
            newShuttle.Name = "Earth Shuttle";

            GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Add(newShuttle);
            GameCore.SingletonInstance.TriggerShipCreated(newShuttle);
        }
    }

    #endregion
}
