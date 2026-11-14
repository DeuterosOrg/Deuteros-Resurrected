using Deuteros.Code.Platform.Base;
using Godot;
using System;

namespace Deuteros.Code.Platform.Screens
{
    public partial class GroundMaterials : BaseSubScene
    {
        public Label DerrickCount { get; set; }
        public Label MaterialNames { get; set; }
        public Label MaterialAmounts { get; set; }
        public Button AddDerrick { get; set; }
        public Texture2D GreenArrow { get; set; }
        public GridContainer GreenArrows { get; set; }

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            DerrickCount = (Label)GetNode("DerrickCount");
            MaterialNames = (Label)GetNode("MaterialNames");
            MaterialAmounts = (Label)GetNode("MaterialAmounts");
            GreenArrows = (GridContainer)GetNode("GreenArrows");
            
            AddDerrick = (Button)GetNode("Derrick/AddDerrick");
            AddDerrick.Connect("button_up", new Callable(this, nameof(AddDerrick_ButtonUp)));

            GreenArrow = (Texture2D)ResourceLoader.Load("res://Sprites/Buttons/GreenArrowRight.png");

            base._Ready();
        }

        private void AddDerrick_ButtonUp()
        {
            var currentPlanet = Deuteros.Code.GameCore.SingletonInstance.GetCurrentPlanet();

            if (currentPlanet.PlanetResources.Derricks < 8 && currentPlanet.PlanetResources.Stores[Deuteros.Code.Enums.ItemTypes.derrick] > 0)
            {
                currentPlanet.PlanetResources.Derricks++;
                currentPlanet.PlanetResources.Stores[Deuteros.Code.Enums.ItemTypes.derrick]--;
            }
        }

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            var currentPlanet = Deuteros.Code.GameCore.SingletonInstance.GetCurrentPlanet();
            DerrickCount.Text = currentPlanet.PlanetResources.Derricks.ToString();
        }

        // Called every update.
        public override void _Draw()
        {
            DrawMinerals(Deuteros.Code.GameCore.SingletonInstance.GetCurrentPlanet());
        }

        public void DrawMinerals(Deuteros.Code.Objects.Interfaces.IPlanet currentPlanet)
        {
            var textureStartY = 20;
            var lineSizeY = 10;
            var greenArrowX = 1;
            var minCount = 0;
            MaterialNames.Text = "";
            MaterialAmounts.Text = "";

            foreach (Node child in GreenArrows.GetChildren())
            {
                child.QueueFree();
            }

            foreach (var mineral in currentPlanet.PlanetResources.Materials)
            {
                var mineralText = mineral.IsSurveying ? "SURVEY" : mineral.GroundAmount.ToString();

                MaterialNames.Text += mineral.MaterialType.ToString() + "\n";
                MaterialAmounts.Text += mineralText + "\n";
                                
                this.DrawTexture(GreenArrow, new Vector2(greenArrowX, textureStartY + (lineSizeY * minCount)));

                var greenArrow = new TextureRect();
                greenArrow.Texture = GreenArrow;
                GreenArrows.AddChild(greenArrow);

                minCount++;
            }
        }
    }
}