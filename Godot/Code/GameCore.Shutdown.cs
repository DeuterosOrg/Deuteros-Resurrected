using Godot;

namespace Deuteros.Code
{
    public partial class GameCore
    {
        private bool quitRequested;

        public override void _Notification(int what)
        {
            if (what == NotificationWMCloseRequest) RequestQuit();
        }

        public void RequestQuit(int exitCode = 0)
        {
            if (quitRequested) return;
            quitRequested = true;
            // Window-close notifications can arrive during input dispatch.
            CallDeferred(nameof(FinishQuit), exitCode);
        }

        private async void FinishQuit(int exitCode)
        {
            var tree = GetTree();
            SetProcess(false);
            SetProcessInput(false);
            tree.Paused = true;
            // Scene exit cancels typing and stops background players. Do this
            // while the mixer and managed runtime are still available.
            foreach (var scene in GetNode("MainScene").GetChildren()) scene.Free();
            _currentScreen = null;
            _menuScreen = null;

            // Godot 4.2 defers stopped playback disposal to its audio mixer.
            // Wait for observed mixes, rather than an arbitrary frame count.
            var deadline = Time.GetTicksMsec() + 2000;
            var previousMixAge = AudioServer.GetTimeSinceLastMix();
            var observedMixes = 0;
            while (observedMixes < 2 && Time.GetTicksMsec() < deadline)
            {
                await ToSignal(tree, SceneTree.SignalName.ProcessFrame);
                var mixAge = AudioServer.GetTimeSinceLastMix();
                if (mixAge < previousMixAge) observedMixes++;
                previousMixAge = mixAge;
            }
            if (observedMixes < 2)
            {
                GD.PushError("Audio mixer did not finish shutdown within two seconds.");
                if (exitCode == 0) exitCode = 1;
            }
            if (OS.IsDebugBuild())
            {
                // The audio wait above also advances beyond the input frame.
                // A native-only replacement releases Godot 4.2.2's debug input
                // cache before C# bindings are torn down (upstream #92201).
                using var inputCleanup = GD.Load<GDScript>("res://Code/Utility/ShutdownInput.gd").New().AsGodotObject();
                inputCleanup.Call("release_managed_events", tree.Root);
            }
            // Godot 4.2's shutdown tracker skips wrappers whose weak targets
            // were cleared while their native-resource finalizers are pending.
            // Drain those finalizers while the engine and bindings still exist.
            // This collection runs only on exit, after scene/audio release.
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();
            tree.Quit(exitCode);
        }
    }
}
