using Deuteros.Code.Utility;
using Deuteros.Code.Platform.Helpers;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Code.Platform.Screens
{
    public partial class ShipInterior
    {
        private Control supplyPods;

        private void SupplyPods_Pressed()
        {
            if (RejectShipCommand()) return;
            var core = GameCore.SingletonInstance;
            if (!IsInsideTree() || IsQueuedForDeletion() || moduleInteractionInProgress ||
                GlobalInput.UiLocked || core.GetNode<InputBlocker>("InputBlocker").Blocked ||
                core.GetNode<GlobalInput>("VirtualCursorView").IsLocked || OverlayManager.Instance.IsOpen)
                return;

            var dialog = OverlayManager.Instance.ShowOverlay(
                GD.Load<PackedScene>("res://Screens/Base/SupplyPods.tscn")) as Control;
            if (dialog == null) return;
            supplyPods = dialog;
            GameCore.HoverText = "";
            var ship = Ship;
            var rows = dialog.GetNode<VBoxContainer>("Rows");
            for (var index = 0; index < ship.Modules.Count; index++)
            {
                var slot = index;
                var module = ship.Modules[slot];
                var item = module.ItemStored;
                var count = module.ItemCount;
                var row = new HBoxContainer { Name = "Pod" + slot };
                rows.AddChild(row);
                var description = module.ModuleType == Module_Types.Supply
                    ? (count > 0 ? count + " " + item.ToScreenString(" ") : "Empty")
                    : module.ModuleType.ToString();
                var label = new Label { Name = "Cargo", Text = $"{slot + 1}: {description}", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
                row.AddChild(label);
                var ditch = new Button { Name = "Ditch", Text = "Ditch", CustomMinimumSize = new Vector2(52, 16),
                    Disabled = module.ModuleType != Module_Types.Supply || count <= 0 };
                row.AddChild(ditch);
                ditch.Pressed += () =>
                {
                    if (RejectShipCommand(dialog) || ditch.Disabled || supplyPods != dialog || dialog.IsQueuedForDeletion() ||
                        !OverlayManager.Instance.IsOpen || !IsInsideTree() || IsQueuedForDeletion() ||
                        !core.GameData.ActiveSaveFile.Ships.Contains(ship) || slot >= ship.Modules.Count ||
                        ship.Modules[slot] != module || module.ModuleType != Module_Types.Supply ||
                        module.ItemStored != item || module.ItemCount != count)
                        return;

                    // Original handler $33F40 retains pod kind and discards type/quantity.
                    // No store transfer, journey change or mining-cycle reset occurs here.
                    ditch.Disabled = true;
                    module.ItemStored = ItemTypes.none;
                    module.ItemCount = 0;
                    label.Text = $"{slot + 1}: Empty";
                    UpdateState();
                };
            }
            dialog.GetNode<Button>("Close").Pressed += OverlayManager.Instance.CloseOverlay;
            dialog.TreeExiting += () => { if (supplyPods == dialog) supplyPods = null; };
        }
    }
}
