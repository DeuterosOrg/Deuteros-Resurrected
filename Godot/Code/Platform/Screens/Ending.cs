using System;
using System.Linq;
using Deuteros.Code.Objects.GameData;
using Deuteros.Code.Platform.Base;
using Deuteros.Code.Platform.Helpers;
using Godot;
using Newtonsoft.Json.Linq;

namespace Deuteros.Code.Platform.Screens
{
    /// Fixed original presentation compiled by scripts/extract_ending.py.
    public partial class Ending : Control
    {
        private (int Width, int Height, byte[] Pixels)[] pictures;
        private int[][] palettes;
        private (int Number, int Palette, (int Image, int X, int Y, bool Masked)[] Layers)[] frames;
        private int frameCount, rate, displayed = -1;
        private readonly byte[] indices = new byte[320 * 200];
        private readonly byte[] rgb = new byte[320 * 200 * 3];
        private TextureRect picture;
        private Image image;
        private ImageTexture texture;
        private AudioStreamPlayer music;
        private SaveFile world;
        private Node ownerScreen;
        private bool ready, finished;
        private ulong finishedAt;

        public override void _Ready()
        {
            world = GameCore.SingletonInstance.GameData.ActiveSaveFile;
            ownerScreen = GameCore.SingletonInstance.GetNode("GameContainer/GameViewport/MainScene").GetChildren().FirstOrDefault(n => n is BaseSubScene);
            try
            {
                const string dataPath = "res://Ending/sequence.json", musicPath = "res://Ending/music.wav";
                if (!FileAccess.FileExists(dataPath) || !ResourceLoader.Exists(musicPath))
                    throw new InvalidOperationException("Ending assets are missing.");
                LoadSequence(FileAccess.GetFileAsString(dataPath));
                var stream = GD.Load<AudioStream>(musicPath);
                if (stream == null || Math.Abs(stream.GetLength() - (double)frameCount / rate) > 0.01)
                    throw new InvalidOperationException("Ending audio and animation lengths disagree.");
                image = Image.CreateFromData(320, 200, false, Image.Format.Rgb8, rgb);
                texture = ImageTexture.CreateFromImage(image);
                picture = new TextureRect { Name = "Picture", Texture = texture, Size = new Vector2(320, 200), MouseFilter = MouseFilterEnum.Stop };
                AddChild(picture);
                // Master retains the user's volume preference; Game may be muted by an SDM alarm.
                music = new AudioStreamPlayer { Name = "Music", Stream = stream, Bus = "Master" };
                AddChild(music);
                music.Finished += () => { finished = true; finishedAt = Time.GetTicksUsec(); };
                ready = true;
                Restart();
            }
            catch (Exception error)
            {
                GD.PushWarning("Ending unavailable: " + error.Message);
                music?.Stop();
                var close = new Button { Text = "Ending unavailable.\nVerify the game files.\n\nClose", Size = new Vector2(320, 200) };
                AddChild(close);
                close.Pressed += OverlayManager.Instance.CloseOverlay;
                close.GrabFocus();
            }
        }

        private static int Integer(JToken token, int min, int max)
        {
            if (token?.Type != JTokenType.Integer || token.Value<long>() < min || token.Value<long>() > max)
                throw new InvalidOperationException("Invalid ending number.");
            return token.Value<int>();
        }

        private void LoadSequence(string json)
        {
            var data = JObject.Parse(json);
            rate = Integer(data["rate"], 50, 50);
            frameCount = Integer(data["frame_count"], 1, 10034);
            var imageData = data["images"] as JArray;
            var paletteData = data["palettes"] as JArray;
            var frameData = data["frames"] as JArray;
            if (imageData == null || imageData.Count > 256 || paletteData == null || paletteData.Count > 80
                || frameData == null || frameData.Count == 0 || frameData.Count > frameCount)
                throw new InvalidOperationException("Invalid ending tables.");
            pictures = imageData.Select(value =>
            {
                var width = Integer(value[0], 0, 320); var height = Integer(value[1], 0, 200);
                var pixels = Convert.FromBase64String(value[2].Value<string>());
                if (pixels.Length != width * height || pixels.Any(pixel => pixel > 15))
                    throw new InvalidOperationException("Invalid ending pixels.");
                return (width, height, pixels);
            }).ToArray();
            palettes = paletteData.Select(value =>
            {
                if (!(value is JArray colors) || colors.Count != 16) throw new InvalidOperationException("Invalid ending palette.");
                return colors.Select(color => Integer(color, 0, 4095)).ToArray();
            }).ToArray();
            var previous = -1;
            frames = frameData.Select(value =>
            {
                var number = Integer(value[0], previous + 1, frameCount - 1); previous = number;
                var palette = Integer(value[1], 0, palettes.Length - 1);
                if (!(value[2] is JArray layers) || layers.Count > 7) throw new InvalidOperationException("Invalid ending layers.");
                var decoded = layers.Select(layer =>
                {
                    var index = Integer(layer[0], 0, pictures.Length - 1);
                    if (pictures[index].Width == 0 || layer[3]?.Type != JTokenType.Boolean)
                        throw new InvalidOperationException("Invalid ending layer image.");
                    return (index, Integer(layer[1], -320, 320), Integer(layer[2], -200, 200), layer[3].Value<bool>());
                }).ToArray();
                return (number, palette, decoded);
            }).ToArray();
            if (frames[0].Number != 0 || frames[^1].Number != frameCount - 1 || frames[^1].Layers.Length != 0)
                throw new InvalidOperationException("Ending must start at zero and finish at black.");
        }

        private void Restart()
        {
            finished = false;
            DrawFrame(0);
            music.Play();
        }

        public override void _Process(double delta)
        {
            if (!ReferenceEquals(world, GameCore.SingletonInstance.GameData.ActiveSaveFile)
                || (ownerScreen != null && (!IsInstanceValid(ownerScreen) || !ownerScreen.IsInsideTree() || ownerScreen.IsQueuedForDeletion())))
            {
                SetProcess(false);
                OverlayManager.Instance.CloseOverlay();
                return;
            }
            if (!ready) return;
            var latency = AudioServer.GetOutputLatency();
            var seconds = finished
                ? music.Stream.GetLength() - latency + (Time.GetTicksUsec() - finishedAt) / 1000000.0
                : music.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix() - latency;
            DrawFrame((int)Math.Max(0, seconds * rate));
            if (finished && seconds >= music.Stream.GetLength() && !Input.IsMouseButtonPressed(MouseButton.Left)) Restart();
        }

        public override void _Input(InputEvent @event)
        {
            if (ready) GetViewport().SetInputAsHandled(); // Original ending disables all skip input.
        }

        private void DrawFrame(int number)
        {
            var low = 0; var high = frames.Length - 1;
            while (low < high)
            {
                var mid = (low + high + 1) / 2;
                if (frames[mid].Number <= number) low = mid; else high = mid - 1;
            }
            if (displayed == low) return;
            displayed = low;
            Array.Clear(indices, 0, indices.Length);
            foreach (var layer in frames[low].Layers)
            {
                var source = pictures[layer.Image];
                for (var y = Math.Max(0, -layer.Y); y < Math.Min(source.Height, 200 - layer.Y); y++)
                    for (var x = Math.Max(0, -layer.X); x < Math.Min(source.Width, 320 - layer.X); x++)
                    {
                        var pixel = source.Pixels[y * source.Width + x];
                        if (pixel != 0 || !layer.Masked) indices[(y + layer.Y) * 320 + x + layer.X] = pixel;
                    }
            }
            var colors = palettes[frames[low].Palette];
            for (var pixel = 0; pixel < indices.Length; pixel++)
            {
                var color = colors[indices[pixel]];
                rgb[pixel * 3] = (byte)((color >> 8 & 15) * 17);
                rgb[pixel * 3 + 1] = (byte)((color >> 4 & 15) * 17);
                rgb[pixel * 3 + 2] = (byte)((color & 15) * 17);
            }
            image.SetData(320, 200, false, Image.Format.Rgb8, rgb);
            texture.Update(image);
        }

        public override void _ExitTree()
        {
            ready = false;
            music?.Stop();
            if (picture != null) picture.Texture = null;
            texture?.Dispose();
            image?.Dispose();
        }
    }
}
