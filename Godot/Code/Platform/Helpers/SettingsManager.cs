using Deuteros.Code.Objects;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Deuteros.Code.Platform.Helpers
{
    public partial class SettingsManager : Node
    {
        public const int VolumeSteps = 10;
        public const string KeybindSection = "keybinds";
        public const string WindowedMode = "Windowed";
        public const string BorderlessMode = "Borderless";
        public const string FullscreenMode = "Fullscreen";
        public const string UnlimitedFrameLimit = "Unlimited";

        public static readonly Vector2I MinWindowSize = new Vector2I(1280, 720);

        private readonly List<string> _windowModeOptions = new List<string>();
        private readonly List<string> _frameLimitOptions = new List<string>();
        private readonly Dictionary<string, Key> _defaultKeys = new Dictionary<string, Key>();
        private readonly List<Vector2I> _resolutionSizes = new List<Vector2I>();
        private readonly Dictionary<string, string> _volumeBuses = new Dictionary<string, string>();
        private readonly Dictionary<string, Variant> _defaults = new Dictionary<string, Variant>();
        private bool _focusMuted;

        public static SettingsManager Instance { get; private set; }

        public GameConfig Config { get; private set; }

        public string[] ResolutionOptions { get; private set; }

        public override void _Ready()
        {
            Instance = this;
            ProcessMode = ProcessModeEnum.Always;

            BuildOptionLists();
            BuildVolumeBuses();

            Config = new GameConfig();
            ResolutionOptions = BuildResolutionOptions();
            BuildDefaults();
            ApplyAll();
        }

        public override void _ExitTree()
        {
            foreach (var action in _defaultKeys.Keys)
            {
                if (InputMap.HasAction(action))
                    InputMap.ActionEraseEvents(action);
            }
            Config?.Dispose();
            Config = null;
            Instance = null;
        }

        public override void _Notification(int what)
        {
            if (Config == null)
                return;

            if (what == NotificationApplicationFocusOut)
                SetFocusMuted(true);
            else if (what == NotificationApplicationFocusIn)
                SetFocusMuted(false);
        }

        public Variant GetSetting(string settingKey, Variant fallback = default)
        {
            var (section, key) = SplitKey(settingKey);

            var defaultValue = GetDefault(settingKey, fallback);
            var value = Config.GetValue(section, key, defaultValue);
            if (defaultValue.VariantType != Variant.Type.Nil && value.VariantType != defaultValue.VariantType)
                return defaultValue;
            if (_volumeBuses.ContainsKey(settingKey))
                return Math.Clamp(value.AsInt64(), 0, VolumeSteps);
            if (settingKey == "display/scanlines")
                return Math.Clamp(value.AsInt64(), 0, 10);
            return value;
        }

        public Variant GetDefault(string settingKey, Variant fallback = default)
        {
            return _defaults.TryGetValue(settingKey, out var value) ? value : fallback;
        }

        public bool HasDefault(string settingKey)
        {
            return _defaults.ContainsKey(settingKey);
        }

        public void SetSetting(string settingKey, Variant value)
        {
            var (section, key) = SplitKey(settingKey);
            Config.SetValue(section, key, value, false);
        }

        public Godot.Error Save() => Config.Save();

        public void Revert()
        {
            Config.Load();
            ApplyAll();
        }

        public string[] GetOptions(string settingKey)
        {
            return settingKey switch
            {
                "display/resolution" => ResolutionOptions,
                "display/window_mode" => _windowModeOptions.ToArray(),
                "display/frame_limit" => _frameLimitOptions.ToArray(),
                "display/pixel_scaling" => new[] { "Integer", "Fit" },
                _ => null
            };
        }

        public void Apply(string settingKey)
        {
            var (section, _) = SplitKey(settingKey);

            if (section == "display")
                ApplyDisplay();
            else if (section == "audio")
                ApplyAudio();
            else if (section == KeybindSection)
                ApplyKeybinds();
        }

        public void ApplyAll()
        {
            ApplyDisplay();
            ApplyAudio();
            ApplyKeybinds();
        }

        public void ApplyDisplay()
        {
            Engine.MaxFps = ParseFrameLimit(GetSetting("display/frame_limit").AsString());
            GameViewportContainer.Instance?.UpdateLayout();
            if (DisplayServer.GetName() == "headless") return;
            DisplayServer.WindowSetMinSize(MinWindowSize);
            DisplayServer.WindowSetVsyncMode(GetSetting("display/vsync").AsBool() ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);

            var mode = ParseWindowMode(GetSetting("display/window_mode").AsString());
            DisplayServer.WindowSetMode(mode);

            if (mode != DisplayServer.WindowMode.Windowed)
                return;

            var size = ParseResolution(GetSetting("display/resolution").AsString());

            if (DisplayServer.WindowGetSize() == size)
                return;

            DisplayServer.WindowSetSize(size);

            var screen = DisplayServer.ScreenGetUsableRect(DisplayServer.WindowGetCurrentScreen());
            DisplayServer.WindowSetPosition(screen.Position + (screen.Size - size) / 2);
        }

        public void ApplyAudio()
        {
            foreach (var volumeBus in _volumeBuses)
                ApplyBusVolume(volumeBus.Value, GetSetting(volumeBus.Key).AsInt32());
        }

        public void ApplyKeybinds()
        {
            foreach (var action in _defaultKeys.Keys)
            {
                if (!InputMap.HasAction(action))
                    continue;

                InputMap.ActionEraseEvents(action);

                var key = (Key)GetSetting(KeybindSection + "/" + action).AsInt64();

                if (key == Key.None)
                    continue;

                var newEvent = new InputEventKey();
                newEvent.Keycode = key;
                newEvent.Device = -1;
                InputMap.ActionAddEvent(action, newEvent);
            }
        }

        public static string FormatResolution(Vector2I size)
        {
            return $"{size.X} x {size.Y}";
        }

        private void BuildOptionLists()
        {
            _windowModeOptions.Add(WindowedMode);
            _windowModeOptions.Add(BorderlessMode);
            _windowModeOptions.Add(FullscreenMode);

            _frameLimitOptions.Add("30");
            _frameLimitOptions.Add("60");
            _frameLimitOptions.Add("120");
            _frameLimitOptions.Add("144");
            _frameLimitOptions.Add(UnlimitedFrameLimit);

            _defaultKeys["pause"] = Key.P;
            _defaultKeys["speed_up"] = Key.Equal;
            _defaultKeys["slow_down"] = Key.Minus;
            _defaultKeys["next_location"] = Key.Tab;
            _defaultKeys["research"] = Key.R;
            _defaultKeys["production"] = Key.F;
            _defaultKeys["quick_save"] = Key.F5;

            _resolutionSizes.Add(new Vector2I(1280, 720));
            _resolutionSizes.Add(new Vector2I(1280, 800));
            _resolutionSizes.Add(new Vector2I(1366, 768));
            _resolutionSizes.Add(new Vector2I(1440, 900));
            _resolutionSizes.Add(new Vector2I(1600, 900));
            _resolutionSizes.Add(new Vector2I(1680, 1050));
            _resolutionSizes.Add(new Vector2I(1920, 1080));
            _resolutionSizes.Add(new Vector2I(1920, 1200));
            _resolutionSizes.Add(new Vector2I(2560, 1440));
            _resolutionSizes.Add(new Vector2I(2560, 1600));
            _resolutionSizes.Add(new Vector2I(3840, 2160));
        }

        private void BuildVolumeBuses()
        {
            _volumeBuses["audio/master_volume"] = "Master";
            _volumeBuses["audio/music_volume"] = "Music";
            _volumeBuses["audio/effects_volume"] = "Effects";
            _volumeBuses["audio/interface_volume"] = "Interface";
        }

        private void BuildDefaults()
        {
            _defaults["display/resolution"] = FormatResolution(MinWindowSize);
            _defaults["display/window_mode"] = WindowedMode;
            _defaults["display/vsync"] = true;
            _defaults["display/frame_limit"] = UnlimitedFrameLimit;
            _defaults["display/pixel_scaling"] = "Integer";
            _defaults["display/scanlines"] = 0;

            foreach (var volumeBus in _volumeBuses)
                _defaults[volumeBus.Key] = 8;

            _defaults["audio/mute_unfocused"] = false;

            foreach (var defaultKey in _defaultKeys)
                _defaults[KeybindSection + "/" + defaultKey.Key] = (long)defaultKey.Value;
        }

        private void SetFocusMuted(bool unfocused)
        {
            _focusMuted = unfocused && GetSetting("audio/mute_unfocused").AsBool();
            ApplyAudio();
        }

        private void ApplyBusVolume(string busName, int volume)
        {
            var bus = AudioServer.GetBusIndex(busName);

            if (bus < 0)
                return;

            var muted = volume <= 0 || (bus == 0 && _focusMuted);
            AudioServer.SetBusMute(bus, muted);

            if (volume > 0)
                AudioServer.SetBusVolumeDb(bus, Mathf.LinearToDb(Math.Min(volume, VolumeSteps) / (float)VolumeSteps));
        }

        private string[] BuildResolutionOptions()
        {
            if (DisplayServer.GetName() == "headless") return _resolutionSizes.Select(FormatResolution).ToArray();
            var screen = DisplayServer.ScreenGetUsableRect(DisplayServer.WindowGetCurrentScreen()).Size;
            var options = _resolutionSizes.Where(size => size.X <= screen.X && size.Y <= screen.Y).Select(FormatResolution).ToList();

            if (options.Count == 0)
                options.Add(FormatResolution(MinWindowSize));

            return options.ToArray();
        }

        private static Vector2I ParseResolution(string option)
        {
            var parts = option.Split('x');

            if (parts.Length != 2 || !int.TryParse(parts[0].Trim(), out var width) || !int.TryParse(parts[1].Trim(), out var height))
                return MinWindowSize;

            return new Vector2I(Math.Max(width, MinWindowSize.X), Math.Max(height, MinWindowSize.Y));
        }

        private static int ParseFrameLimit(string option)
        {
            return int.TryParse(option, out var fps) ? Math.Max(0, fps) : 0;
        }

        private static DisplayServer.WindowMode ParseWindowMode(string option)
        {
            if (option == BorderlessMode)
                return DisplayServer.WindowMode.Fullscreen;

            if (option == FullscreenMode)
                return DisplayServer.WindowMode.ExclusiveFullscreen;

            return DisplayServer.WindowMode.Windowed;
        }

        private static (string Section, string Key) SplitKey(string settingKey)
        {
            var slash = settingKey.IndexOf('/');
            return slash < 0 ? ("general", settingKey) : (settingKey.Substring(0, slash), settingKey.Substring(slash + 1));
        }
    }
}
