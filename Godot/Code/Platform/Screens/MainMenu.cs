using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Base;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Deuteros.Code.Utility;

namespace Deuteros.Code.Platform.Screens
{
	public partial class MainMenu : BaseSubScene
	{
		private AudioStreamPlayer menuClickSound;
		private AudioStreamPlayer sdmAlarm;
		private SpaceStation alarmStation;
		private bool menuSoundBound;

		public override void _EnterTree()
		{
			base._EnterTree();
			if (menuSoundBound) return;
			menuClickSound = GetNode<AudioStreamPlayer>("MenuClickSound");
			sdmAlarm = GetNode<AudioStreamPlayer>("SdmAlarm");
			// Bind before child _Ready navigation callbacks can change the clicked slot.
			foreach (var path in new[] { "Top", "MainButtons" })
				foreach (var button in GetNode(path).GetChildren().OfType<BaseButton>())
					button.Pressed += () => PlayMenuClick(button);
			var hold = GetNode<BaseButton>("Time/TimeBox/TimerHoldButton");
			hold.ButtonDown += () => PlayMenuClick(hold);
			menuSoundBound = true;
		}

		private void PlayMenuClick(BaseButton button)
		{
			if (button.Disabled || !button.IsVisibleInTree() || !button.CanProcess() || GlobalInput.UiLocked)
				return;
			if (button is SceneChangeButton sceneButton && sceneButton.TargetScene == Enums.Scenes.None
				&& (sceneButton.ClickActions == null || sceneButton.ClickActions.Count == 0))
				return;
			menuClickSound.Play();
		}

		public override void _ExitTree()
		{
			menuClickSound.Stop();
			sdmAlarm.Stop();
			AudioServer.SetBusMute(AudioServer.GetBusIndex("Game"), false);
			alarmStation = null;
			base._ExitTree();
		}

		public Label HoverInfo { get; set; }
		public Label Location { get; set; }
		public Label Star { get; set; }
		public Label Time { get; set; }
		public List<Objects.MenuButton> MenuButtons { get; set; }

		public SceneChangeButton EarthButton { get; set; }
		public SceneChangeButton MasterControlButton { get; set; }
		public SceneChangeButton NewsButton { get; set; }
		public SceneChangeButton SaveButton { get; set; }
		public TimerSwitchButton TimeButton { get; set; }
		public SceneChangeButton StockButton { get; set; }
		public SceneChangeButton DepositAnalysisButton { get; set; }
		
		public AnimatedSprite2D EarthAnimation { get; set; }
        public AnimatedSprite2D MasterControlAnimation { get; set; }
		public AnimatedSprite2D NewsAnimation { get; set; }
		public AnimatedSprite2D SaveAnimation { get; set; }
		public AnimatedSprite2D TimeAnimation { get; set; }
		public AnimatedSprite2D StockAnimation { get; set; }
		public AnimatedSprite2D DepositAnalysisAnimation { get; set; }

		// Called when the node enters the scene tree for the first time.
		public override void _Ready()
		{
			MenuButtons = new List<Objects.MenuButton>();
			HoverInfo = GetNode<Label>("HoverInfo");
			Location = GetNode<Label>("Location/LocationBox/Location");
			Star = GetNode<Label>("Boxes/StarName/Star");
			Time = GetNode<Label>("Time/TimeBox/Time");

			EarthButton = GetNode<SceneChangeButton>("Top/Earth");
			MasterControlButton = GetNode<SceneChangeButton>("Top/MasterControl");
			NewsButton = GetNode<SceneChangeButton>("Top/News");
			SaveButton = GetNode<SceneChangeButton>("Top/Save");
			TimeButton = GetNode<TimerSwitchButton>("Top/Time");
			StockButton = GetNode<SceneChangeButton>("Top/Stock");
			DepositAnalysisButton = GetNode<SceneChangeButton>("Top/DepositAnalysis");

			EarthAnimation = GetNode<AnimatedSprite2D>("Top/Earth/EarthAnimation");
            MasterControlAnimation = GetNode<AnimatedSprite2D>("Top/MasterControl/MasterControlAnimation");
			NewsAnimation = GetNode<AnimatedSprite2D>("Top/News/NewsAnimation");
			SaveAnimation = GetNode<AnimatedSprite2D>("Top/Save/SaveAnimation");
			TimeAnimation = GetNode<AnimatedSprite2D>("Top/Time/TimeAnimation");
			StockAnimation = GetNode<AnimatedSprite2D>("Top/Stock/StockAnimation");
			DepositAnalysisAnimation = GetNode<AnimatedSprite2D>("Top/DepositAnalysis/DepositAnalysisAnimation");

			EarthButton.Pressed += UpdateAnimations;
			MasterControlButton.Pressed += UpdateAnimations;
			NewsButton.Pressed += UpdateAnimations;
			SaveButton.Pressed += UpdateAnimations;
			TimeButton.Pressed += UpdateAnimations;
			StockButton.Pressed += UpdateAnimations;
			DepositAnalysisButton.Pressed += UpdateAnimations;

			GameCore.SingletonInstance.UnlockAdded += SingletonInstance_UnlockAdded;

			UpdateTime(Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay, Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentDay);

			Deuteros.Code.GameCore.SingletonInstance.DayPassed += DayTick;

			if (Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentPlanet.ToString().ToUpperInvariant() != Location.Text)
			{
				Location.Text = Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentPlanet.ToString().ToUpperInvariant();
			}

			UpdateAnimations();

			base._Ready();
		}

		private void SingletonInstance_UnlockAdded(Enums.Game_Unlocks addedUnlock)
		{
			SetupMenus();
		}

		public override void _Process(double delta)
		{
			UpdateSdmAlarm();
			UpdateTimeAnimation();
			if (Deuteros.Code.GameCore.HoverText != HoverInfo.Text)
			{
				HoverInfo.Text = Deuteros.Code.GameCore.HoverText;
			}
		}

		private void UpdateSdmAlarm()
		{
			var core = GameCore.SingletonInstance;
			var save = core.GameData.ActiveSaveFile;
			var planet = core.GetCurrentPlanet();
			var local = core.currentScene is not (Enums.Scenes.Overview or Enums.Scenes.News or Enums.Scenes.SaveScreen
				or Enums.Scenes.Bulletins or Enums.Scenes.IntroScreen or Enums.Scenes.None
				or Enums.Scenes.Earth_Ground or Enums.Scenes.Earth_Research or Enums.Scenes.Earth_Training);
			if (planet.PlanetId == Enums.StellarBodies.earth && GameCore.Earth.GroundSelected) local = false;
			if (core.currentScene == Enums.Scenes.ShipInterior)
			{
				// ShipSelected is cleared after entry; the live interior owns the viewed ship.
				var ship = GetParent().GetChildren().OfType<ShipInterior>().LastOrDefault(s => !s.IsQueuedForDeletion())?.Ship;
				local &= ship != null && save.Ships.Contains(ship) && ship.ShipState != Enums.Ship_States.InTransit;
			}
			if (!local || !planet.Station.Built || planet.Station.SdmCountdown == 0)
			{
				if (alarmStation != null) AudioServer.SetBusMute(AudioServer.GetBusIndex("Game"), false);
				sdmAlarm.Stop();
				alarmStation = null;
				return;
			}
			if (!ReferenceEquals(alarmStation, planet.Station) || !sdmAlarm.Playing)
			{
				// The original alarm owns all four channels until leaving or defusing.
				AudioServer.SetBusMute(AudioServer.GetBusIndex("Game"), true);
				alarmStation = planet.Station;
				sdmAlarm.Play();
			}
		}

		//Triggered from gamecore
		public void DayTick(uint previousDay, uint currentDay)
		{
            UpdateTime(previousDay, currentDay);
		}

		private void UpdateTime(uint previousDay, uint currentDay)
		{
			Time.Text = Deuteros.Code.Objects.GameClock.FormatDate(GameCore.SingletonInstance.GameData.ActiveSaveFile.Clock.DateCentidays);
		}

		public void UpdateAnimations()
		{
			MasterControlAnimation.Play("static");
			EarthAnimation.Play("static");
			NewsAnimation.Play("static");
			SaveAnimation.Play("static");
			UpdateTimeAnimation();
			StockAnimation.Play("static");
			DepositAnalysisAnimation.Play("static");

			if (GameCore.SingletonInstance.currentScene == Enums.Scenes.Earth_Training ||
				GameCore.SingletonInstance.currentScene == Enums.Scenes.Earth_Ground ||
				GameCore.SingletonInstance.currentScene == Enums.Scenes.Earth_Research ||
				(GameCore.SingletonInstance.GetCurrentPlanet().PlanetId == Enums.StellarBodies.earth && GameCore.SingletonInstance.SceneVariables.Contains(Enums.SceneVariables.Ground))
				)
				EarthAnimation.Play("animated");
			else if (GameCore.SingletonInstance.currentScene == Enums.Scenes.Overview)
				MasterControlAnimation.Play("animated");
			else if (GameCore.SingletonInstance.currentScene == Enums.Scenes.News)
				NewsAnimation.Play("animated");
			else if (GameCore.SingletonInstance.currentScene == Enums.Scenes.SaveScreen)
				SaveAnimation.Play("animated");
			else if (GameCore.SingletonInstance.currentScene == Enums.Scenes.Store)
				StockAnimation.Play("animated");
			else if (GameCore.SingletonInstance.currentScene == Enums.Scenes.ResourceMap)
				DepositAnalysisAnimation.Play("animated");
		}

		private void UpdateTimeAnimation()
		{
			var save = GameCore.SingletonInstance.GameData.ActiveSaveFile;
			var animation = save.TimeSkip || save.TimeSkipDay ? "animated" : "static";
			// Clock changes can come from holds or simulation events, independently of menu clicks.
			// Keep the current frame when unrelated menu state refreshes.
			if (TimeAnimation.Animation.ToString() != animation)
				TimeAnimation.Play(animation);
		}

		public void SetupMenus()
		{
			var column = "A";
			var row = 1;

			for (var i = 0; i < 12; i++)
			{
				var menuButton = MenuButtons[i];

				var currentButton = GetNode<MenuButton>("MainButtons/" + column + row.ToString() + "/");

				if (menuButton == null || !menuButton.Enabled())
				{
					currentButton.SetButtonType(menuButton?.DisabledButtonType() ?? Enums.Menu_Buttons.Empty);
					currentButton.HoverText = "";
					currentButton.SceneVariables = new Godot.Collections.Array<Enums.SceneVariables>();
					currentButton.TargetScene = Enums.Scenes.None;
					currentButton.ClickActions = null;
					currentButton.Disabled = true;
				}
				else
				{
					currentButton.SetButtonType(menuButton.ButtonType);
					currentButton.HoverText = menuButton.HoverText;
					currentButton.SceneVariables = menuButton.SceneVariables;
					currentButton.TargetScene = menuButton.SceneToLoad;
					currentButton.ClickActions = menuButton.ClickActions;
					currentButton.Disabled = false;
				}

				row++;

				if (row == 7)
				{
					row = 1;
					column = "B";
				}
			}
		}

		public void SetMethanoidIndicator(bool visible)
		{
			GetNode<TextureRect>("MethanoidIndicator").Visible = visible;
		}
	}
}
