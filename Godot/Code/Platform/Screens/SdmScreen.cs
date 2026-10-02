using Deuteros.Code.Objects;
using Deuteros.Code.Objects.Interfaces;
using Deuteros.Code.Platform.Base;
using Godot;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Utility;

namespace Deuteros.Code.Platform.Screens
{
    public partial class SdmScreen : BaseSubScene
    {
        private IPlanet planet;
        private int switches;
        private Button low;
        private Button high;
        private Label status;

        public override void _Ready()
        {
            planet = GameCore.SingletonInstance.GetCurrentPlanet();
            switches = planet.Station.SdmCountdown == 0 ? 0x0200 : 0x0002;
            low = GetNode<Button>("LowSwitch");
            high = GetNode<Button>("HighSwitch");
            status = GetNode<Label>("Status");
            low.Pressed += () => Toggle(false);
            high.Pressed += () => Toggle(true);
            base._Ready();
            Refresh();
            if (SdmSystem.CanAccess(GameCore.SingletonInstance.GameData.ActiveSaveFile, planet) && !Discovered)
                CallDeferred(nameof(Discover));
        }

        private bool Discovered => !GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.s__d__m).Research.Locked;
        private bool Interlocked => planet.ActiveMethanoid && (switches & 0x0200) != 0
            && !GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.hyperlight).Research.Locked;

        public void Discover()
        {
            var core = GameCore.SingletonInstance;
            if (!IsInsideTree() || IsQueuedForDeletion() || Discovered || !SdmSystem.CanAccess(core.GameData.ActiveSaveFile, planet)) return;
            core.TriggerAlienTechDiscovery(Enums.ItemTypes.s__d__m);
        }

        private void Toggle(bool upper)
        {
            var core = GameCore.SingletonInstance;
            if (!Discovered || (!upper && Interlocked) || GlobalInput.UiLocked || GetTree().Paused || OverlayManager.Instance.IsOpen
                || !SdmSystem.CanAccess(core.GameData.ActiveSaveFile, planet)) return;
            switches ^= upper ? 0x0200 : 0x0002;
            SdmSystem.ApplySwitches(core.GameData.ActiveSaveFile, planet, switches);
            core.UpdateMenuButtons(false, true);
            Refresh();
        }

        private void Refresh()
        {
            var available = SdmSystem.CanAccess(GameCore.SingletonInstance.GameData.ActiveSaveFile, planet);
            high.Disabled = !available || !Discovered;
            low.Disabled = high.Disabled || Interlocked;
            low.TooltipText = Interlocked ? "Switch interlocked" : "";
            low.Text = "Switch 1: " + ((switches & 2) != 0 ? "ON" : "OFF");
            high.Text = "Switch 2: " + ((switches & 0x200) != 0 ? "ON" : "OFF");
            var count = planet.Station.SdmCountdown & 0x7f;
            status.Text = !available ? "Mechanism unavailable" : count == 0 ? "DISARMED" : $"ARMED  {count / 60:00}:{count % 60:00}";
        }

        public override void _Process(double delta) => Refresh();
    }
}
