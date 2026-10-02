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
            if (FileAccess.FileExists(FilePath)) config.Load(FilePath);
        }

        public Godot.Error Save() => config.Save(FilePath);
        public Godot.Error SetValue(string section, string key, Variant value)
        {
            config.SetValue(section, key, value);
            return Save();
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

        public bool SoundEnabled
        {
            get => ReadBool("audio", "enabled", true);
            set => config.SetValue("audio", "enabled", value);
        }
        public int Volume
        {
            get => Math.Clamp(ReadInt("audio", "volume", 100), 0, 100);
            set => config.SetValue("audio", "volume", Math.Clamp(value, 0, 100));
        }
        public int WindowScale
        {
            get => Math.Clamp(ReadInt("display", "scale", 3), 2, 4);
            set => config.SetValue("display", "scale", Math.Clamp(value, 2, 4));
        }
        public bool Fullscreen
        {
            get => ReadBool("display", "fullscreen", false);
            set => config.SetValue("display", "fullscreen", value);
        }

        public void ApplyAudio()
        {
            var master = AudioServer.GetBusIndex("Master");
            AudioServer.SetBusMute(master, !SoundEnabled || Volume == 0);
            AudioServer.SetBusVolumeDb(master, Volume == 0 ? -80 : Mathf.LinearToDb(Volume / 100f));
        }

        public void ApplyDisplay()
        {
            if (DisplayServer.GetName() == "headless") return;
            DisplayServer.WindowSetMode(Fullscreen ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
            if (!Fullscreen) DisplayServer.WindowSetSize(new Vector2I(320 * WindowScale, 200 * WindowScale));
        }

        public void ResetPreferences()
        {
            SoundEnabled = true;
            Volume = 100;
            WindowScale = 3;
            Fullscreen = false;
        }

        public void Dispose() => config.Dispose();
    }
}
