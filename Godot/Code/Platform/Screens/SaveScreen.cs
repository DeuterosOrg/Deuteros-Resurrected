using System;
using Deuteros.Code.Platform.Base;
using Deuteros.Code.Utility;
using Godot;

namespace Deuteros.Code.Platform.Screens
{
    public partial class SaveScreen : BaseSubScene
    {
        public SaveStorage Storage { get; set; }
        private Label status;
        private Label confirmation;
        private Button confirm;
        private Button cancel;
        private Action pending;

        public override void _Ready()
        {
            base._Ready();
            Storage ??= GameCore.SingletonInstance.Storage;
            var background = new ColorRect { Name = "Background", Position = new Vector2(72, 24),
                Size = new Vector2(246, 159), Color = Colors.Black, MouseFilter = Control.MouseFilterEnum.Ignore };
            AddChild(background);
            AddLabel("Title", "SAVE / LOAD", 82, 29, 230);
            for (var slot = 1; slot <= SaveStorage.QuickSlot; slot++)
            {
                var selected = slot;
                var y = 44 + (slot - 1) * 18;
                AddLabel($"Slot{slot}", "", 82, y + 2, 128);
                AddButton($"Save{slot}", "Save", 214, y, 42, () => RequestSave(selected)).TooltipText =
                    slot == SaveStorage.QuickSlot ? "Save to the separate Quick Save slot." : "";
                AddButton($"Load{slot}", "Load", 263, y, 42, () => RequestLoad(selected)).TooltipText =
                    slot == SaveStorage.QuickSlot ? "Load the Quick Save." : "";
            }
            status = AddLabel("Status", "Choose a slot.", 82, 151, 230);
            confirmation = AddLabel("Confirmation", "", 82, 165, 144);
            confirm = AddButton("Confirm", "Yes", 230, 164, 34, () =>
            {
                var operation = pending;
                ClearConfirmation();
                operation?.Invoke();
            });
            cancel = AddButton("Cancel", "No", 270, 164, 34, ClearConfirmation);
            ClearConfirmation();
            RefreshSlots();
        }

        private Label AddLabel(string name, string text, float x, float y, float width)
        {
            var label = new Label { Name = name, Text = text, Position = new Vector2(x, y),
                Size = new Vector2(width, 12), ClipText = true, MouseFilter = Control.MouseFilterEnum.Ignore };
            label.AddThemeFontSizeOverride("font_size", 8);
            AddChild(label);
            return label;
        }

        private Button AddButton(string name, string text, float x, float y, float width, Action action)
        {
            var button = new Button { Name = name, Text = text, Position = new Vector2(x, y), Size = new Vector2(width, 15) };
            button.AddThemeFontSizeOverride("font_size", 8);
            button.Pressed += action;
            AddChild(button);
            return button;
        }

        private void ClearConfirmation()
        {
            pending = null;
            confirmation.Text = "";
            confirm.Visible = cancel.Visible = false;
        }

        private void Ask(string message, Action operation)
        {
            pending = operation;
            confirmation.Text = message;
            confirm.Visible = cancel.Visible = true;
        }

        public void RefreshSlots()
        {
            for (var slot = 1; slot <= SaveStorage.QuickSlot; slot++)
            {
                var exists = Storage.Exists(slot);
                var label = GetNode<Label>($"Slot{slot}");
                var name = slot == SaveStorage.QuickSlot ? "Quick" : $"Slot {slot}";
                label.Text = $"{name}: empty";
                GetNode<Button>($"Load{slot}").Disabled = !exists;
                if (!exists) continue;
                try { label.Text = $"{(slot == SaveStorage.QuickSlot ? "Q" : slot.ToString())}: {Deuteros.Code.Objects.GameClock.FormatDate(Storage.Read(slot).Clock.DateCentidays)}"; }
                catch (Exception error) when (SaveStorage.IsSaveError(error)) { label.Text = $"{name}: unreadable"; }
            }
        }

        private void RequestSave(int slot)
        {
            ClearConfirmation();
            if (Storage.Exists(slot)) Ask(slot == SaveStorage.QuickSlot ? "Replace quick save?" : $"Replace slot {slot}?", () => WriteSlot(slot));
            else WriteSlot(slot);
        }

        private void WriteSlot(int slot)
        {
            try
            {
                Storage.Write(slot, GameCore.SingletonInstance.GameData.ActiveSaveFile);
                RefreshSlots();
                status.Text = slot == SaveStorage.QuickSlot ? "Quick saved." : $"Saved slot {slot}.";
            }
            catch (Exception error) when (SaveStorage.IsSaveError(error)) { status.Text = "Save failed; file kept."; }
        }

        private void RequestLoad(int slot)
        {
            ClearConfirmation();
            Ask("Load this game?", () =>
            {
                try
                {
                    var save = Storage.Read(slot);
                    GameCore.SingletonInstance.LoadSavedGame(save);
                }
                catch (Exception error) when (SaveStorage.IsSaveError(error)) { status.Text = "Cannot load this save."; }
            });
        }
    }
}
