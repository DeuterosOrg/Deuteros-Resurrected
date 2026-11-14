using Godot;
using System;

namespace Deuteros.Code.Objects
{
    public partial class GameConfig : IDisposable
    {
        public string FilePath { get; set; }
        private readonly ConfigFile config = new();

        public GameConfig(string path = "user://settings.cfg")
        {
            FilePath = path;
            Load();
        }

        public void Load()
        {
            config.Clear();
            if (FileAccess.FileExists(FilePath)) config.Load(FilePath);
            // Convert the earlier preferences in memory; Apply remains the only disk write.
            if (!HasValue("audio", "master_volume") && (HasValue("audio", "enabled") || HasValue("audio", "volume")))
                config.SetValue("audio", "master_volume", SoundEnabled ? (int)Math.Round(Volume / 10.0, MidpointRounding.AwayFromZero) : 0);
            if (!HasValue("display", "resolution") && HasValue("display", "scale"))
                config.SetValue("display", "resolution", $"{Math.Max(1280, 320 * WindowScale)} x {Math.Max(720, 200 * WindowScale)}");
            if (!HasValue("display", "window_mode") && HasValue("display", "fullscreen"))
                config.SetValue("display", "window_mode", Fullscreen ? "Borderless" : "Windowed");
        }

        public Godot.Error Save() => config.Save(FilePath);
        public Godot.Error SetValue(string section, string key, Variant value, bool save = true)
        {
            config.SetValue(section, key, value);
            return save ? Save() : Godot.Error.Ok;
        }
        public Variant GetValue(string section, string key, Variant fallback = default) => config.GetValue(section, key, fallback);
        public bool HasValue(string section, string key) => config.HasSectionKey(section, key);

        private int ReadInt(string section, string key, int fallback)
        {
            var value = GetValue(section, key, fallback);
            return value.VariantType == Variant.Type.Int ? (int)value : fallback;
        }
        private bool ReadBool(string section, string key, bool fallback)
        {
            var value = GetValue(section, key, fallback);
            return value.VariantType == Variant.Type.Bool ? (bool)value : fallback;
        }

        // Legacy keys are read only during migration and remain type checked.
        public bool SoundEnabled => ReadBool("audio", "enabled", true);
        public int Volume => Math.Clamp(ReadInt("audio", "volume", 100), 0, 100);
        public int WindowScale => Math.Clamp(ReadInt("display", "scale", 3), 2, 4);
        public bool Fullscreen => ReadBool("display", "fullscreen", false);

        public void Dispose() => config.Dispose();
    }
}
