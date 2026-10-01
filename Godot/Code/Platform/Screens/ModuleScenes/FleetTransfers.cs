using Deuteros.Code.Objects;
using Deuteros.Code.Objects.Battle;
using Deuteros.Code.Platform.Base;
using Godot;
using System.Threading;
using System.Threading.Tasks;

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Deuteros.Code.Objects.Interfaces;
using Deuteros.Code;
using System.IO;
using Deuteros.Code.Platform;

public partial class FleetTransfers : BaseSubScene
{
	InterStellarShip _player;

	Label OrbitalDronePoolLabel { get; set; }
	Label FleetDronePoolLabel { get; set; }
	Label PowerLabel { get; set; }

	RepeatingButton TransferButton1 { get; set; }
	RepeatingButton TransferButton2 { get; set; }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		OrbitalDronePoolLabel = GetNode<Label>("OrbitalColorRect/OrbitalDronePoolLabel");
		FleetDronePoolLabel = GetNode<Label>("FleetColorRect/FleetDronePoolLabel");
		PowerLabel = GetNode<Label>("PowerColorRect/PowerLabel");

		TransferButton1 = GetNode<RepeatingButton>("RepeatingButton1");
		TransferButton2 = GetNode<RepeatingButton>("RepeatingButton2");

		TransferButton1.Pressed += Transfer1_Pressed;
		TransferButton2.Pressed += Transfer2_Pressed;

		base._Ready();

	}

	private void updatePoolLabels()
	{
		var p = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[_player.PlanetLocation];

		int drones = 0;

		if (_player is IOS)
		{
			drones = p.Station.Resources.Stores[Enums.ItemTypes.ios_drone];
		}
		else if (_player is SCG)
		{
			drones = p.Station.Resources.Stores[Enums.ItemTypes.star_drone];
		}

		OrbitalDronePoolLabel.Text = " Orbital\n  Drone\n  Pool\n  " + drones;
		FleetDronePoolLabel.Text = "\n Fleet\n Drones\n  " + _player.DroneCount;

		int Level = (_player.Pilot == null ? 0 : _player.Pilot.GetLevel()) + 4;
		PowerLabel.Text = "\n Fleet\n Power\n  " + _player.DroneCount * Level;
	}
	private void Transfer1_Pressed()
	{
		var p = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[_player.PlanetLocation];

		Enums.ItemTypes droneType = Enums.ItemTypes.ios_drone;
		if (_player is IOS)
		{
			droneType = Enums.ItemTypes.ios_drone;
		}
		else if (_player is SCG)
		{
			droneType = Enums.ItemTypes.star_drone;
		}

		if (p.Station.Resources.Stores[droneType] > 0 && _player.DroneCount < 200)
		{
			_player.DroneCount++;
			p.Station.Resources.Stores[droneType]--;
			updatePoolLabels();
		}
	}

	private void Transfer2_Pressed()
	{
		var p = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[_player.PlanetLocation];
		if (_player.DroneCount > 0)
		{
			_player.DroneCount--;
			if (_player is IOS)
			{
				p.Station.Resources.Stores[Enums.ItemTypes.ios_drone]++;
			}
			else if (_player is SCG)
			{
				p.Station.Resources.Stores[Enums.ItemTypes.star_drone]++;
			}
			updatePoolLabels();
		}
	}


	public override void _Process(double delta)
	{
	}

	public void TransferDrones(InterStellarShip player)
	{
		if (player == null)
		{
			player = new IOS();

			player.DroneCount = 192;
			player.PTL = true;
			player.Fuel = 200;
			player.PlanetLocation = GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentPlanet;
		}

		_player = player;

		updatePoolLabels();
	}

}
