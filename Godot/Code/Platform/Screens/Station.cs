using System.Linq;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Base;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Code.Platform.Screens
{
    public partial class Station : BaseSubScene
    {
        private Label productLabel;
        private Label shuttleLabel;
        private Label derricksLabel;

        public override void _Ready()
        {
            productLabel = GetNode<Label>("Labels/Product");
            shuttleLabel = GetNode<Label>("Labels/Shuttle");
            derricksLabel = GetNode<Label>("Labels/Derricks");
            base._Ready();
            DrawStatus();
        }

        protected override void DayTick(uint previousDay, uint currentDay) => DrawStatus();
        protected override void PlanetChange(Objects.Interfaces.IPlanet planet) => DrawStatus();

        private void DrawStatus()
        {
            var core = GameCore.SingletonInstance;
            var planet = core.GetCurrentPlanet();
            var product = planet.Station.Factory.CurrentProductionItem()?.Product.ShortName ?? "None";
            productLabel.Text = "Product\n" + product;
            productLabel.TooltipText = product;
            var shuttle = core.GameData.ActiveSaveFile.Ships.OfType<Shuttle>()
                .FirstOrDefault(ship => ship.PlanetLocation == planet.PlanetId);
            var status = shuttle == null ? "None" : shuttle.ShipState switch
            {
                Ship_States.TakingOff => "Takeoff",
                Ship_States.Landing => "Landing",
                Ship_States.Launching => "Launch",
                Ship_States.Docking => "Docking",
                Ship_States.InTransit => "Transit",
                Ship_States.CrewRepairing => "Repairs",
                _ => "Idle"
            };
            shuttleLabel.Text = "Shuttle\n" + status;
            derricksLabel.Text = planet.PlanetResources.Derricks + " Derricks\nDeployed";
        }
    }
}
