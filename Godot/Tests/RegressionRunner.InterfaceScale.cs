using System.Threading.Tasks;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Helpers;
using Godot;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task SettingsInterfaceScale()
        {
            await WithSettings(async screen =>
            {
                var manager = SettingsManager.Instance;
                manager.SetSetting("display/resolution", "1920 x 1080");
                GetWindow().Size = new Vector2I(1920, 1080);
                await InputFrames();
                var gameScale = GameViewportContainer.Instance.Scale;
                float Scale() => screen.GetGlobalTransformWithCanvas().Scale.X;
                async Task Click(Control control)
                {
                    var point = control.GetGlobalTransformWithCanvas() * (control.Size / 2);
                    foreach (var pressed in new[] { true, false })
                        GetViewport().PushInput(new InputEventMouseButton
                        { Position = point, GlobalPosition = point, ButtonIndex = MouseButton.Left, Pressed = pressed }, true);
                    await InputFrames();
                }
                void Fits(Control control)
                {
                    var transform = control.GetGlobalTransformWithCanvas();
                    var top = transform * Vector2.Zero;
                    var bottom = transform * control.Size;
                    var window = GetViewport().GetVisibleRect().Size;
                    Equal(true, top.X >= 0 && top.Y >= 0 && bottom.X <= window.X + 1 && bottom.Y <= window.Y + 1,
                        control.Name + " remains inside the window");
                }
                screen.InterfaceScaleRow.StepValue(1);
                await InputFrames();
                Equal(1.25f, Scale(), "125% preview enlarges the actual settings controls");
                Equal(gameScale, GameViewportContainer.Instance.Scale, "interface zoom leaves the game scale alone");
                screen.InterfaceScaleRow.StepValue(1);
                await InputFrames();
                Equal(1.5f, Scale(), "150% preview");
                screen.InterfaceScaleRow.StepValue(1);
                await InputFrames();
                Equal(1.5f, Scale(), "200% request is capped to fit a 1920x1080 window");
                Equal("200%", screen.InterfaceScaleRow.SettingValue.AsString(), "requested zoom survives the fit cap");
                Fits(screen.CloseButton);
                Fits(screen.ApplyButton);
                await Click(screen.ApplyButton);
                using (var disk = new GameConfig(manager.Config.FilePath))
                    Equal("200%", disk.GetValue("display", "interface_scale").AsString(), "scaled pointer applies the preference to disk");
                // macOS caps a native window to the usable desktop height.
                if (DisplayServer.GetName() == "headless")
                {
                    GetWindow().Size = new Vector2I(2560, 1440);
                    await InputFrames();
                    Equal(2f, Scale(), "200% is available when both dimensions permit it");
                }
                foreach (var (size, scale) in new[]
                {
                    (new Vector2I(1600, 1080), 1.25f),
                    (new Vector2I(1920, 900), 1.25f)
                })
                {
                    GetWindow().Size = size;
                    await InputFrames();
                    Equal(scale, Scale(), "preferred zoom fits both dimensions at " + size);
                    Fits(screen.CloseButton);
                    Fits(screen.ApplyButton);
                }
                GetWindow().Size = new Vector2I(1280, 720);
                await InputFrames();
                Equal(1f, Scale(), "shrinking the open window keeps controls usable");
                Fits(screen.CloseButton);
                Fits(screen.ApplyButton);
                GetWindow().Size = new Vector2I(1920, 1080);
                await InputFrames();
                Equal(1.5f, Scale(), "growing the window restores the preferred zoom up to the fit cap");
                await Click(screen.CloseButton);
                Equal(false, OverlayManager.Instance.IsOpen, "scaled close pointer reaches its control");
                screen = OpenSettings();
                await InputFrames();
                Equal(1.5f, Scale(), "reopening uses the saved scale");
                screen.InterfaceScaleRow.StepValue(1);
                await InputFrames();
                Equal(1f, Scale(), "100% previews without changing the applied preference");
                await Click(screen.CloseButton);
                Equal(true, screen.ConfirmOverlay.Visible, "unsaved zoom retains the discard prompt");
                Fits(screen.ConfirmDiscardButton);
                await Click(screen.ConfirmDiscardButton);
                screen = OpenSettings();
                await InputFrames();
                Equal(1.5f, Scale(), "discard restores saved zoom");
                await CaptureDisplayEvidence("interface-scale-fit");
                Press(screen, "%RestoreButton");
                await InputFrames();
                Equal(1f, Scale(), "Display defaults restore 100% zoom");
                foreach (var invalid in new Variant[] { "invalid", "-100%", 42 })
                {
                    manager.SetSetting("display/interface_scale", invalid);
                    manager.ApplyDisplay();
                    await InputFrames();
                    Equal(1f, Scale(), "malformed zoom falls back safely");
                }
            });
        }
    }
}
