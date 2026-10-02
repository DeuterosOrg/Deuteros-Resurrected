using Deuteros.Code.Objects;
using Deuteros.Code.Objects.Interfaces;
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
	public partial class AMA : BaseSubScene
	{
		public const string NavSpriteBasePath = "res://Sprites//SceneSprites//";

		TextureButton MineButton { get; set; }

		TextureRect AsteroidSprite { get; set; }

		Control PodTypeHeaders { get; set; }
		Control Warning { get; set; }
		Control EquipmentActive { get; set; }
		Control Searching { get; set; }
		Control SearchingNone { get; set; }
		Control SearchingFound { get; set; }

		Label FoundSize { get; set; }
		Label FoundMainElement { get; set; }
		Label FoundType { get; set; }

		ShipModule ShipModule { get; set; }
		Ship Ship { get; set; }

		public Action CloseWindow;

		public override void _Ready()
		{
			GetNode<Node2D>("Window").GetNode<Label>("Background/Number").Text = "1";

			MineButton = GetNode<TextureButton>("Buttons/Mine");

			PodTypeHeaders = GetNode<Control>("Labels/PodTypeHeaders");
			Warning = GetNode<Control>("Labels/Warning");
			EquipmentActive = GetNode<Control>("Labels/EquipmentActive");
			Searching = GetNode<Control>("Labels/Searching");
			SearchingNone = GetNode<Control>("Labels/Searching/None");
			SearchingFound = GetNode<Control>("Labels/Searching/Found");

			FoundSize = GetNode<Label>("Labels/Searching/Found/Size");
			FoundMainElement = GetNode<Label>("Labels/Searching/Found/MainElement");
			FoundType = GetNode<Label>("Labels/Searching/Found/Type");

			AsteroidSprite = GetNode<TextureRect>("Sprites/Asteroid");

			MineButton.Pressed += MineButton_Pressed;

			base._Ready();
		}

		public void Load(Ship ship, ShipModule shipModule)
		{
			Ship = ship;
			ShipModule = shipModule;
			
			UpdateState();
		}

        private bool CanMine() => Ship is InterStellarShip ship
            && ship.ShipState == Ship_States.UnDocked && ship.PlanetLocation == StellarBodies.asteroids
            && ship.Fuel > 0 && ship.AsteroidScanResults != null && ship.AsteroidScanResults.Class >= 6;

		private void MineButton_Pressed()
		{
            if (CanMine())
			{
				Ship.Modules.First(T => T.ModuleType == Module_Types.Tool && T.ItemStored == ItemTypes.a__m__a).LastMinedDay = GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay;
				Ship.Dock();

				UpdateState();

				CloseWindow?.Invoke();
			}
		}

		protected override void DayTick(uint previousDay, uint currentDay)
		{
			UpdateState();

			base.DayTick(previousDay, currentDay);
		}

		public void UpdateState()
		{
			PodTypeHeaders.Visible = false;
			Warning.Visible = false;
			EquipmentActive.Visible = false;
			Searching.Visible = false;
			SearchingNone.Visible = false;
			SearchingFound.Visible = false;
			
			AsteroidSprite.Visible = false;
			
			MineButton.Visible = false;
            MineButton.Disabled = !CanMine();

			//In transit, undocked anywhere but Asteroids and docking/launching the screen is the same
			if (Ship.ShipState == Ship_States.InTransit || Ship.ShipState == Ship_States.Launching || Ship.ShipState == Ship_States.Docking || (Ship.ShipState == Ship_States.UnDocked && Ship.PlanetLocation != StellarBodies.asteroids))
			{
				PodTypeHeaders.Visible = true;
				Warning.Visible = true;
			}
			//We're at the asteroids, but there's no pilot
			else if (Ship.ShipState == Ship_States.UnDocked && Ship.Pilot == null && Ship.PlanetLocation == StellarBodies.asteroids)
			{
				PodTypeHeaders.Visible = true;
				Warning.Visible = true;
			}
			//We're at the asteroids scanning, scanning happens automatically as part of shipinterior we just show the results here
			else if (Ship.ShipState == Ship_States.UnDocked && Ship.Pilot != null && Ship.Pilot.GetLevel() > 0 && Ship.PlanetLocation == StellarBodies.asteroids)
			{
				if (((InterStellarShip)Ship).AsteroidScanResults != null && (((InterStellarShip)Ship).AsteroidScanResults is Asteroid))

                {
					var currentAsteroid = (Asteroid)((InterStellarShip)Ship).AsteroidScanResults;

					Searching.Visible = true;
					SearchingFound.Visible = true;

					MineButton.Visible = true;
					AsteroidSprite.Visible = true;
					FoundSize.Visible = true;
					FoundType.Visible = true;

					AsteroidSprite.Texture = SpriteManager.LoadImage(NavSpriteBasePath + "Asteroid_" + currentAsteroid.MassName.ToString() + ".png");

					FoundSize.Text = currentAsteroid.Class.ToString() + "\n" + currentAsteroid.Mass.ToString() + "t.";
					FoundType.Text = currentAsteroid.Type.ToScreenString();
				}
				else
				{
					Searching.Visible = true;
					SearchingNone.Visible = true;
				}
			}
			//We're at the asteroids mining
			else if (Ship.ShipState == Ship_States.Docked && Ship.PlanetLocation == StellarBodies.asteroids)
			{
				PodTypeHeaders.Visible = true;
				EquipmentActive.Visible = true;
			}
		}

		#region statics

		// Original mining amount is independent of asteroid class; cadence remains provisional.
		public static int Mine(Asteroid asteroid, ShipModule shipModule, Random random = null)
		{
			random ??= Random.Shared;
			var minedAmount = 0;

			//There is a 1/20 chance for an instant mine, but only on the first day
			if (shipModule.LastMinedDay + 1 == GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay && random.Next(0, 20) == 10)
			{
				return random.Next(12, 44);
			}
			//Otherwise it is a 5 day mining cycle
			else if (GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay - shipModule.LastMinedDay  == 5)
			{
				shipModule.LastMinedDay = GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay;
				return random.Next(12, 44);
			}

			return minedAmount;
		}

		#endregion
	}
}