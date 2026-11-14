using Godot;
using System;

namespace Deuteros.Code.Platform.Helpers
{
    /// Add as an AutoLoad named "OverlayManager".
    public partial class OverlayManager : CanvasLayer
    {
        public static OverlayManager Instance { get; private set; }

        private Control _overlayRoot;
        private Control _contentArea;
        private bool _fullResolution;
        private Node _contentInstance;
        private bool _wasPaused;
        private bool _focusPaused;
        private bool _overlayPaused;
        private PackedScene _settingsScreen;

        public bool IsOpen => _overlayRoot != null;
        public bool IsShowing(Node content) => content != null && _contentInstance == content;

        public override void _Ready()
        {
            Instance = this; // assign the AutoLoaded instance
            Layer = 128;
            ProcessMode = Node.ProcessModeEnum.Always;

            _settingsScreen = GD.Load<PackedScene>("res://Screens/Settings/SettingsScreen.tscn");
        }

        public Node ShowOverlay(PackedScene packed, bool dodim = true, bool fullResolution = false)
        {
            if (_overlayRoot != null)
                return null; // Already showing something

            Input.MouseMode = Input.MouseModeEnum.Visible;

            _overlayRoot = new Control
            {
                Name = "GlobalOverlay",
                MouseFilter = Control.MouseFilterEnum.Stop,    // Block mouse to the game
                ProcessMode = Node.ProcessModeEnum.WhenPaused, // Overlay stays active when paused
                FocusMode = Control.FocusModeEnum.All
            };
            _overlayRoot.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            AddChild(_overlayRoot);

            if (dodim)
            {
                var dim = new ColorRect
                {
                    Color = new Color(0, 0, 0, 0.5f),
                    MouseFilter = Control.MouseFilterEnum.Stop
                };
                dim.SetAnchorsPreset(Control.LayoutPreset.FullRect);
                _overlayRoot.AddChild(dim);
            }

            _fullResolution = fullResolution;
            _contentArea = new Control
            {
                Name = fullResolution ? "InterfaceArea" : "GameArea",
                MouseFilter = Control.MouseFilterEnum.Pass
            };
            _overlayRoot.AddChild(_contentArea);
            FitContentArea();
            if (GameViewportContainer.Instance != null)
                GameViewportContainer.Instance.LayoutChanged += FitContentArea;

            var center = new CenterContainer
            {
                Name = "Center",
                MouseFilter = Control.MouseFilterEnum.Pass
            };
            center.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _contentArea.AddChild(center);

            _contentInstance = packed.Instantiate();
            center.AddChild(_contentInstance);

            if (_contentInstance is Control ctrl)
            {
                ctrl.FocusMode = Control.FocusModeEnum.All;
                ctrl.GrabFocus();
            }

            if (!_focusPaused && !_overlayPaused)
                _wasPaused = GetTree().Paused;
            _overlayPaused = true;
            GetTree().Paused = true;

            return _contentInstance;
        }

        /// Escape can dismiss overlays; Pause opens Settings and lets that screen own resuming.
        public override void _UnhandledInput(InputEvent @event)
        {
            if (!@event.IsActionPressed("ui_cancel")
                && (!@event.IsActionPressed("pause", false, true) || IsOpen))
                return;

            GetViewport().SetInputAsHandled(); // swallow the event

            if (_overlayRoot == null)
                ShowOverlay(_settingsScreen, true, true);
            else
                CloseOverlay();
        }

        public void CloseOverlay()
        {
            if (_overlayRoot == null)
                return;

            Input.MouseMode = Input.MouseModeEnum.Hidden;

            if (_contentInstance != null && IsInstanceValid(_contentInstance))
                _contentInstance.QueueFree();
            _contentInstance = null;

            if (_contentArea != null && GameViewportContainer.Instance != null)
                GameViewportContainer.Instance.LayoutChanged -= FitContentArea;
            _contentArea = null;

            _overlayRoot.QueueFree();
            _overlayRoot = null;

            // Defer unpausing to the next idle frame,
            // and clear any buffered input (like the Esc that closed us).
            CallDeferred(nameof(FinishClose));
        }

        public void CloseForShutdown()
        {
            // Free paused overlay audio before GameCore waits for mixer disposal.
            if (_contentArea != null && GameViewportContainer.Instance != null)
                GameViewportContainer.Instance.LayoutChanged -= FitContentArea;
            _contentArea = null;
            _overlayRoot?.Free();
            _contentInstance = null;
            _overlayRoot = null;
        }

        private void FitContentArea()
        {
            if (_fullResolution)
            {
                var requested = SettingsManager.Instance?.GetSetting("display/interface_scale").AsString() switch
                {
                    "125%" => 1.25f,
                    "150%" => 1.5f,
                    "200%" => 2f,
                    _ => 1f
                };
                var window = GetViewport().GetVisibleRect().Size;
                var minimum = SettingsManager.MinWindowSize;
                var scale = Math.Min(requested, Math.Min(window.X / minimum.X, window.Y / minimum.Y));
                _contentArea.Scale = Vector2.One * scale;
                _contentArea.Size = window / scale;
                return;
            }

            var game = GameViewportContainer.Instance;

            if (game == null)
            {
                _contentArea.SetAnchorsPreset(Control.LayoutPreset.FullRect);
                return;
            }

            _contentArea.Position = game.GlobalPosition;
            _contentArea.Scale = game.Scale;
            _contentArea.Size = game.Size;
        }

        private void FinishClose()
        {
            // Drop any lingering key presses so the base scene doesn't see them.
            Input.FlushBufferedEvents(); // Godot 4.x

            _overlayPaused = IsOpen;
            GetTree().Paused = _wasPaused || _focusPaused || _overlayPaused;
        }

        public void SetFocusPaused(bool paused)
        {
            if (_focusPaused == paused) return;
            if (paused && !_overlayPaused)
                _wasPaused = GetTree().Paused;
            _focusPaused = paused;
            GetTree().Paused = _wasPaused || _focusPaused || _overlayPaused;
        }
    }

}
