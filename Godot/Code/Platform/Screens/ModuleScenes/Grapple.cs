using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Base;
using Deuteros.Code.Platform.Helpers;
using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Deuteros.Code.Enums;

namespace Deuteros.Code.Platform.Screens.ModuleScenes
{
	public partial class Grapple : BaseSubScene
	{
		public const string NavSpriteBasePath = "res://Sprites//SceneSprites//";

		Control Enabled {  get; set; }
		Control Disabled { get; set; }

		TextureButton ReleaseButton { get; set; }
		TextureButton GrabButton { get; set; }

		TextureRect AsteroidSprite { get; set; }

		Label ScannerAnalysisLabel { get; set; }
		Label ScannerStatusLabel { get; set; }
		Label MassLabel { get; set; }
		Label ElementLabel { get; set; }
		Label ContentsLabel { get; set; }
		Label ToolContentsLabel { get; set; }

		ShipModule ShipModule { get; set; }
		Ship Ship { get; set; }
		
		public Action UpdateParent;

		public override void _Ready()
		{
			GetNode<Node2D>("Window").GetNode<Label>("Background/Number").Text = "1";

			Enabled = GetNode<Control>("Enabled");
			Disabled = GetNode<Control>("Disabled");

			ReleaseButton = GetNode<TextureButton>("Enabled/Buttons/Release");
			GrabButton = GetNode<TextureButton>("Enabled/Buttons/Grab");

			ScannerAnalysisLabel = GetNode<Label>("Enabled/Labels/ScannerAnalysis");
			ScannerStatusLabel = GetNode<Label>("Enabled/Labels/ScannerStatus");
			MassLabel = GetNode<Label>("Enabled/Labels/Mass");
			ElementLabel = GetNode<Label>("Enabled/Labels/Element");
			ContentsLabel = GetNode<Label>("Enabled/Labels/Contents");

			ToolContentsLabel = GetNode<Label>("Disabled/ToolContents");

			AsteroidSprite = GetNode<TextureRect>("Enabled/Sprites/Asteroid");

			ReleaseButton.Pressed += ReleaseButton_Pressed;
			GrabButton.Pressed += GrabButton_Pressed;

			base._Ready();
		}

		public void Load(Ship ship, ShipModule shipModule)
		{
			Ship = ship;
			ShipModule = shipModule;
			
			UpdateState();
		}

		private void GrabButton_Pressed()
		{
			if (Ship.ShipState == Ship_States.UnDocked)
			{

                if ((((InterStellarShip)Ship).AsteroidScanResults != null) && ((InterStellarShip)Ship).AsteroidScanResults.Mass <= 250)
				{
					var heldAsteroid = ((InterStellarShip)Ship).AsteroidScanResults;
					ShipModule.HeldItem = heldAsteroid;
					((InterStellarShip)Ship).ItemScanResults = null;
				}
				else if (((InterStellarShip)Ship).ItemScanResults != null)
				{
                    var heldItem = ((InterStellarShip)Ship).ItemScanResults;
                    ShipModule.HeldItem = heldItem;

					//artifact has been captured
					GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Stars[Ship.StarLocation].ArtifactLocation = StellarBodies.none;

                    ((InterStellarShip)Ship).ItemScanResults = null;
                }
                UpdateState();

				UpdateParent?.Invoke();
			}
		}

		private void ReleaseButton_Pressed()
		{
			if (ShipModule.HeldItem != null)
			{
				ShipModule.HeldItem = null;

				UpdateState();

				UpdateParent?.Invoke();
			}
		}

		protected override void DayTick(uint previousDay, uint currentDay)
		{
			UpdateState();

			base.DayTick(previousDay, currentDay);
		}

		public void UpdateState()
		{
			if (Ship.Pilot != null && Ship.Pilot.GetLevel() == 1 || Ship.ShipState != Ship_States.UnDocked)
			{
				Disabled.Visible = true;
				Enabled.Visible = false;

				if (ShipModule.HeldItem != null)
				{
					if (ShipModule.HeldItem.GrappleItemType == GrappleItemTypes.Asteroid)
					{
						var heldAsteroid = (Asteroid)ShipModule.HeldItem;

						ToolContentsLabel.Text = heldAsteroid.Mass + " " + heldAsteroid.Type.ToScreenString();
					}
                    else if (ShipModule.HeldItem.GrappleItemType == GrappleItemTypes.UnknownItem)
					{
                        ToolContentsLabel.Text = "Unknown";
                    }
                }
				else
				{
					ToolContentsLabel.Text = "None";
				}
			} 
			else
			{
				Disabled.Visible = false;
				Enabled.Visible = true;

				if (ShipModule.HeldItem != null)
				{
					if (ShipModule.HeldItem.GrappleItemType == GrappleItemTypes.Asteroid)
					{
						var heldAsteroid = (Asteroid)ShipModule.HeldItem;

						AsteroidSprite.Texture = SpriteManager.LoadImage(NavSpriteBasePath + "Asteroid_" + heldAsteroid.MassName.ToString() + ".png");
						ContentsLabel.Text = "Asteroid\r\n" + heldAsteroid.Mass.ToString() + "t.\r\n" + heldAsteroid.Type.ToScreenString();
					}
					else if (ShipModule.HeldItem.GrappleItemType == GrappleItemTypes.UnknownItem)
					{
						AsteroidSprite.Texture = null;
                        ContentsLabel.Text = "Unknown";
                    }

                }
                else
				{
					AsteroidSprite.Texture = null;
					ContentsLabel.Text = "None";
				}

				if (((InterStellarShip)Ship).AsteroidScanResults != null)
				{
					var currentAsteroid = ((InterStellarShip)Ship).AsteroidScanResults;
					ScannerStatusLabel.Text = "Object\r\nAsteroid";

					MassLabel.Text = currentAsteroid.Mass.ToString() + "t.";
					ElementLabel.Text = "Main Element\r\n" + currentAsteroid.Type.ToScreenString();
				}
                else if (((InterStellarShip)Ship).ItemScanResults != null)
                {
                    var currentItem = ((InterStellarShip)Ship).ItemScanResults;
                    ScannerStatusLabel.Text = "Unknown Object";

                    MassLabel.Text = "";
                    ElementLabel.Text = "";
                }
                else
                {
					ScannerStatusLabel.Text = "No Object\r\nIn Vicinity";
					ScannerStatusLabel.Text = "No Object\r\nIn Vicinity";
					MassLabel.Text = "";
					ElementLabel.Text = "";
				}
			}
		}
	}
}