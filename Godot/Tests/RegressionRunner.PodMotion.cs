using System.Collections.Generic;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Platform.Screens.ShipBayScenes;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task PodFittingMotion()
        {
            var bay = await NavigationBay(Ship_Types.Shuttle);
            var torso = bay.GetNode<Torso>(ShipParts + "Torso1");
            var blocker = GameCore.SingletonInstance.GetNode<InputBlocker>("GameContainer/GameViewport/InputBlocker");
            if (!string.IsNullOrEmpty(OS.GetEnvironment("DEUTEROS_SCREENSHOT_DIR")))
            {
                Press(bay, "Buttons/ShipNav/Nav_Torso1");
                await ToSignal(GetTree().CreateTimer(1.1), SceneTreeTimer.SignalName.Timeout);
            }
            foreach (var fitting in new[] { (Module_Types.Supply, ItemTypes.supply_pod, "AddSupplyPod"),
                (Module_Types.Tool, ItemTypes.tool_pod, "AddToolPod"),
                (Module_Types.Cryo, ItemTypes.cryo_pod, "AddCryoPod") })
            {
                GameCore.SingletonInstance.GameData.GetItem(fitting.Item2).Locked = false;
                bay.ResourceList.Stores[fitting.Item2] = 1;
                Press(torso, "SpriteHolder/Buttons/" + fitting.Item3);
                torso.SetProcess(false);
                Equal(118f, torso.Cargo.Position.Y, "fitting starts below the bay floor");
                await CaptureDisplayEvidence("pod-" + fitting.Item1 + "-start");
                Equal(true, blocker.Blocked, "fitting owns input until settled");
                Equal(0, bay.ResourceList.Stores[fitting.Item2], "one spare charged");
                Press(torso, "SpriteHolder/Buttons/" + fitting.Item3);
                Equal(fitting.Item1, torso.Module.ModuleType, "repeat input cannot remove a moving pod");
                torso._Process(0.6);
                Equal(58f, torso.Cargo.Position.Y, "pod enters upward in original two-row steps");
                torso.UpdateState();
                Equal(58f, torso.Cargo.Position.Y, "refresh preserves in-flight artwork");
                await CaptureDisplayEvidence("pod-" + fitting.Item1 + "-entering");
                torso.SetProcess(true);
                var deadline = Time.GetTicksMsec() + 3000;
                while (blocker.Blocked && Time.GetTicksMsec() < deadline) await InputFrames();
                Equal(Vector2.Zero, torso.Cargo.Position, "fitting settles at the mount");
                Equal(false, blocker.Blocked, "fitting releases its lock");
                Equal(true, torso.Cargo.Visible, "fitted pod remains visible");
                await CaptureDisplayEvidence("pod-" + fitting.Item1 + "-fitted");
                Press(torso, "SpriteHolder/Buttons/" + fitting.Item3);
                torso.SetProcess(false);
                Equal(1f, torso.Cargo.Position.Y, "removal starts downward");
                Equal(1, bay.ResourceList.Stores[fitting.Item2], "old pod returned exactly once");
                torso._Process(0.6);
                Equal(61f, torso.Cargo.Position.Y, "removal follows original two-row steps");
                await CaptureDisplayEvidence("pod-" + fitting.Item1 + "-leaving");
                torso._Process(0.57);
                Equal(false, torso.Cargo.Visible, "removed pod disappears");
                Equal(false, blocker.Blocked, "removal releases its lock");
                Equal(Module_Types.None, torso.Module.ModuleType, "mount remains empty");
            }
        }

        private async Task PodReplacementAndExit()
        {
            var bay = await EmptyAssemblyBay(Ship_Types.SCG);
            bay.ResourceList.Stores[ItemTypes.g_chassis] = 1;
            Press(bay, "Buttons/Nav_Create_SCG");
            var torso = bay.GetNode<Torso>(ShipParts + "Torso6");
            var blocker = GameCore.SingletonInstance.GetNode<InputBlocker>("GameContainer/GameViewport/InputBlocker");
            var oldPod = bay.Ship.Modules[5];
            oldPod.ModuleType = Module_Types.Supply;
            torso.UpdateState();
            var oldTexture = torso.Component.Texture;
            GameCore.SingletonInstance.GameData.GetItem(ItemTypes.tool_pod).Locked = false;
            bay.ResourceList.Stores[ItemTypes.tool_pod] = 0;
            Press(torso, "SpriteHolder/Buttons/AddToolPod");
            Equal(false, blocker.Blocked, "rejected replacement starts no motion or lock");
            Equal(Module_Types.Supply, oldPod.ModuleType, "missing spare retains old pod");
            bay.ResourceList.Stores[ItemTypes.tool_pod] = 1;
            bay.ResourceList.Stores[ItemTypes.supply_pod] = 0;
            Press(torso, "SpriteHolder/Buttons/AddToolPod");
            torso.SetProcess(false);
            Equal(oldTexture, torso.Component.Texture, "replacement removes old artwork first");
            Press(bay, ShipParts + "Torso1/SpriteHolder/Buttons/AddSupplyPod");
            Equal(Module_Types.None, bay.Ship.Modules[0].ModuleType, "another mount cannot fit during the animation lock");
            torso._Process(1.16);
            Equal(118f, torso.Cargo.Position.Y, "replacement starts fitting after removal");
            Equal(true, torso.Component.Texture.ResourcePath.EndsWith("Component_ToolPod.png"), "replacement displays new pod");
            GameCore.LockScreen("unrelated test owner");
            var stores = bay.ResourceList.Stores;
            GameCore.SingletonInstance.ChangeScene(Scenes.Overview, new List<SceneVariables>());
            await InputFrames();
            Equal(true, blocker.Blocked, "scene exit preserves another owner's lock");
            GameCore.UnLockScreen();
            Equal(false, blocker.Blocked, "scene exit released its own lock");
            Equal(Module_Types.Tool, oldPod.ModuleType, "scene exit retains completed stock transaction");
            Equal(0, stores[ItemTypes.tool_pod], "exit never charges the spare twice");
            Equal(1, stores[ItemTypes.supply_pod], "exit never returns the old pod twice");
        }
    }
}
