using Deuteros.Code.Utility;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Utility
{
    using Godot;

    public partial class GlobalInput : CanvasLayer
    {
        [Export] public Texture2D CursorTexture;
        [Export] public Vector2 Hotspot = Vector2.Zero;
        public static bool UiLocked { get; private set; }
        private static Viewport _viewport;
        private TextureRect _cursor;

        // Lock state
        public bool IsLocked { get; private set; }
        public Rect2 LockRect { get; private set; }

        private bool _mouseInsideWindow = true;

        public Vector2 DisplayPos { get; private set; }

        public override void _Ready()
        {
            _viewport = GetViewport();

            _cursor = new TextureRect
            {
                Texture = CursorTexture,
                MouseFilter = Control.MouseFilterEnum.Ignore,
                ZIndex = (int)RenderingServer.CanvasItemZMax
            };
            _cursor.StretchMode = TextureRect.StretchModeEnum.Keep;
            AddChild(_cursor);

            // Show graphic cursor; do not capture or confine
            Input.MouseMode = Input.MouseModeEnum.Hidden;

            UiLocked = false;
        }

        public override void _ExitTree()
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }

        public override void _Process(double delta)
        {
            var osPos = GetViewport().GetMousePosition(); // real mouse pos in window coords
            DisplayPos = IsLocked ? ClampToRect(osPos, LockRect) : osPos;

            _cursor.Position = DisplayPos - Hotspot;
            _cursor.Visible = true;
        }

        public void LockToRect(Rect2 rectInViewportCoords)
        {
            LockRect = rectInViewportCoords;
            IsLocked = true;

            // Immediately clamp current display position
            DisplayPos = ClampToRect(GetViewport().GetMousePosition(), LockRect);
            _cursor.Position = DisplayPos - Hotspot;
        }

        public void Unlock()
        {
            IsLocked = false;
        }

        private static Vector2 ClampToRect(Vector2 p, Rect2 r)
        {
            // Clamp to inclusive edges
            var min = r.Position;
            var max = r.Position + r.Size;
            return new Vector2(
                Mathf.Clamp(p.X, min.X, max.X),
                Mathf.Clamp(p.Y, min.Y, max.Y)
            );
        }

        public static void LockUi()
        {
            UiLocked = true;

            // Hide & disable the mouse cursor
            Input.MouseMode = Input.MouseModeEnum.Hidden;

            // Disable ALL Control (UI) input: mouse + keyboard
            _viewport.GuiDisableInput = true;
        }

        public static void UnlockUi()
        {
            UiLocked = false;

            // Restore mouse cursor
            Input.MouseMode = Input.MouseModeEnum.Visible;

            // Re-enable UI input
            _viewport.GuiDisableInput = false;
        }
    }
}
