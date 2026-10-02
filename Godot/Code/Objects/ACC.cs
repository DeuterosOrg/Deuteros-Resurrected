using Deuteros.Code.Objects.Interfaces;
using Deuteros.Code.Platform.Base;
using Deuteros.Code.Platform.Screens.ModuleScenes;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Deuteros.Code.Enums;

namespace Deuteros.Code.Objects
{
	public class ACC
	{
		public Enums.StellarBodies Source { get; set; }
		public Enums.StellarBodies Destination { get; set; }

		public List<Enums.ItemTypes> SourceItems { get; set; }
		public List<Enums.ItemTypes> DestinationItems { get; set; }

		public Enums.ItemTypes CurrentSource { get; set; }
		public Enums.ItemTypes CurrentDestination { get; set; }

		public bool Active { get; set; }
		public bool CycleMode { get; set; }
		public bool Refuelling { get; set; }

		public IShip Ship { get; set; }

		private int RefuelLimit
		{
			get
			{
				return Ship.ShipType == Ship_Types.Shuttle ? GameCore.SingletonInstance.GameData.ActiveSaveFile.GameConfig.ShuttleRefuelThreshold : GameCore.SingletonInstance.GameData.ActiveSaveFile.GameConfig.IOSRefuelThreshold;
			}
		}

		private int RefuelMax
		{
			get
			{
				return Ship.ShipType == Ship_Types.Shuttle ? 100 : 250;
			}
		}

		public bool Refuel()
		{
			if (Ship.Fuel >= RefuelLimit) return true;

			var stores = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[Ship.PlanetLocation].Station.Resources.Stores;

			if (Ship.ShipType == Ship_Types.Shuttle && ((Shuttle)Ship).OnGround)
				stores = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[Ship.PlanetLocation].PlanetResources.Stores;

			if (stores[Ship.FuelType] >= (RefuelMax - Ship.Fuel))
			{
				stores[Ship.FuelType] -= (RefuelMax - Ship.Fuel);
				Ship.Fuel = RefuelMax;
				Refuelling = false;
				return true;
			}
			else if (stores[Ship.FuelType] >= (RefuelLimit - Ship.Fuel))
			{
				Ship.Fuel += stores[Ship.FuelType];
				stores[Ship.FuelType] = 0;
				Refuelling = false;
				return true;
			}
			else
			{
				Refuelling = true;
				return false;
			}
		}

		public void LoadSupply()
		{
			if (Ship.ShipState != Ship_States.Docked || Ship.PlanetLocation == StellarBodies.asteroids)
				return;

			var planets = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets;
			var planet = planets[Ship.PlanetLocation];
			var atDestination = Ship.ShipType == Ship_Types.Shuttle
				? !((Shuttle)Ship).OnGround : Ship.PlanetLocation == Destination;
			var stores = Ship.ShipType == Ship_Types.Shuttle && !atDestination
				? planet.PlanetResources.Stores : planet.Station.Resources.Stores;
			var supplyPods = Ship.Modules.Where(m => m.ModuleType == Module_Types.Supply).ToList();

			// Unload every pod before planning new cargo. Full stores leave the remainder aboard.
			foreach (var module in supplyPods)
			{
				if (module.ItemStored != ItemTypes.none && module.ItemCount > 0)
				{
					var accepted = Math.Min(module.ItemCount, Math.Max(0, 50000 - stores[module.ItemStored]));
					stores[module.ItemStored] += accepted;
					module.ItemCount -= accepted;
				}
				if (module.ItemCount == 0) module.ItemStored = ItemTypes.none;
			}

			// Asteroid runs return their ore but do not load outbound supplies.
			if (Ship.DestinationPlanetLocation == StellarBodies.asteroids) return;

			Store otherStores = null;
			if (Ship.ShipType == Ship_Types.Shuttle)
				otherStores = atDestination ? planet.PlanetResources.Stores : planet.Station.Resources.Stores;
			else if (planets.TryGetValue(atDestination ? Source : Destination, out var otherPlanet))
				otherStores = otherPlanet.Station.Resources.Stores;

			var selectedItems = atDestination ? DestinationItems : SourceItems;
			var otherSelectedItems = atDestination ? SourceItems : DestinationItems;
			var current = atDestination ? CurrentDestination : CurrentSource;
			foreach (var module in supplyPods.Where(m => m.ItemCount == 0))
			{
				var first = current;
				do
				{
					if (selectedItems.Contains(current) && stores[current] > 0)
					{
						var amount = Math.Min(250, stores[current]);
						if (otherSelectedItems.Contains(current))
						{
							// Shared selections balance endpoints. Account for cargo already assigned
							// to other pods so a second pod cannot overshoot the same surplus.
							var aboard = supplyPods.Where(m => m.ItemStored == current).Sum(m => m.ItemCount);
							amount = otherStores == null ? 0 : Math.Min(amount,
								Math.Max(0, (stores[current] - otherStores[current] - aboard) / 2));
						}
						if (amount > 0)
						{
							module.ItemStored = current;
							module.ItemCount = amount;
							stores[current] -= amount;
						}
					}
					current++;
					if (current > ItemTypes.hed_fuel) current = ItemTypes.iron;
				} while (current != first && module.ItemCount == 0);
			}
			if (atDestination) CurrentDestination = current;
			else CurrentSource = current;
		}

		public void Update(Ship_States oldState)
		{
			if (!Active && !CycleMode) return;
			if (StopInvalidRoute()) return;

			//We're docked at the asteroids, which means we are running the AMA on an asteroid - Don't bother running checks
			if (Ship.ShipState == Ship_States.Docked && Ship.PlanetLocation == StellarBodies.asteroids)
			{
				//Do nothing
			}
			//We're undocked at the asteroids
			else if (Ship.ShipState == Ship_States.UnDocked && Ship.PlanetLocation == StellarBodies.asteroids)
			{
				var scanResults = ((InterStellarShip)Ship).AsteroidScanResults;

                    //Check if we have an AMA on-board and the relevant equipment and crew
                if (Ship.Modules.Any(T => T.ModuleType == Module_Types.Tool && T.ItemStored == ItemTypes.a__m__a) && Ship.Pilot != null && Ship.Pilot.GetLevel() > 0)
				{
					//Our cargo hold is full and we are undocked, so we need to go home
					if (!Ship.Modules.Any(T => T.ModuleType == Module_Types.Supply && T.ItemCount < 250))
						Ship.EngageEngine();
					//We are not full, so check for a minable asteroid that is of the correct type and is large enough
					//Also check the asteroid has not previously been mined - This means we just took off for it, so we should not land on it again
					else if (scanResults != null && Ship.ACC.DestinationItems.Contains(scanResults.Type) && scanResults.Class >= 6 && !scanResults.HasBeenMined)
					{
						//Reset lastminedday
						Ship.Modules.First(T => T.ModuleType == Module_Types.Tool && T.ItemStored == ItemTypes.a__m__a).LastMinedDay = GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay;
						//Dock the ship with the Asteroid
						Ship.Dock();
					}
				}
			}
			//In this state the ship is waiting for fuel
			else if (Ship.ShipState == oldState && Ship.ShipState == Ship_States.Docked && Refuelling)
			{
				if (Refuel())
				{
					LoadSupply();
					Ship.TakeOff();
				}
			}
			else if (Ship.ShipState == Ship_States.UnDocked && oldState == Ship_States.InTransit)
			{
				Ship.Dock();
			}
			else if (Ship.ShipState == Ship_States.UnDocked && oldState == Ship_States.TakingOff)
			{
				Ship.Dock();
			}
			else if (Ship.ShipState == Ship_States.UnDocked && oldState == Ship_States.Launching)
			{
				if (Ship.ShipType == Ship_Types.Shuttle)
				{
					Ship.Land();
				}
				else
				{
					Ship.EngageEngine();
				}
			}
			//If we're docked, and the old state was either docking or landing, then we're due a resupply
			else if (Ship.ShipState == Ship_States.Docked && (oldState == Ship_States.Docking || oldState == Ship_States.Landing))
			{
				if (Refuel())
				{
					LoadSupply();
					Ship.TakeOff();
				}
			}
		}

		public void Activate()
		{
			if (StopInvalidRoute()) return;
			if (!Active && Ship.Modules.Any(T => T.ModuleType == Module_Types.Supply))
			{
				Active = true;
				
				//Just in case
				if (Ship.ShipType != Ship_Types.Shuttle)
					((InterStellarShip)Ship).ItemScanResults = null;

				if (Ship.ShipState == Ship_States.Docked)
				{
					if (Refuel())
					{
						LoadSupply();
						Ship.TakeOff();
					}
				}
				else if (Ship.ShipState == Ship_States.UnDocked)
				{
					//TODO - BUG - When undocked the active status sets Active to true, but does not start the ship moving?
				}
			}
		}

		private bool StopInvalidRoute()
		{
			// Shuttle ACC runs between ground and orbit at one body, without a planetary route.
			if (Ship.ShipType == Ship_Types.Shuttle ||
				(Ship.CanTravelTo(Source) && Ship.CanTravelTo(Destination) && Ship.CanTravelTo(Ship.DestinationPlanetLocation)))
				return false;
			Active = CycleMode = Refuelling = false;
			return true;
		}
	}
}
