using Godot;

namespace Deuteros.Code.Platform.Helpers;

// First child of the game viewport: reverse input order lets screen modals handle the event first.
public partial class GameInput : Node
{
    public override void _Input(InputEvent input)
    {
        if (input is InputEventMouseButton)
            GameCore.SingletonInstance.HandleGameInput(input);
    }

    public override void _UnhandledKeyInput(InputEvent input) => GameCore.SingletonInstance.HandleGameInput(input);
}
