using System.Collections.Generic;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Platform.Screens;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        public async Task RunTimeAnimationRegressions()
        {
            foreach (var scene in new[] { Scenes.Earth_Ground, Scenes.Overview, Scenes.News, Scenes.SaveScreen, Scenes.Store })
                await CheckAsync($"Time device animates independently of {scene} menu context", () => TimeToggleAnimation(scene));
            await CheckAsync("Time device follows hold release and pointer exit", TimeHoldAnimation);
            await CheckAsync("Time device follows external clock stops without restarting unchanged frames", TimeAnimationStateChanges);
            await CheckAsync("Blocked time controls cannot start clock or animation", TimeAnimationInputLock);
        }

        private MainMenu TimeMenu(Scenes scene)
        {
            InitializeUi();
            // Exercise the real persistent menu with each context consumed by its controller.
            // Background scenes are unnecessary here and enter the separate audio shutdown defect.
            GameCore.SingletonInstance.currentScene = scene;
            GameCore.SingletonInstance.SceneVariables = new List<SceneVariables>
                { scene == Scenes.Earth_Ground ? SceneVariables.Ground : SceneVariables.Orbit };
            Save.TimeSkip = Save.TimeSkipDay = false;
            var menu = ActiveRecipeScreen<MainMenu>();
            menu.UpdateAnimations();
            return menu;
        }

        private async Task TimeToggleAnimation(Scenes scene)
        {
            var menu = TimeMenu(scene);
            try
            {
                menu.TimeButton.EmitSignal(BaseButton.SignalName.Pressed);
                await InputFrames();
                Equal(true, Save.TimeSkip, "real toggle starts advancement");
                Equal("animated", menu.TimeAnimation.Animation.ToString(), "time device independent of selected screen");
                var selected = scene == Scenes.Earth_Ground ? menu.EarthAnimation : scene == Scenes.Overview ? menu.MasterControlAnimation
                    : scene == Scenes.News ? menu.NewsAnimation : scene == Scenes.SaveScreen ? menu.SaveAnimation : menu.StockAnimation;
                Equal("animated", selected.Animation.ToString(), "selected-screen indicator remains active");
                menu.TimeButton.EmitSignal(BaseButton.SignalName.Pressed);
                await InputFrames();
                Equal(false, Save.TimeSkip, "real toggle stops advancement");
                Equal("static", menu.TimeAnimation.Animation.ToString(), "stopped clock uses static frame");
            }
            finally { Save.TimeSkip = Save.TimeSkipDay = false; }
        }

        private async Task TimeHoldAnimation()
        {
            var menu = TimeMenu(Scenes.SaveScreen);
            var hold = menu.GetNode<BaseButton>("Time/TimeBox/TimerHoldButton");
            try
            {
                hold.EmitSignal(BaseButton.SignalName.ButtonDown);
                await InputFrames();
                Equal(true, Save.TimeSkip, "hold starts advancement");
                Equal("animated", menu.TimeAnimation.Animation.ToString(), "hold starts animation without menu rebuild");
                hold.EmitSignal(BaseButton.SignalName.ButtonUp);
                await InputFrames();
                Equal(false, Save.TimeSkip, "release ends hold");
                Equal("static", menu.TimeAnimation.Animation.ToString(), "release settles animation");
                hold.EmitSignal(BaseButton.SignalName.ButtonDown);
                await InputFrames();
                hold.EmitSignal(Control.SignalName.MouseExited);
                await InputFrames();
                Equal(false, Save.TimeSkip, "pointer exit ends hold");
                Equal("static", menu.TimeAnimation.Animation.ToString(), "pointer exit settles animation");
            }
            finally { Save.TimeSkip = Save.TimeSkipDay = false; }
        }

        private async Task TimeAnimationStateChanges()
        {
            var menu = TimeMenu(Scenes.Overview);
            try
            {
                menu.TimeButton.EmitSignal(BaseButton.SignalName.Pressed);
                await InputFrames();
                Equal("animated", menu.TimeAnimation.Animation.ToString(), "clock running");
                menu.TimeAnimation.Frame = 3;
                menu.TimeAnimation.FrameProgress = 0.5f;
                menu.UpdateAnimations();
                Equal(3, menu.TimeAnimation.Frame, "unrelated icon refresh preserves time frame");
                Equal(0.5f, menu.TimeAnimation.FrameProgress, "unrelated icon refresh preserves partial frame");
                // Simulation completions and encounters stop the clock by changing this flag,
                // without pressing the time control or notifying MainMenu.
                Save.TimeSkip = false;
                await InputFrames();
                Equal("static", menu.TimeAnimation.Animation.ToString(), "external stop observed next frame");
                Equal(false, Save.TimeSkip, "view refresh does not restart simulation");
            }
            finally { Save.TimeSkip = Save.TimeSkipDay = false; }
        }

        private async Task TimeAnimationInputLock()
        {
            var menu = TimeMenu(Scenes.SaveScreen);
            var originalParent = menu.GetParent();
            var viewport = new SubViewport { Size = new Vector2I(320, 200), GuiDisableInput = false };
            AddChild(viewport);
            menu.Reparent(viewport);
            var blocker = new InputBlocker();
            viewport.AddChild(blocker);
            async Task ClickTime()
            {
                var point = menu.TimeButton.GetGlobalRect().GetCenter();
                var motion = new InputEventMouseMotion { Position = point, GlobalPosition = point };
                viewport.PushInput(motion, true);
                foreach (var pressed in new[] { true, false })
                {
                    var click = new InputEventMouseButton { ButtonIndex = MouseButton.Left, Pressed = pressed,
                        Position = point, GlobalPosition = point };
                    viewport.PushInput(click, true);
                }
                await InputFrames();
            }
            try
            {
                await InputFrames();
                blocker.SetBlocked(true);
                await ClickTime();
                Equal(false, Save.TimeSkip, "input blocker prevents real pointer activation");
                Equal("static", menu.TimeAnimation.Animation.ToString(), "blocked click does not animate");
                blocker.SetBlocked(false);
                await ClickTime();
                Equal(true, Save.TimeSkip, "unblocked pointer starts clock");
                Equal("animated", menu.TimeAnimation.Animation.ToString(), "unblocked pointer starts animation");
            }
            finally
            {
                Save.TimeSkip = Save.TimeSkipDay = false;
                menu.Reparent(originalParent);
                viewport.Free();
            }
        }
    }
}
