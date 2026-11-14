using Deuteros.Code.Utility;
using Godot;
using System.Collections.Generic;
using System.Linq;
using static Deuteros.Code.Enums;


namespace Deuteros.Code.Platform.Helpers
{
    public partial class InputBlocker : ColorRect
    {
        public bool Blocked { get; set; } = false;

        public override void _Ready()
        {
            MouseFilter = MouseFilterEnum.Stop;
            Visible = Blocked;

            // Force correct size now + on resize
            FitToViewport();
            GetViewport().SizeChanged += FitToViewport;
        }

        public override void _ExitTree()
        {
            if (IsInstanceValid(GetViewport()))
                GetViewport().SizeChanged -= FitToViewport;
        }

        public void SetBlocked(bool blocked)
        {
            Blocked = blocked;
            Visible = blocked;

            if (blocked)
                GrabFocus();
        }

        private void FitToViewport()
        {
            // This does NOT depend on anchors/containers/inspector settings.
            Position = Vector2.Zero;
            Size = GetViewportRect().Size;
            Color = Colors.Transparent;
        }

        public override void _GuiInput(InputEvent @event)
        {
            if (!Blocked) return;
            AcceptEvent(); // eats pointer GUI events
        }

        public override void _UnhandledInput(InputEvent @event)
        {
            if (!Blocked) return;

            GetViewport().SetInputAsHandled(); // eats keyboard/gamepad too
        }
    }
}