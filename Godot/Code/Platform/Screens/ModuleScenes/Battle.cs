using Deuteros.Code.Objects;
using Deuteros.Code.Utility;
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

public partial class Battle : BaseSubScene
{
	TextureButton FleeButton { get; set; }
	TextureButton PTLButton { get; set; }
	Label PlayerShips { get; set; }
	Label PlayerPower { get; set; }

	Label EnemyShips { get; set; }
	Label EnemyPower { get; set; }
	Label PlayerLabels { get; set; }
	Label EnemyLabels { get; set; }
	TextureRect PlayerStation { get; set; }
	TextureRect EnemyStation { get; set; }

	Godot.Timer BattleTimer { get; set; }
	BattleCanvas BattleCanvas { get; set; }
	StarsCanvas StarsCanvas { get; set; }

	BattleLogic _battle;
    private bool exiting;
    private GlobalInput cursor;
    private Rect2 inputBounds;
	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		FleeButton = GetNode<TextureButton>("PlayerFlee");

		PTLButton = GetNode<TextureButton>("PlayerPTL");

		PlayerShips = GetNode<Label>("PlayerShips");

		PlayerPower = GetNode<Label>("PlayerPower");

		EnemyShips = GetNode<Label>("EnemyShips");

		EnemyPower = GetNode<Label>("EnemyPower");

		BattleCanvas = GetNode<BattleCanvas>("BattleCanvas");
		
		StarsCanvas = GetNode<StarsCanvas>("StarsCanvas");

		PlayerStation = GetNode<TextureRect>("PlayerStation");
		EnemyStation = GetNode<TextureRect>("EnemyStation");

		BattleTimer = GetNode<Godot.Timer>("Timer");


		PlayerShips.Text = "123";

		FleeButton.Pressed += FleeButton_Pressed;

		PTLButton.Pressed += PTLButton_Pressed;

		BattleTimer.Timeout += TimerTimeout;

		base._Ready();
        cursor = GameCore.SingletonInstance.GetNode<GlobalInput>("GameContainer/GameViewport/VirtualCursorView");
        inputBounds = GetNode<Control>("Background").GetGlobalRect();
        cursor.LockToRect(inputBounds);

	}

    public override void _ExitTree()
    {
        exiting = true;
        BattleTimer.Stop();
        if (IsInstanceValid(cursor) && cursor.IsLocked && cursor.LockRect == inputBounds) cursor.Unlock();
        base._ExitTree();
    }

	private void FleeButton_Pressed()
	{
		if (_battle.canFlee())
			_battle.PlayerFlee();
	}
	private void PTLButton_Pressed()
	{
		if (_battle.canPTL())
		{
			_battle.LaunchPTL();
			PTLButton.Hide();
		}

	}

	private void TimerTimeout()
	{
        FleeButton.Disabled = !_battle.canFlee();
        PTLButton.Disabled = !_battle.canPTL();
		BattleCanvas.QueueRedraw();
	}

	public override void _Process(double delta)
	{
	}
	private async Task WaitMs(int ms)
	{
        if (exiting) throw new OperationCanceledException();
        await ToSignal(GetTree().CreateTimer(ms / 1000.0, false), SceneTreeTimer.SignalName.Timeout);
        if (exiting) throw new OperationCanceledException();
	}

	public async Task DoBattle(InterStellarShip player, EnemyFleet enemy, Action onCompleted)
	{
		if (player == null)
		{
			player = new IOS();
			player.DroneCount = 192;
			player.PTL = true;
			player.Fuel = 200;
			player.PlanetLocation = Enums.StellarBodies.earth;
		}

		if (enemy == null)
		{
			enemy = new EnemyFleet();
			enemy.DroneCount = 42;
		}

		int fleeCount;

		if (GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[player.PlanetLocation].ActiveMethanoid)
		{
			fleeCount = 0;
			PlayerStation.Hide();
			EnemyStation.Show();
		}
		else
		{
			fleeCount = enemy.DroneCount / 2;
			if (fleeCount <= 2) fleeCount = 0;
			PlayerStation.Show();
			EnemyStation.Hide();
		}

		_battle = new BattleLogic(player, enemy, fleeCount, PlayerPower, PlayerShips, EnemyPower, EnemyShips, BattleCanvas);
		BattleCanvas.BattleLogic = _battle;
        var save = GameCore.SingletonInstance.GameData.ActiveSaveFile;
        save.TimeSkip = save.TimeSkipDay = false;
        var completionReported = false;
        void SettleResult()
        {
            player.DroneCount = _battle.Player1Ships;
            enemy.DroneCount = _battle.Player2Ships;
            if (!_battle.Completed() || completionReported) return;
            completionReported = true;
            if (!ReferenceEquals(save, GameCore.SingletonInstance.GameData.ActiveSaveFile)) return;
            if (_battle.EnemyFled) enemy.Attacking = false;
            if (_battle.PlayerFled) player.EngageEngine();
            onCompleted();
        }
        TreeExiting += SettleResult;

		if (!_battle.hasPTL())
			PTLButton.Hide();
		else
			PTLButton.Show();

		FleeButton.Disabled = false;
		PTLButton.Disabled = false;

		BattleTimer.Start();
		
		while (!_battle.Completed())
		{
			await WaitMs(100);
		}
		BattleTimer.Stop();

        SettleResult();
        await WaitMs(1000);
	}
}
