using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Godot;
using static Deuteros.Code.Enums;
using TrainingScreen = Deuteros.Code.Platform.Screens.Training;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private TrainingScreen OpenTraining()
        {
            InitializeUi();
            Save.CurrentPlanet = StellarBodies.earth;
            GameCore.SingletonInstance.ChangeScene(Scenes.Earth_Training, new List<SceneVariables> { SceneVariables.Ground });
            return ActiveScreen<TrainingScreen>();
        }

        private async Task TrainingLighting()
        {
            var screen = OpenTraining();
            await InputFrames();
            Image before = null;
            if (DisplayServer.GetName() != "headless")
            {
                await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                before = GetViewport().GetTexture().GetImage();
            }
            screen.LightSwitchButton_Pressed();
            if (before != null)
            {
                await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
                using var dimmed = GetViewport().GetTexture().GetImage();
                var checkedPixels = 0;
                for (var y = 120; y < 136; y++)
                    for (var x = 136; x < 150; x++)
                    {
                        var colour = before.GetPixel(x, y);
                        if (colour.R < 0.8f || colour.G < 0.8f || colour.B < 0.8f) continue;
                        Equal(true, dimmed.GetPixel(x, y).IsEqualApprox(colour), "rendered arrow retains brightness");
                        checkedPixels++;
                    }
                before.Dispose();
                Equal(true, checkedPixels > 0, "checked visible arrow pixels");
            }
            Equal(new Color(0.5f, 0.5f, 0.5f), screen.GetChildren().OfType<CanvasModulate>().Single().Color, "switch dims room");
            Equal(new Color(2, 2, 2), screen.GetNode<Node2D>("Doors").Modulate, "door controls retain brightness");
            await CaptureDisplayEvidence("training-light-dimmed");
            screen.LightSwitchButton_Pressed();
            Equal(Colors.White, screen.GetChildren().OfType<CanvasModulate>().Single().Color, "second switch restores room");
            Equal(Colors.White, screen.GetNode<Node2D>("Doors").Modulate, "second switch restores controls");
            screen.LightSwitchButton_Pressed();
            GameCore.SingletonInstance.ChangeScene(Scenes.Earth_Ground, new List<SceneVariables> { SceneVariables.Ground });
            await InputFrames();
            Equal(false, GodotObject.IsInstanceValid(screen), "leaving frees dimmed training room");
        }

        private async Task TrainingDoorAudio()
        {
            var screen = OpenTraining();
            await InputFrames();
            var buttons = screen.GetNode<AudioStreamPlayer>("SoundPlayer");
            Equal(false, buttons.Playing, "opening a static screen is silent");
            var training = GameCore.Earth.TrainingData;
            training.ResearcherTrainingCount = training.ProductionTrainingCount = training.MarinesTrainingCount = 1;
            training.ChildDayTick(0, 1);
            screen.DrawData();
            var sounds = screen.FindChildren("*", "AudioStreamPlayer", true, false).OfType<AudioStreamPlayer>();
            var door = sounds.SingleOrDefault(p => p.Stream?.ResourcePath == "res://Sounds/Button/sTrainingRoom_Door.wav");
            Equal(true, door != null && door.Playing, "simultaneous closing plays one original door cue");
            Equal("Game", door.Bus.ToString(), "door obeys game audio preferences");
            foreach (var name in new[] { "Research", "Production", "Marines" })
                Equal("close", screen.GetNode<AnimatedSprite2D>("Doors/" + name + "/TrainingDoors/DoorAnimation").Animation.ToString(), "door closes");
            door.Stop();
            screen.DrawData();
            Equal(false, door.Playing, "unchanged transition does not replay cue");
            await ToSignal(GetTree().CreateTimer(1.6), SceneTreeTimer.SignalName.Timeout);
            training.ResearcherLocked = training.ProductionLocked = training.MarinesLocked = false;
            screen.DrawData();
            Equal(true, door.Playing, "opening also plays door cue");
            Equal("res://Sounds/Button/sTrainingRoom_Button.wav", buttons.Stream.ResourcePath, "door cue preserves button sample");
            await ToSignal(GetTree().CreateTimer(1.6), SceneTreeTimer.SignalName.Timeout);
            screen.ResearchPlusButton_Pressed();
            Equal(true, buttons.Playing, "button feedback still plays after door motion");
            training.ResearcherLocked = true;
            screen.DrawData();
            Equal(true, door.Playing, "exit interrupts an active door cue");
            var parent = screen.GetParent();
            parent.RemoveChild(screen);
            Equal(false, door.Playing, "leaving stops door audio");
            Equal(false, buttons.Playing, "leaving stops button audio");
            screen.Free();
            await DrainStoppedAudio();
        }

        private async Task TrainingDoorLifecycle()
        {
            var training = GameCore.Earth.TrainingData;
            training.ResearcherLocked = true;
            var screen = OpenTraining();
            var core = GameCore.SingletonInstance;
            var blocker = core.GetNode<Deuteros.Code.Platform.Helpers.InputBlocker>("InputBlocker");
            GameCore.LockScreen("another owner");
            await ToSignal(GetTree().CreateTimer(0.4), SceneTreeTimer.SignalName.Timeout);
            Equal(true, blocker.Blocked, "static closed door cannot release another owner's lock");
            Equal(false, screen.GetNode<AudioStreamPlayer>("DoorSoundPlayer").Playing, "initial closed door is silent");
            GameCore.UnLockScreen();

            training.ResearcherLocked = false;
            screen.DrawData();
            var research = screen.GetNode<AnimatedSprite2D>("Doors/Research/TrainingDoors/DoorAnimation");
            var production = screen.GetNode<AnimatedSprite2D>("Doors/Production/TrainingDoors/DoorAnimation");
            training.ProductionLocked = true;
            screen.DrawData();
            Equal("opening", research.Animation.ToString(), "research starts opening");
            Equal("close", production.Animation.ToString(), "production starts closing independently");
            // Deliver the first door's completion while the second is still moving.
            research.Stop();
            research.EmitSignal(AnimatedSprite2D.SignalName.AnimationFinished);
            Equal("open", research.Animation.ToString(), "only completed door settles");
            Equal("close", production.Animation.ToString(), "other door is not cut short");
            Equal(true, blocker.Blocked, "other door retains its lock");
            research.EmitSignal(AnimatedSprite2D.SignalName.AnimationFinished);
            Equal(true, blocker.Blocked, "duplicate completion cannot release other door's lock");
            GameCore.LockScreen("another owner during interruption");
            core.ChangeScene(Scenes.Earth_Ground, new List<SceneVariables> { SceneVariables.Ground });
            await InputFrames();
            Equal(true, blocker.Blocked, "leaving moving doors preserves unrelated lock");
            GameCore.UnLockScreen();
            Equal(false, blocker.Blocked, "leaving released every training-owned lock");
        }
    }
}
