using Godot;

namespace Deuteros.Code.Platform.Helpers
{
    /// Add as an AutoLoad named "OverlayManager".
    public partial class OverlayManager : CanvasLayer
    {
        public static OverlayManager Instance { get; private set; }

        private Control _overlayRoot;
        private Node _contentInstance;
        private bool _wasPaused;

        public bool IsOpen => _overlayRoot != null;
        public bool IsShowing(Node content) => content != null && _contentInstance == content;

        public override void _Ready()
        {
            Instance = this; // assign the AutoLoaded instance
            Layer = 128;
            ProcessMode = Node.ProcessModeEnum.WhenPaused;
        }

        public Node ShowOverlay(PackedScene packed, bool dodim = true)
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

            var center = new CenterContainer
            {
                Name = "Center",
                MouseFilter = Control.MouseFilterEnum.Pass
            };
            center.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _overlayRoot.AddChild(center);

            _contentInstance = packed.Instantiate();
            center.AddChild(_contentInstance);

            if (_contentInstance is Control ctrl)
            {
                ctrl.FocusMode = Control.FocusModeEnum.All;
                ctrl.GrabFocus();
            }

            _wasPaused = GetTree().Paused;
            GetTree().Paused = true;

            return _contentInstance;
        }

        /// Handle Escape / Cancel while overlay is up.
        public override void _UnhandledInput(InputEvent @event)
        {
            if (_overlayRoot == null)
                return;

            if (@event.IsActionPressed("ui_cancel"))
            {
                GetViewport().SetInputAsHandled(); // swallow the event
                CloseOverlay();
            }
        }

        public void CloseOverlay()
        {
            if (_overlayRoot == null)
                return;

            Input.MouseMode = Input.MouseModeEnum.Hidden;

            if (_contentInstance != null && IsInstanceValid(_contentInstance))
                _contentInstance.QueueFree();
            _contentInstance = null;

            _overlayRoot.QueueFree();
            _overlayRoot = null;

            // Defer unpausing to the next idle frame,
            // and clear any buffered input (like the Esc that closed us).
            CallDeferred(nameof(FinishClose));
        }

        public void CloseForShutdown()
        {
            // Free paused overlay audio before GameCore waits for mixer disposal.
            _overlayRoot?.Free();
            _contentInstance = null;
            _overlayRoot = null;
        }

        private void FinishClose()
        {
            // Drop any lingering key presses so the base scene doesn't see them.
            Input.FlushBufferedEvents(); // Godot 4.x

            GetTree().Paused = _wasPaused;
        }
    }

}
