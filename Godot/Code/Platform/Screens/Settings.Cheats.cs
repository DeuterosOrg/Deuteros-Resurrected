using Deuteros.Code;
using Deuteros.Code.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static Deuteros.Code.Enums;

public partial class Settings
{
	private void BuildTitanStation_Pressed()
	{
		if (!GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[StellarBodies.titan].Station.Built)
		{
			var titan = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[StellarBodies.titan];
			titan.PlanetResources.Derricks = 8;
			titan.BaseBuildParts = 2;
			titan.Station.Built = true;
			titan.Station.BuildParts = 8;
			titan.Station.Factory.AOC = true;
			titan.Station.MtxInstalled = true;
		}
	}

	private void ActivateMTX_Pressed()
	{
		if (!GameCore.Earth.Station.Built)
			ProdInEarthOrbit_Pressed();
		GameCore.Earth.Station.MtxInstalled = true;
		if (!GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Contains(Enums.Game_Unlocks.Mass_Tranceiver))
		{
			GameCore.SingletonInstance.TriggerAlienTechDiscovery(Enums.ItemTypes.m__t__x);
		}
	}

	private void IOSModulesReady_Pressed()
	{
		var gameData = GameCore.SingletonInstance.GameData;
		var earth = (Earth)gameData.ActiveSaveFile.BaseGameData.Planets[Enums.StellarBodies.earth];

		if (!GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Contains(Enums.Game_Unlocks.IOS_Attachments))
		{
			if (!earth.Station.Built)
				ProdInEarthOrbit_Pressed();

			GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Add(Enums.Game_Unlocks.IOS_Attachments);

			GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.a__m__a).Research.Locked = false;
			GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.a__o__c).Research.Locked = false;
			GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.bandaid).Research.Locked = false;
			GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.grapple).Research.Locked = false;
			GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.r_frame).Research.Locked = false;

			gameData.GetItem(Enums.ItemTypes.a__m__a).Research.Researched = true;
			gameData.GetItem(Enums.ItemTypes.a__m__a).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
			gameData.GetItem(Enums.ItemTypes.a__m__a).Locked = false;

			gameData.GetItem(Enums.ItemTypes.a__o__c).Research.Researched = true;
			gameData.GetItem(Enums.ItemTypes.a__o__c).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
			gameData.GetItem(Enums.ItemTypes.a__o__c).Locked = false;

			gameData.GetItem(Enums.ItemTypes.bandaid).Research.Researched = true;
			gameData.GetItem(Enums.ItemTypes.bandaid).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
			gameData.GetItem(Enums.ItemTypes.bandaid).Locked = false;

			gameData.GetItem(Enums.ItemTypes.grapple).Research.Researched = true;
			gameData.GetItem(Enums.ItemTypes.grapple).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
			gameData.GetItem(Enums.ItemTypes.grapple).Locked = false;

			gameData.GetItem(Enums.ItemTypes.r_frame).Research.Researched = true;
			gameData.GetItem(Enums.ItemTypes.r_frame).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
			gameData.GetItem(Enums.ItemTypes.r_frame).Locked = false;
		}
	}

	private void ProdInEarthOrbit_Pressed()
	{
		var gameData = GameCore.SingletonInstance.GameData;
		var earth = (Earth)gameData.ActiveSaveFile.BaseGameData.Planets[Enums.StellarBodies.earth];

		//Make sure we have a station at all
		if (earth.Station.BuildParts < 7)
		{
			EarthStationTo7_Pressed();
		}

		earth.Station.Built = true;
		earth.Station.BuildParts = 8;
		earth.Station.Factory.AOC = true;

		if (!gameData.ActiveSaveFile.Unlocks.Contains(Game_Unlocks.First_Station_Segment))
			gameData.ActiveSaveFile.Unlocks.Add(Game_Unlocks.First_Station_Segment);
		if (!gameData.ActiveSaveFile.Unlocks.Contains(Game_Unlocks.Space_Stations))
			gameData.ActiveSaveFile.Unlocks.Add(Game_Unlocks.Space_Stations);

		GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.i_chassis).Research.Locked = false;
		GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.i_drive).Research.Locked = false;
		GameCore.SingletonInstance.GameData.GetItem(Enums.ItemTypes.a__c__c).Research.Locked = false;
	}

	private void EarthStationTo7_Pressed()
	{
		var gameData = GameCore.SingletonInstance.GameData;
		var earth = (Earth)gameData.ActiveSaveFile.BaseGameData.Planets[Enums.StellarBodies.earth];

		if (earth.Station.BuildParts < 7)
		{
			earth.Station.BuildParts = 7;

			if (!gameData.GetItem(Enums.ItemTypes.a__c__c).Research.Researched)
			{
				gameData.GetItem(Enums.ItemTypes.a__c__c).Research.Researched = true;
				gameData.GetItem(Enums.ItemTypes.a__c__c).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
				gameData.GetItem(Enums.ItemTypes.a__c__c).Locked = false;

				gameData.GetItem(Enums.ItemTypes.i_chassis).Research.Locked = false;
				gameData.GetItem(Enums.ItemTypes.i_drive).Research.Locked = false;
				gameData.GetItem(Enums.ItemTypes.a__c__c).Research.Locked = false;
			}

			earth.PlanetResources.Stores[Enums.ItemTypes.supply_pod] = Math.Max(1, earth.PlanetResources.Stores[Enums.ItemTypes.supply_pod]);
			earth.PlanetResources.Stores[Enums.ItemTypes.a__c__c] = Math.Max(1, earth.PlanetResources.Stores[Enums.ItemTypes.a__c__c]);

			GameCore.SingletonInstance.TriggerStationPiecePlaced(Enums.StellarBodies.earth);
		}

		if (!GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Contains(Enums.Game_Unlocks.Shuttle_Unlock))
		{
			SkipToShuttles_Pressed();
		}
	}

	private void SkipToShuttles_Pressed()
	{
		var gameData = GameCore.SingletonInstance.GameData;

		if (!gameData.GetItem(Enums.ItemTypes.s_chassis).Research.Researched)
		{
			gameData.GetItem(Enums.ItemTypes.s_chassis).Research.Researched = true;
			gameData.GetItem(Enums.ItemTypes.s_chassis).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
			gameData.GetItem(Enums.ItemTypes.s_chassis).Locked = false;
		}

		if (!gameData.GetItem(Enums.ItemTypes.s_drive).Research.Researched)
		{
			gameData.GetItem(Enums.ItemTypes.s_drive).Research.Researched = true;
			gameData.GetItem(Enums.ItemTypes.s_drive).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
			gameData.GetItem(Enums.ItemTypes.s_drive).Locked = false;
		}

		if (!gameData.GetItem(Enums.ItemTypes.meh_fuel).Research.Researched)
		{
			gameData.GetItem(Enums.ItemTypes.meh_fuel).Research.Researched = true;
			gameData.GetItem(Enums.ItemTypes.meh_fuel).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
			gameData.GetItem(Enums.ItemTypes.meh_fuel).Locked = false;
		}

		if (!gameData.GetItem(Enums.ItemTypes.of_frame).Research.Researched)
		{
			gameData.GetItem(Enums.ItemTypes.of_frame).Research.Researched = true;
			gameData.GetItem(Enums.ItemTypes.of_frame).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
			gameData.GetItem(Enums.ItemTypes.of_frame).Locked = false;
		}

		if (!gameData.GetItem(Enums.ItemTypes.tool_pod).Research.Researched)
		{
			gameData.GetItem(Enums.ItemTypes.tool_pod).Research.Researched = true;
			gameData.GetItem(Enums.ItemTypes.tool_pod).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
			gameData.GetItem(Enums.ItemTypes.tool_pod).Locked = false;
		}

		if (!gameData.GetItem(Enums.ItemTypes.supply_pod).Research.Researched)
		{
			gameData.GetItem(Enums.ItemTypes.supply_pod).Research.Researched = true;
			gameData.GetItem(Enums.ItemTypes.supply_pod).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
			gameData.GetItem(Enums.ItemTypes.supply_pod).Locked = false;
		}

		if (!gameData.GetItem(Enums.ItemTypes.cryo_pod).Research.Researched)
		{
			gameData.GetItem(Enums.ItemTypes.cryo_pod).Research.Researched = true;
			gameData.GetItem(Enums.ItemTypes.cryo_pod).Research.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
			gameData.GetItem(Enums.ItemTypes.cryo_pod).Locked = false;
		}

		var earth = (Earth)gameData.ActiveSaveFile.BaseGameData.Planets[Enums.StellarBodies.earth];

		if (earth.ResearchStaff == null || earth.ResearchStaff.Count == 0)
		{
			earth.ResearchStaff = new Staff();
			earth.ResearchStaff.Leader = "Von Braun";
			earth.ResearchStaff.Count = 250;
			earth.ResearchStaff.AddAction(20);

			earth.ResearchStaff.Type = Enums.StaffType.Research;
		}

		if (earth.Factory.Builder == null || earth.Factory.Builder.Count == 0)
		{
			earth.Factory.Builder = new Staff();
			earth.Factory.Builder.Leader = "Bob";
			earth.Factory.Builder.Count = 200;
			earth.Factory.Builder.AddAction(20);
			earth.Factory.Builder.Type = Enums.StaffType.Production;
		}

		if (!earth.PlanetResources.Staff.Any(T => T != null && T.Type == Enums.StaffType.Marines))
		{
			var newMarine = new Staff();
			newMarine.Leader = GameCore.SingletonInstance.GameData.GetNextPersonName();
			newMarine.Count = 41;
			newMarine.AddAction(30);
			newMarine.Type = Enums.StaffType.Marines;

			earth.PlanetResources.AddStaff(newMarine);
		}

		earth.PlanetResources.Derricks = Math.Max(8, earth.PlanetResources.Derricks);
		earth.PlanetResources.Stores[Enums.ItemTypes.s_chassis] = Math.Max(1, earth.PlanetResources.Stores[Enums.ItemTypes.s_chassis]);
		earth.PlanetResources.Stores[Enums.ItemTypes.s_drive] = Math.Max(1, earth.PlanetResources.Stores[Enums.ItemTypes.s_drive]);
		earth.PlanetResources.Stores[Enums.ItemTypes.of_frame] = Math.Max(8, earth.PlanetResources.Stores[Enums.ItemTypes.of_frame]);

		if (!GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Any(T => T.ShipType == Ship_Types.Shuttle && T.PlanetLocation == StellarBodies.earth))
		{
			var newShuttle = new Shuttle();
			newShuttle.StartTravelDay = 0;
			newShuttle.StarLocation = Enums.StellarBodies.the_sun;
			newShuttle.Modules = new List<ShipModule>();
			newShuttle.Modules.Add(new ShipModule());
			newShuttle.ShipState = Ship_States.Docked;
			newShuttle.Fuel = 250;
			newShuttle.Engine = true;
			newShuttle.FuelType = Enums.ItemTypes.meh_fuel;
			newShuttle.OnGround = true;
			newShuttle.Pilot = null;
			newShuttle.PlanetLocation = Enums.StellarBodies.earth;
			newShuttle.ShipType = Enums.Ship_Types.Shuttle;
			newShuttle.LocationView = false;
			newShuttle.Name = "Earth Shuttle";

			GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Add(newShuttle);
			GameCore.SingletonInstance.TriggerShipCreated(newShuttle);
		}
	}

}
