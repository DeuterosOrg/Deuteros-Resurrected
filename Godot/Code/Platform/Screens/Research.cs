using Deuteros.Code.Platform.Base;
using Deuteros.Code.Objects;
using Godot;
using System;
using System.Linq;
using System.Collections.Generic;
using Deuteros.Code.Platform.Helpers;
using System.Threading.Tasks;
using System.IO;

namespace Deuteros.Code.Platform.Screens
{
	public partial class Research : BaseSubScene
	{
		public const string ResearchSpriteBasePath = "res://Sprites//Items//Research//";
		public List<ResearchButton> Buttons { get; set; }
		public ResearchButton SelectedButton { get; set; }
		Label StaffCountLabel { get; set; }
		Label RankLabel { get; set; }
		Label LeaderNameLabel { get; set; }
		Label ItemAnalysisLabel { get; set; }
		Label ItemNameLabel { get; set; }
		Label TechLevelDataLabel { get; set; }
		Label TechLevelLabel { get; set; }
		Label MassLabel { get; set; }
		Label TeamWorkingLabel { get; set; }
		Label ProjectCompletionLabel { get; set; }
		Label MassDataLabel { get; set; }
		Label ProductionAmountListLabel { get; set; }
		Label ProductionMaterialListLabel { get; set; }
		Label ItemNotesLabel { get; set; }
		Label ItemNotesDataLabel { get; set; }
		Node2D InProgressNode { get; set; }
		Node2D ResearchedNode { get; set; }
		TextureRect ResearchImageTextureRect { get; set; }

		public override void _Ready()
		{
			StaffCountLabel = GetNode<Label>("Labels/StaffCountLabel");
			RankLabel = GetNode<Label>("Labels/RankLabel");
			LeaderNameLabel = GetNode<Label>("Labels/LeaderNameLabel");
			ItemAnalysisLabel = GetNode<Label>("Labels/ItemAnalysisLabel");
			ItemNameLabel = GetNode<Label>("Labels/ItemNameLabel");
			TechLevelDataLabel = GetNode<Label>("Labels/TechLevelDataLabel");
			TechLevelLabel = GetNode<Label>("Labels/TechLevelLabel");

			MassLabel = GetNode<Label>("Labels/Researched/MassLabel");
			TeamWorkingLabel = GetNode<Label>("Labels/InProgress/TeamWorkingLabel");
			ProjectCompletionLabel = GetNode<Label>("Labels/InProgress/ProjectCompletionLabel");
			MassDataLabel = GetNode<Label>("Labels/Researched/MassDataLabel");
			ProductionAmountListLabel = GetNode<Label>("Labels/Researched/ProductionAmountListLabel");
			ProductionMaterialListLabel = GetNode<Label>("Labels/Researched/ProductionMaterialListLabel");
			ItemNotesLabel = GetNode<Label>("Labels/Researched/ItemNotesLabel");
			ItemNotesDataLabel = GetNode<Label>("Labels/Researched/ItemNotesDataLabel");

			InProgressNode = GetNode<Node2D>("Labels/InProgress");
			ResearchedNode = GetNode<Node2D>("Labels/Researched");

			ResearchImageTextureRect = GetNode<TextureRect>("Sprites/ResearchImage");
			ResearchImageTextureRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize;
			ResearchImageTextureRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;

			Buttons = Utility.Buttons.CreateButtons<ResearchButton, ResearchItem>(GetNode<GridContainer>("ResearchButtonGrid"),
				GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null).Select(T => T.Research).OrderBy(T => T.Index).ToDictionary(obj => obj.Index),
				this,
				nameof(ResearchButton_Clicked),
				"/Code/Platform/ResearchButton.cs",
				"ResearchButton");

			if (GameCore.Earth.CurrentResearchItem != null)
			{
				SelectedButton = Buttons.Single(T => T.ObjectData != null && T.ObjectData.Index == GameCore.Earth.CurrentResearchItem.Index);
				DrawData(true);
			}
			else
			{
				DrawData(false);
			}

			base._Ready();
		}

		private void ResearchButton_Clicked(int index)
		{
			var clickedButton = Buttons.Single(T => T.ObjectData != null && T.ObjectData.Index == index);

			if (!clickedButton.ObjectData.Locked)
			{
				if (SelectedButton != null) SelectedButton.Selected = false;
				SelectedButton = clickedButton;
				SelectedButton.Selected = true;

				var earth = GameCore.GetPlanet<Earth>(Enums.StellarBodies.earth);
				earth.CurrentResearchItem = SelectedButton.ObjectData;

				DrawData(false);
			}
		}

		//Triggered from gamecore
		protected override async void DayTick(uint previousDay, uint currentDay)
		{
			UpdateResearchButton(true);
		}

		public void UpdateResearchButton(bool dayPassed)
		{
			if (SelectedButton != null && SelectedButton.ObjectData != null)
			{
				SelectedButton.Redraw(dayPassed);

				DrawData(dayPassed);
			}
		}

		// Called every update.
		public override void _Draw()
		{
			//DrawData(false);
		}

		public void DrawData(bool dayPassed)
		{
			ResearchImageTextureRect.Texture = null;
			ItemAnalysisLabel.Text = "";
			ItemNameLabel.Text = "";
			TechLevelDataLabel.Text = "";
			TechLevelLabel.Text = "";
			MassLabel.Text = "";
			TeamWorkingLabel.Text = "";
			ProjectCompletionLabel.Text = "";
			MassDataLabel.Text = "";
			ProductionAmountListLabel.Text = "";
			ProductionMaterialListLabel.Text = "";
			ItemNotesLabel.Text = "";
			ItemNotesDataLabel.Text = "";

			InProgressNode.Visible = false;
			ResearchedNode.Visible = false;

			if (GameCore.Earth.ResearchStaff == null)
			{
				StaffCountLabel.Text = "";
				RankLabel.Text = "";
				LeaderNameLabel.Text = "None";
			}
			else
			{
				StaffCountLabel.Text = GameCore.Earth.ResearchStaff.Count.ToString();
				RankLabel.Text = ((Enums.StaffLevel_Researcher)GameCore.Earth.ResearchStaff.GetLevel()).ToString();
				LeaderNameLabel.Text = GameCore.Earth.ResearchStaff.Leader;
			}

			if (SelectedButton != null && SelectedButton.ObjectData != null)
			{
				var researchItem = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Single(T => T.Research != null && T.Research.Index == SelectedButton.ObjectData.Index);

				ItemAnalysisLabel.Text = "Item Analysis";
				ItemNameLabel.Text = researchItem.FullName;
				TechLevelLabel.Text = "Tech Level";
				TechLevelDataLabel.Text = researchItem.Research.TechLevel.ToString();

				if (researchItem.Research.Researched)
				{
					ResearchedNode.Visible = true;

					ResearchImageTextureRect = SpriteManager.LoadImageToTextureRect(ResearchSpriteBasePath + researchItem.Research.ItemType.ToString() + ".png", ResearchImageTextureRect);
					if (researchItem.BuildRequirements == null)
					{
						ItemNotesLabel.Text = "Research complete";
						return;
					}
					MassLabel.Text = "Mass " + "".PadRight(researchItem.Mass.ToString().Length, ' ') + "t.";
					MassDataLabel.Text = researchItem.Mass.ToString();
					TeamWorkingLabel.Text = "";
					ProjectCompletionLabel.Text = "";
					ProductionAmountListLabel.Text = string.Join('\n', researchItem.BuildRequirements.Select(T => T.ItemCount));
					ProductionMaterialListLabel.Text = string.Join('\n', researchItem.BuildRequirements.Select(T => T.ItemType.ToString()));
					ItemNotesLabel.Text = "This item may\nbe produced";
					ItemNotesDataLabel.Text = researchItem.OrbitOnly ? "In Orbit Only" : "by any factory";
				}
				
				else if (GameCore.Earth.ResearchStaff == null || GameCore.Earth.ResearchStaff.GetLevel()<researchItem.Research.TechLevel)
				{
					InProgressNode.Visible = true;

					ResearchImageTextureRect.Texture = null;
					MassLabel.Text = "";
					MassDataLabel.Text = "";
					TeamWorkingLabel.Text = "Team Leader Is\nNot Qualified\nFor This Item";
					TeamWorkingLabel.AddThemeColorOverride("font_color", GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Red);

					ProjectCompletionLabel.Text = "";
					ProductionAmountListLabel.Text = "";
					ProductionMaterialListLabel.Text = "";
					ItemNotesLabel.Text = "";
					ItemNotesDataLabel.Text = "";

				}
				else if (GameCore.Earth.ResearchStaff != null && researchItem.Research.ResearchPercentageComplete > 0 && dayPassed)
				{
					InProgressNode.Visible = true;

					ResearchImageTextureRect.Texture = null;
					MassLabel.Text = "";
					MassDataLabel.Text = "";
					TeamWorkingLabel.Text = "Team Working";
					TeamWorkingLabel.RemoveThemeColorOverride("font_color");
					ProjectCompletionLabel.Text = "Project is\n" + researchItem.Research.ResearchPercentageComplete.ToString().PadLeft(2, ' ') + "% complete";
					ProductionAmountListLabel.Text = "";
					ProductionMaterialListLabel.Text = "";
					ItemNotesLabel.Text = "";
					ItemNotesDataLabel.Text = "";
				}
			}
		}

		public static void UpdateResearch(uint previousDay, uint currentDay)
		{
			var currentItem = GameCore.Earth.CurrentResearchItem;

			if (currentItem != null)
			{
				//If the item is researched, return 100
				if (currentItem.Researched)
					return;

				var earth = GameCore.GetPlanet<Earth>(Enums.StellarBodies.earth);

				//If the researchstaff is null, then the game has just started, do nothing
				if (earth.ResearchStaff == null)
					return;

				var level = earth.ResearchStaff.GetLevel();

				if (level >= currentItem.TechLevel && currentItem.ResearchPercentageComplete < currentItem.ResearchLimit)
				{
					var teamSize = earth.ResearchStaff.Count;

					int v = (teamSize << level) * currentItem.ResearchMultiplier / 801;

					if ((currentItem.ResearchValue + v) > 255)
					{
						currentItem.ResearchValue = (currentItem.ResearchValue + v) & 0xFF; // Overflow wraparound
						if (currentItem.ResearchPercentageComplete < 100)
						{
							currentItem.ResearchPercentageComplete += 11;

							if (currentItem.ResearchPercentageComplete > currentItem.ResearchLimit)
								currentItem.ResearchPercentageComplete = currentItem.ResearchLimit;

                            if (currentItem.ResearchPercentageComplete > 100)
								currentItem.ResearchPercentageComplete = 100;
						}
					}
					else
					{
						currentItem.ResearchValue += v;
					}

					if (currentItem.ResearchPercentageComplete == 100)
					{
						currentItem.Researched = true;
						currentItem.ResearchOrder = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ItemList.Where(T => T.Research != null && T.Research.Researched).Count();
						earth.ResearchStaff.AddAction();

						GameCore.SingletonInstance.GameData.GetItem(currentItem.ItemType).Locked = false;

						GameCore.SingletonInstance.TriggerResearchFinished(GameCore.SingletonInstance.GameData.GetItem(currentItem.ItemType).Research);
					}
				}
			}
		}
	}
}
