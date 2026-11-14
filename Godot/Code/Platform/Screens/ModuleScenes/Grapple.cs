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
			GetNode<Label>("Window/Background/Number").Text = (ship.Modules.IndexOf(shipModule) + 1).ToString();
			
			UpdateState();
		}

		private void GrabButton_Pressed()
		{
			if (!CanOperate() || ShipModule.HeldItem != null || Ship is not InterStellarShip ship)
				return;

			var scannedItem = ship.ItemScanResults;
			if (scannedItem is Asteroid asteroid)
			{
				if (asteroid.Mass > 250) return;
			}
			else if (scannedItem is UnknownItem unknown)
			{
				if (unknown.ItemType == UnknownItemTypes.AlienArtifact)
				{
					var star = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Stars[Ship.StarLocation];
					// Another ship may already have collected the object since this scan.
					if (star.ArtifactLocation != Ship.PlanetLocation) return;
					star.ArtifactLocation = StellarBodies.none;
				}
			}
			else return;

			ShipModule.HeldItem = scannedItem;
			ship.ItemScanResults = null;
			UpdateState();
			UpdateParent?.Invoke();
		}

		private bool CanOperate()
		{
			return RogueCrew.CanCommand(Ship) && Ship.Modules.Contains(ShipModule) && Ship.ShipState == Ship_States.UnDocked && Ship.Pilot != null &&
				Ship.Pilot.Count > 0 && Ship.Pilot.Type == StaffType.Marines && Ship.Pilot.GetLevel() > 1;
		}

		private void ReleaseButton_Pressed()
		{
            if (!RogueCrew.CanCommand(Ship) || !Ship.Modules.Contains(ShipModule)) return;
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
            GrabButton.Disabled = !CanOperate();
            ReleaseButton.Disabled = !RogueCrew.CanCommand(Ship) || !Ship.Modules.Contains(ShipModule);
			if (!CanOperate())
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
