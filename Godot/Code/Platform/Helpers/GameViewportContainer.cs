using Godot;
using System;

namespace Deuteros.Code.Platform.Helpers
{
    public partial class GameViewportContainer : SubViewportContainer
    {
        public static readonly Vector2I GameSize = new Vector2I(320, 200);

        public static GameViewportContainer Instance { get; private set; }

        public event Action LayoutChanged;

        public override void _Ready()
        {
            Instance = this;
            GetViewport().SizeChanged += UpdateLayout;
            UpdateLayout();
        }

        public override void _ExitTree()
        {
            if (Instance == this)
                Instance = null;

            if (IsInstanceValid(GetViewport()))
                GetViewport().SizeChanged -= UpdateLayout;
        }

        private void UpdateLayout()
        {
            var area = GetViewportRect().Size;
            var scale = Math.Max(1, (int)Math.Min(area.X / GameSize.X, area.Y / GameSize.Y));

            Size = GameSize;
            Scale = new Vector2(scale, scale);
            Position = ((area - (Vector2)GameSize * scale) / 2).Floor();

            LayoutChanged?.Invoke();
        }
    }
}
