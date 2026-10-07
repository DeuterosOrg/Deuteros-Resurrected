using Deuteros.Code;
using Deuteros.Code.Utility;
using Deuteros.Code.Objects;
using Deuteros.Code.Objects.GameData;
using Deuteros.Code.Objects.Interfaces;
using Deuteros.Code.Platform;
using Deuteros.Code.Platform.Base;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Platform.Screens;
using Godot;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Security;
using System.Threading.Tasks;
using static Deuteros.Code.Enums;
using static System.Collections.Specialized.BitVector32;

public partial class Overview : BaseSubScene
{
	public const string SpriteBasePath = "res://Sprites//Buttons//Overview//";

	List<TextureButton> StationButtons = new List<TextureButton>();
	List<TextureButton> IOSButtons = new List<TextureButton>();
	List<TextureButton> SCGButtons = new List<TextureButton>();
	Dictionary<TextureButton, string> HoverTexts = new Dictionary<TextureButton, string>();
	TextureButton HoveredButton;
    private int page;
    private HBoxContainer pages;
    private Button previousPage, nextPage;
    private Label pageNumber;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		for (int i = 0; i < 2; i++)
		{
			for (int j = 0; j < 8; j++)
			{
				var curIndex = i * 8 + j;
				StationButtons.Add(GetNode<TextureButton>("Stations/Col" + i.ToString() + "/Station0" + j.ToString()));
				StationButtons.Last().Pressed += () => Station_Pressed(curIndex);

				IOSButtons.Add(GetNode<TextureButton>("IOS/Col" + i.ToString() + "/IOS0" + j.ToString()));
				IOSButtons.Last().Pressed += () => IOS_Pressed(curIndex);

				SCGButtons.Add(GetNode<TextureButton>("SCG/Col" + i.ToString() + "/SCG0" + j.ToString()));
				SCGButtons.Last().Pressed += () => SCG_Pressed(curIndex);

			}
		}

		foreach (var button in StationButtons.Concat(IOSButtons).Concat(SCGButtons))
		{
			button.MouseEntered += () =>
			{
				HoveredButton = button;
				GameCore.HoverText = HoverTexts.GetValueOrDefault(button, "");
			};
			button.MouseExited += () =>
			{
				if (HoveredButton == button)
				{
					HoveredButton = null;
					GameCore.HoverText = "";
				}
			};
		}
		foreach (var button in IOSButtons.Concat(SCGButtons))
		{
			button.AddChild(new Godot.Label
			{
				Name = "DroneCount",
				OffsetLeft = 8,
				OffsetTop = 3,
				OffsetRight = 30,
				OffsetBottom = 14,
				HorizontalAlignment = HorizontalAlignment.Right,
				MouseFilter = Control.MouseFilterEnum.Ignore
			});
		}

        pages = new HBoxContainer { Name = "Pages", Position = new Vector2(64, 18),
            Size = new Vector2(168, 16), Alignment = BoxContainer.AlignmentMode.Center };
        previousPage = new Button { Name = "Previous", Text = "Prev", TooltipText = "Previous overview page" };
        pageNumber = new Label { Name = "Page", MouseFilter = Control.MouseFilterEnum.Ignore };
        nextPage = new Button { Name = "Next", Text = "Next", TooltipText = "Next overview page" };
        foreach (var control in new Control[] { previousPage, pageNumber, nextPage })
        {
            control.AddThemeFontSizeOverride("font_size", 8);
            pages.AddChild(control);
        }
        AddChild(pages);
        previousPage.Pressed += () => ChangePage(-1);
        nextPage.Pressed += () => ChangePage(1);

		UpdateState();

		base._Ready();
	}

	public override void _ExitTree()
	{
		if (HoveredButton != null)
			GameCore.HoverText = "";
		base._ExitTree();
	}

    private void ChangePage(int delta)
    {
        if (!IsInsideTree() || IsQueuedForDeletion() || GetTree().Paused || GlobalInput.UiLocked
            || OverlayManager.Instance.IsOpen || GameCore.SingletonInstance.GetNode<InputBlocker>("GameContainer/GameViewport/InputBlocker").Blocked) return;
        page += delta;
        UpdateState();
    }

	private void Station_Pressed(int buttonPressed)
	{
		//var planet = Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[(StellarBodies)Enum.Parse(typeof(StellarBodies),StationButtons[buttonPressed].GetMeta("planetid").ToString())];
		var planet = Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[(StellarBodies)StationButtons[buttonPressed].GetMeta("planetid").AsInt32()];

		if (!planet.Station.Built) return;

		GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentPlanet = planet.PlanetId;

		var sceneVariables = new List<SceneVariables>();
		sceneVariables.Add(Enums.SceneVariables.Orbit);

		//Underscores in scene names represent a folder
		Deuteros.Code.GameCore.SingletonInstance.ChangeScene(Enums.Scenes.Station, sceneVariables);
	}

	private void IOS_Pressed(int buttonPressed)
	{
		//show ios

		GameCore.SingletonInstance.ShipSelected = Guid.Parse(IOSButtons[buttonPressed].GetMeta("shipid").ToString());
		GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentPlanet = GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Single<IShip>(s => s.ShipID == GameCore.SingletonInstance.ShipSelected).PlanetLocation;

		var sceneVariables = new List<SceneVariables>();
		sceneVariables.Add(Enums.SceneVariables.Orbit);

		//Underscores in scene names represent a folder
		Deuteros.Code.GameCore.SingletonInstance.ChangeScene(Enums.Scenes.ShipInterior, sceneVariables);
	}

	private void SCG_Pressed(int buttonPressed)
	{
		//show scg
		GameCore.SingletonInstance.ShipSelected = Guid.Parse(SCGButtons[buttonPressed].GetMeta("shipid").ToString());
		GameCore.SingletonInstance.GameData.ActiveSaveFile.CurrentPlanet = GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Single<IShip>(s => s.ShipID == GameCore.SingletonInstance.ShipSelected).PlanetLocation;


		//Underscores in scene names represent a folder
		Deuteros.Code.GameCore.SingletonInstance.ChangeScene(Enums.Scenes.ShipInterior, new List<SceneVariables>());
	}


	//Triggered from gamecore
	protected override void DayTick(uint previousDay, uint currentDay)
	{
		UpdateState();
	}
	
	public void UpdateState()
	{
		var stationList = Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets.Values.Where(p => !p.ActiveMethanoid && p.Station.BuildParts > 0)
            .Select(p => p.Station).OrderBy(s => s.StationOrdinal).ThenBy(s => s.PlanetId).ToList();
		var iosList = Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Where(T => T.ShipType == Ship_Types.IOS).ToList();
		var scgList = Deuteros.Code.GameCore.SingletonInstance.GameData.ActiveSaveFile.Ships.Where(T => T.ShipType == Ship_Types.SCG).ToList();

        var pageCount = Math.Max(1, (Math.Max(stationList.Count, Math.Max(iosList.Count, scgList.Count)) + 15) / 16);
        page = Math.Clamp(page, 0, pageCount - 1);
        pages.Visible = pageCount > 1;
        foreach (var name in new[] { "Stations", "IOS", "SCG" })
        {
            var group = GetNode<Control>(name);
            group.Position = new Vector2(group.Position.X, pages.Visible ? 38 : 25);
            group.Size = new Vector2(group.Size.X, pages.Visible ? 140 : 152);
            foreach (var column in group.GetChildren().Cast<VBoxContainer>())
            {
                column.Size = new Vector2(column.Size.X, pages.Visible ? 140 : 148);
                column.AddThemeConstantOverride("separation", pages.Visible ? 1 : name == "Stations" ? 5 : 4);
            }
        }
        pageNumber.Text = $"Page {page + 1}/{pageCount}";
        previousPage.Disabled = page == 0;
        nextPage.Disabled = page == pageCount - 1;

		StationButtons.ForEach(T => T.Visible = false);
		IOSButtons.ForEach(T => T.Visible = false);
		SCGButtons.ForEach(T => T.Visible = false);
		HoverTexts.Clear();

		var stationCount = 0;
		var iosCount = 0;
		var scgCount = 0;

		foreach (var station in stationList.Skip(page * 16).Take(16))
		{
			var curStationButton = StationButtons[((stationCount & 1) * 8)+(stationCount >> 1)];

			if (!GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[station.PlanetId].ActiveMethanoid)
			{

				curStationButton.Visible = true;
				curStationButton.SetMeta("planetid",Variant.From<int>((Int32)station.PlanetId));
				HoverTexts[curStationButton] = station.PlanetId.ToScreenString(" ");

				if (!station.Built)
					curStationButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Station_UnderConstruction.png");
				else if (GameCore.SingletonInstance.GameData.PlanetUnderAttack(station.PlanetId))
					curStationButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Station_UnderAttack.png");
				else
					curStationButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Station_" + Math.Floor((decimal)(station.Factory.ProdCycle / 2)) + ".png");

				stationCount++;
			}
		}

		foreach (InterStellarShip ios in iosList.Skip(page * 16).Take(16))
		{
			var curIOSButton = IOSButtons[((iosCount & 1) * 8) + (iosCount >> 1)];

			curIOSButton.Visible = true;
			curIOSButton.SetMeta("shipid", ios.ShipID.ToString());
			UpdateShipDetails(curIOSButton, ios);

			if (GameCore.SingletonInstance.GameData.ActiveSaveFile.AtWar && (GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[ios.PlanetLocation].ActiveMethanoid ||
                GameCore.SingletonInstance.GameData.PlanetUnderAttack(ios.PlanetLocation)))
				curIOSButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_UnderAttack.png");
			else if (ios.ShipState == Ship_States.Docking)
				curIOSButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_Docking.png");
			else if(ios.ShipState == Ship_States.Launching)
				curIOSButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_Launching.png");
			else if(ios.ShipState == Ship_States.InTransit && ios.StarLocation != ios.DestinationStarLocation)
				curIOSButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_InTransit_InterStellar.png");
			else if (ios.ShipState == Ship_States.InTransit && ios.StarLocation == ios.DestinationStarLocation)
				curIOSButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_InTransit.png");
			else if (ios.ShipState == Ship_States.Docked)
				curIOSButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_Docked.png");
			else if (ios.ShipState == Ship_States.UnDocked && ios.Scanning)
				curIOSButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_Scanning.png");
			else if (ios.ShipState == Ship_States.UnDocked && ios.Mining)
				curIOSButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_Mining.png");
			else if (ios.ShipState == Ship_States.UnDocked && ios.DFCC)
				curIOSButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_UnDocked_DFCC.png");
			else if (ios.ShipState == Ship_States.UnDocked && !ios.DFCC)
				curIOSButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_UnDocked.png");

			iosCount++;
		}

		foreach (InterStellarShip scg in scgList.Skip(page * 16).Take(16))
		{
			var curSCGButton = SCGButtons[((scgCount & 1) * 8) + (scgCount >> 1)];

			curSCGButton.Visible = true;
			curSCGButton.SetMeta("shipid", scg.ShipID.ToString());
			UpdateShipDetails(curSCGButton, scg);

			if (GameCore.SingletonInstance.GameData.ActiveSaveFile.AtWar && (GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[scg.PlanetLocation].ActiveMethanoid ||
                GameCore.SingletonInstance.GameData.PlanetUnderAttack(scg.PlanetLocation)))
				curSCGButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_UnderAttack.png");
			else if (scg.ShipState == Ship_States.Docking)
				curSCGButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_Docking.png");
			else if (scg.ShipState == Ship_States.Launching)
				curSCGButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_Launching.png");
			else if (scg.ShipState == Ship_States.InTransit && scg.StarLocation != scg.DestinationStarLocation)
				curSCGButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_InTransit_InterStellar.png");
			else if (scg.ShipState == Ship_States.InTransit && scg.StarLocation == scg.DestinationStarLocation)
				curSCGButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_InTransit.png");
			else if (scg.ShipState == Ship_States.Docked)
				curSCGButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_Docked.png");
			else if (scg.ShipState == Ship_States.UnDocked && scg.Scanning)
				curSCGButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_Scanning.png");
			else if (scg.ShipState == Ship_States.UnDocked && scg.Mining)
				curSCGButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_Mining.png");
			else if (scg.ShipState == Ship_States.UnDocked && scg.DFCC)
				curSCGButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_UnDocked_DFCC.png");
			else if (scg.ShipState == Ship_States.UnDocked && !scg.DFCC)
				curSCGButton.TextureNormal = SpriteManager.LoadImage(SpriteBasePath + "Ship_UnDocked.png");

			scgCount++;
		}

		if (HoveredButton != null)
		{
			GameCore.HoverText = HoverTexts.GetValueOrDefault(HoveredButton, "");
			if (!HoveredButton.Visible)
				HoveredButton = null;
		}
	}

	private void UpdateShipDetails(TextureButton button, InterStellarShip ship)
	{
		var location = ship.PlanetLocation.ToScreenString(" ");
		if (ship.ShipState == Ship_States.InTransit)
			location += " to " + ship.DestinationPlanetLocation.ToScreenString(" ");
		HoverTexts[button] = ship.Name + ": " + location;
		var count = button.GetNode<Godot.Label>("DroneCount");
		count.Text = ship.DroneCount.ToString();
		count.Visible = ship.DFCC;
	}
}
