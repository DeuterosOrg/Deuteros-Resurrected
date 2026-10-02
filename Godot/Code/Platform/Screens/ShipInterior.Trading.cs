using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Helpers;
using Godot;

namespace Deuteros.Code.Platform.Screens
{
    public partial class ShipInterior
    {
        private bool moduleInteractionInProgress;
        private Control tradeDecision;
        private TaskCompletionSource<bool?> pendingTrade;

        private async void ShipInterior_Pressed(int modulePressed)
        {
            if (moduleInteractionInProgress || OverlayManager.Instance.IsOpen || IsQueuedForDeletion()) return;
            moduleInteractionInProgress = true;
            try { await HandleModulePress(modulePressed); }
            catch (OperationCanceledException) { /* The owning scene left during its text/decision. */ }
            finally { moduleInteractionInProgress = false; }
        }

        private async Task<bool?> AskTradeDecision(Dictionary<ShipModule, Enums.ItemTypes> offered,
            Dictionary<ShipModule, Enums.ItemTypes> received, Dictionary<ShipModule, int> counts)
        {
            // Typing precedes the pause; simulation or scene replacement can invalidate an offer.
            bool OfferIsCurrent() => IsInsideTree() && !IsQueuedForDeletion()
                && Ship.ShipState == Enums.Ship_States.Docked && Ship.PlanetLocation == CurrentPlanet.PlanetId
                && CurrentPlanet.ActiveMethanoid && !GameCore.SingletonInstance.GameData.ActiveSaveFile.AtWar
                && GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Contains(Ship)
                && offered.All(pair => Ship.Modules.Contains(pair.Key)
                    && pair.Key.ModuleType == Enums.Module_Types.Supply
                    && pair.Key.ItemStored == pair.Value && pair.Key.ItemCount == counts[pair.Key]);
            if (!OfferIsCurrent()) return null;
            var dialog = OverlayManager.Instance.ShowOverlay(
                GD.Load<PackedScene>("res://Screens/Base/TradeDecision.tscn")) as Control;
            if (dialog == null) return null;
            tradeDecision = dialog;
            var completion = new TaskCompletionSource<bool?>();
            pendingTrade = completion;
            dialog.GetNode<Label>("Cargo/Items").Text = string.Join("\n", offered.Select(pair =>
                $"{pair.Key.ItemCount} {pair.Value.ToScreenString()}  >  {received[pair.Key].ToScreenString()}"));
            void Decide(bool accept)
            {
                // Resolve before closing: retained/repeated signals cannot settle twice.
                if (completion.TrySetResult(accept)) OverlayManager.Instance.CloseOverlay();
            }
            dialog.GetNode<Button>("Accept").Pressed += () => Decide(true);
            dialog.GetNode<Button>("Decline").Pressed += () => Decide(false);
            dialog.TreeExiting += () => completion.TrySetResult(null);
            try
            {
                var decision = await completion.Task;
                return decision.HasValue && OfferIsCurrent() ? decision : null;
            }
            finally
            {
                if (pendingTrade == completion) pendingTrade = null;
                if (tradeDecision == dialog) tradeDecision = null;
            }
        }

        public override void _ExitTree()
        {
            if (supplyPods != null && IsInstanceValid(supplyPods) && supplyPods.IsInsideTree())
                OverlayManager.Instance.CloseOverlay();
            if (tradeDecision != null && IsInstanceValid(tradeDecision) && tradeDecision.IsInsideTree())
                OverlayManager.Instance.CloseOverlay();
            pendingTrade?.TrySetResult(null);
            base._ExitTree();
        }
    }
}
