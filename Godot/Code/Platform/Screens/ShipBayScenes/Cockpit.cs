using Deuteros.Code.Objects;
using Deuteros.Code.Objects.Interfaces;
using Deuteros.Code.Platform.Helpers;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Deuteros.Code.Enums;

namespace Deuteros.Code.Platform.Screens.ShipBayScenes
{
	public partial class Cockpit : Control
	{
		public Control SpriteHolder { get; set; }

		public Label PilotNameLabel { get; set; }
	
		public TextureButton AddACC { get; set; }

		public IShip CurrentShip { get; set; }

		public StaffList StaffList { get; set; }
		
		public delegate void AddACCDelegate();
		public event AddACCDelegate ACCAdded;

		public override void _Ready()
		{
			SpriteHolder = GetNode<Control>("SpriteHolder");

			AddACC = GetNode<TextureButton>("Buttons/AddACC");

			PilotNameLabel = GetNode<Label>("Labels/PilotName");

			StaffList = GetNode<StaffList>("StaffList");
		}

		public void UpdateStaff(Staff[] staff)
		{
			StaffList.UpdateStaff(staff);

			UpdateState();
		}

		public void LoadShip(IShip ship)
		{
			CurrentShip = ship;

			StaffList.UpdateShip(CurrentShip != null);

			UpdateState();
		}

		public void UpdateState()
		{
			if (CurrentShip == null)
			{
				PilotNameLabel.Text = "";
				SpriteHolder.Visible = false;
				AddACC.Visible = false;
			}
			else
			{
				PilotNameLabel.Text = CurrentShip.Pilot != null ? CurrentShip.Pilot.GetLevelString()+"\n"+CurrentShip.Pilot.Leader : "";
				SpriteHolder.Visible = true;
				var ptl = CurrentShip is InterStellarShip { DFCC: true };
				AddACC.TextureNormal = SpriteManager.LoadImage(ShipBay.NavSpriteBasePath + (ptl ? "PTL.png" : "ACC.png"));
				AddACC.Visible = !ptl || GameCore.SingletonInstance.GameData.GetItem(ItemTypes.prejudice_torpedo_launcher).Research.Researched;
			}
		}
	}
}
