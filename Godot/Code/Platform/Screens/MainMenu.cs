using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Base;
using Godot;
using System;
using System.Collections.Generic;

namespace Deuteros.Code.Platform.Screens
{
	public partial class MainMenu : BaseSubScene
	{
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
			UpdateTimeAnimation();
			if (Deuteros.Code.GameCore.HoverText != HoverInfo.Text)
			{
				HoverInfo.Text = Deuteros.Code.GameCore.HoverText;
			}
		}

		//Triggered from gamecore
		public void DayTick(uint previousDay, uint currentDay)
		{
            UpdateTime(previousDay, currentDay);
		}

		private void UpdateTime(uint previousDay, uint currentDay)
		{
			var newDay = (currentDay % 1000).ToString().PadLeft(3, '0');
			var outputYear = (3100 + Math.Floor((decimal)(currentDay / 1000))) + " " + newDay + ".00";

			Time.Text = outputYear;
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
					currentButton.SetButtonType(Enums.Menu_Buttons.Empty);
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
	}
}
