using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Platform.Screens.ShipBayScenes
{
    public partial class Engine : Control
    {
        public Control SpriteHolder { get; set; }
        public Control EngineHolder { get; set; }
        public TextureButton InstallEngineButton { get; set; }
        public bool Installed { get; set; }
        public bool Damaged { get; set; }
        public Enums.Ship_Types Shiptype { get; set; }
        public bool Ground { get; set; }
        public Enums.ItemTypes EngineType { get; set; }

        public delegate void EngineInstalledDelegate();
        public event EngineInstalledDelegate EngineInstalled;
        public Func<bool> MayInstall { get; set; }

        public override void _Ready()
        {
            SpriteHolder = GetNode<Control>("SpriteHolder");
            EngineHolder = GetNode<Control>("SpriteHolder/EngineHolder");
            InstallEngineButton = GetNode<TextureButton>("SpriteHolder/Buttons/InstallEngine");

            InstallEngineButton.Pressed += InstallEngine;
        }

        public void UpdateState()
        {
            EngineHolder.Visible = Installed;
            InstallEngineButton.TooltipText = Damaged ? "Replace damaged drive" : "Install drive";
        }

        public void InstallEngine()
        {
            if (MayInstall?.Invoke() == false) return;
            var planet = GameCore.SingletonInstance.GetCurrentPlanet();
            var stores = Ground ? planet.PlanetResources.Stores : planet.Station.Resources.Stores;
            if ((!Installed || Damaged) && stores[EngineType] > 0)
            {
                stores[EngineType]--;
                Installed = true;
                Damaged = false;
                EngineInstalled.Invoke();
                UpdateState();
            }
        }
    }
}