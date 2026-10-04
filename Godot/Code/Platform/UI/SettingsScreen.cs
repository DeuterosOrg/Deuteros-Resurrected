using Deuteros.Code.UI.Rows;
using Godot;

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
    public Button AccessTab { get; private set; }
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

    // Access
    public VBoxContainer AccessList { get; private set; }
    public CycleSettingRow TextSizeRow { get; private set; }
    public CycleSettingRow ColourPaletteRow { get; private set; }
    public ToggleSettingRow ReduceFlickerRow { get; private set; }
    public SliderSettingRow ScreenShakeRow { get; private set; }
    public ToggleSettingRow HoldToConfirmRow { get; private set; }

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
    public override void _Ready()
    {
        // Header & categories
        CloseButton = GetNode<Button>("%CloseButton");
        TitleLabel = GetNode<Label>("%TitleLabel");
        DisplayTab = GetNode<Button>("%DisplayTab");
        AudioTab = GetNode<Button>("%AudioTab");
        ControlsTab = GetNode<Button>("%ControlsTab");
        GameplayTab = GetNode<Button>("%GameplayTab");
        AccessTab = GetNode<Button>("%AccessTab");
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

        // Access
        AccessList = GetNode<VBoxContainer>("%AccessList");
        TextSizeRow = GetNode<CycleSettingRow>("%TextSizeRow");
        ColourPaletteRow = GetNode<CycleSettingRow>("%ColourPaletteRow");
        ReduceFlickerRow = GetNode<ToggleSettingRow>("%ReduceFlickerRow");
        ScreenShakeRow = GetNode<SliderSettingRow>("%ScreenShakeRow");
        HoldToConfirmRow = GetNode<ToggleSettingRow>("%HoldToConfirmRow");

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
    }
}
