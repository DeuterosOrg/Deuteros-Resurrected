using Deuteros.Code.Objects;
using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using static Deuteros.Code.Enums;

namespace Deuteros.Code.Platform
{
	public partial class StaffList : Control
	{
		[Export]
		public bool ShowDividers { get; set; }

		[Export]
		public bool ShowBackground { get; set; }

		public Control Dividers { get; set; }
		public TextureRect Background { get; set; }

		public Staff[] CurrentStaff { get; set; }

		public Button Staff1Button { get; set; }
		public Button Staff2Button { get; set; }
		public Button Staff3Button { get; set; }
		public Button Staff4Button { get; set; }
		public bool ShipPresent { get; set; }

		public delegate Staff[] ChangePilotDelegate(Staff staff);
		public event ChangePilotDelegate PilotChanged;

		public delegate Staff[] ChangeProductionDelegate(Staff staff);
		public event ChangeProductionDelegate ProductionChanged;

		public delegate Staff[] ClickStaffDelegate(Staff staff);
		public event ClickStaffDelegate StaffClicked;

		public override void _Ready()
		{
			Dividers = GetNode<Control>("Staff/Dividers");
			Background = GetNode<TextureRect>("Background");

			Dividers.Visible = ShowDividers;
			Background.Visible = ShowBackground;

			CurrentStaff = new Staff[4];

			Staff1Button = GetNode<Button>("Staff/Buttons/01");
			Staff2Button = GetNode<Button>("Staff/Buttons/02");
			Staff3Button = GetNode<Button>("Staff/Buttons/03");
			Staff4Button = GetNode<Button>("Staff/Buttons/04");

			Staff1Button.Pressed += () => ButtonPress(0);
			Staff2Button.Pressed += () => ButtonPress(1);
			Staff3Button.Pressed += () => ButtonPress(2);
			Staff4Button.Pressed += () => ButtonPress(3);
		}

		public void ButtonPress(int staffID)
		{
			var staff = CurrentStaff[staffID];

			if (PilotChanged != null && (staff == null || staff.Type == StaffType.Marines))
			{
				CurrentStaff = PilotChanged?.Invoke(staff);

				UpdateState();
			}
			else if (ProductionChanged != null)
			{
				CurrentStaff = ProductionChanged?.Invoke(staff);

				UpdateState();
			}
			else
			{
				CurrentStaff = StaffClicked?.Invoke(staff);

				UpdateState();
			}
		}

		public void UpdateStaff(Staff[] staff)
		{
			CurrentStaff = staff;

			UpdateState();
		}

		public void UpdateShip(bool shipPresent)
		{
			ShipPresent = shipPresent;
		}

		public void UpdateState()
		{
			for (int i = 0; i < 4; i++)
			{
				var interfaceI = i + 1;

				var staffBackground = GetNode<ColorRect>("Staff/Backgrounds/0" + interfaceI);
				var staffName = GetNode<Label>("Staff/Labels2/Staff" + interfaceI + "Name");
				var staffCount = GetNode<Label>("Staff/Labels2/Staff" + interfaceI + "Count");

				if (CurrentStaff[i] != null)
				{
					staffBackground.Visible = true;
					staffName.Text = CurrentStaff[i].GetLevelString(true)+"\n"+CurrentStaff[i].Leader;
					staffCount.Text = CurrentStaff[i].Count.ToString();

					if (CurrentStaff[i].Type == StaffType.Marines)
						staffBackground.Color = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Red;
					else if (CurrentStaff[i].Type == StaffType.Production)
						staffBackground.Color = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Blue;
				}
				else
				{
					staffBackground.Visible = false;
					staffName.Text = "";
					staffCount.Text = "";
				}
			}
		}
	}
}
