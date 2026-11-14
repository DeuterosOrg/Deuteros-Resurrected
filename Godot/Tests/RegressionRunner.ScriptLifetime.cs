using System;
using System.Runtime.CompilerServices;
using Deuteros.Code;
using Deuteros.Code.Platform;
using Godot;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private void RunScriptLifetimeRegressions()
        {
            Check("Research production and store buttons remain usable during background collection", () =>
            {
                // Godot 4.2.1 can deadlock while a new C# script is registered and
                // the finalizer releases a previous prefab. Do not wait for the
                // finalizer between loads: that would hide the broken lock order.
                for (var cycle = 0; cycle < 60; cycle++)
                {
                    LoadAndPressSideButton(cycle % 3);
                    GC.Collect();
                }
                GC.WaitForPendingFinalizers();
            });
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private void LoadAndPressSideButton(int kind)
        {
            var names = new[] { "ResearchButton", "ProductionButton", "StoreButton" };
            var button = GD.Load<PackedScene>($"res://PreFabs/Buttons/{names[kind]}.tscn").Instantiate<Button>();
            try
            {
                var item = GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.derrick);
                switch (button)
                {
                    case ResearchButton research: research.ObjectData = item.Research; break;
                    case ProductionButton production: production.ObjectData = item; break;
                    case StoreButton store: store.ObjectData = item; break;
                    default: throw new InvalidOperationException("Prefab lost its concrete button script");
                }
                var signals = 0;
                var selected = -1;
                button.Connect("Clicked", Callable.From<int>(index => { signals++; selected = index; }));
                AddChild(button);
                button.EmitSignal(BaseButton.SignalName.Pressed);
                Equal(1, signals, "one selection per press after loading");
                Equal(kind == 0 ? 2 : 1, selected, "derrick research index or production order");
            }
            finally { button.Free(); }
        }
    }
}
