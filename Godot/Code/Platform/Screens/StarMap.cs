using Deuteros.Code.Objects;
using Deuteros.Code.Objects.Interfaces;
using Deuteros.Code.Platform.Base;
using Deuteros.Code.Platform.Helpers;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deuteros.Code.Platform.Screens
{
    public partial class StarMap : BaseSubScene
    {
        public const string StarMapSpriteBasePath = "res://Sprites//SceneSprites//Map//";

        [Export]
        public bool ShowResources { get; set; }

        public Control PlanetHolder { get; set; }
        public Control StarSystemHolder { get; set; }
        public Control StarMapHolder { get; set; }

        public TextureButton[] MoonButtons { get; set; } = new TextureButton[11];
        public Button[] PlanetButtons { get; set; } = new Button[11];
        public Button PlanetGoBack { get; set; }
        public Button StarSystemGoBack { get; set; }

        public TextureRect Planet { get; set; }
        public TextureRect StarSystem { get; set; }
        private TextureRect resourceStation;

        public Label SunLabel { get; set; }
        public Label SelectedLocationLabel { get; set; }
        public Label StarCountLabel { get; set; }
        public Label StarSummaryLabel { get; set; }
        public Label PlanetLabel { get; set; }
        public Label MoonLabel { get; set; }
        public Label DepositsLabel { get; set; }
        public List<Label> DepositLabels { get; set; }

        public Enums.StellarBodies CurrentLocation { get; set; }
        public Enums.StellarBodies SelectedStar { get; set; }
        public Enums.StellarBodies SelectedPlanet { get; set; }
        public Enums.StellarBodies SelectedMoon { get; set; }

        public Action PlanetGoBackAction { get; set; }

        public override void _Ready()
        {
            PlanetHolder = GetNode<Control>("PlanetHolder");
            StarSystemHolder = GetNode<Control>("StarSystemHolder");
            StarMapHolder = GetNode<Control>("StarMapHolder");

            Planet = GetNode<TextureRect>("PlanetHolder/Planet");
            StarSystem = GetNode<TextureRect>("StarSystemHolder/StarSystem");
            resourceStation = GetNode<TextureRect>("ResourceStation");

            PlanetGoBack = GetNode<Button>("PlanetHolder/GoBack");
            StarSystemGoBack = GetNode<Button>("StarSystemHolder/GoBack");

            StarSystemGoBack.Pressed += SelectStarGoBack_Pressed;

            (GetNode<Button>("StarMapHolder/Buttons/The Sun")).Pressed += () => SelectStar_Pressed(Enums.StellarBodies.the_sun);
            (GetNode<Button>("StarMapHolder/Buttons/Proxima")).Pressed += () => SelectStar_Pressed(Enums.StellarBodies.proxima);
            (GetNode<Button>("StarMapHolder/Buttons/Centauri")).Pressed += () => SelectStar_Pressed(Enums.StellarBodies.centauri);
            (GetNode<Button>("StarMapHolder/Buttons/Barnard")).Pressed += () => SelectStar_Pressed(Enums.StellarBodies.barnard);
            (GetNode<Button>("StarMapHolder/Buttons/Lalande")).Pressed += () => SelectStar_Pressed(Enums.StellarBodies.lalande);
            (GetNode<Button>("StarMapHolder/Buttons/Sirius")).Pressed += () => SelectStar_Pressed(Enums.StellarBodies.sirius);
            (GetNode<Button>("StarMapHolder/Buttons/Cygni")).Pressed += () => SelectStar_Pressed(Enums.StellarBodies.cygni);
            (GetNode<Button>("StarMapHolder/Buttons/Procyon")).Pressed += () => SelectStar_Pressed(Enums.StellarBodies.procyon);
            (GetNode<Button>("StarMapHolder/Buttons/Tau Ceti")).Pressed += () => SelectStar_Pressed(Enums.StellarBodies.tau_ceti);

            SunLabel = GetNode<Label>("Labels/Sun");
            SelectedLocationLabel = GetNode<Label>("Labels/SelectedLocation");
            StarCountLabel = GetNode<Label>("Labels/StarCount");
            StarSummaryLabel = GetNode<Label>("Labels/StarSummary");
            PlanetLabel = GetNode<Label>("Labels/Planet");
            MoonLabel = GetNode<Label>("Labels/Moon");
            DepositsLabel = GetNode<Label>("Labels/Deposits");

            DepositLabels = new List<Label>();

            for (int i = 0; i < 8; i++)
            {
                DepositLabels.Add(GetNode<Label>("Labels/Deposit" + i.ToString().PadLeft(2, '0')));
            }

            for (int i = 0; i < MoonButtons.Length; i++)
            {
                MoonButtons[i] = GetNode<TextureButton>("PlanetHolder/Moons/Moon" + i.ToString().PadLeft(2, '0'));
            }

            for (int i = 0; i < PlanetButtons.Length; i++)
            {
                PlanetButtons[i] = GetNode<Button>("StarSystemHolder/Planets/Planet" + i.ToString().PadLeft(2, '0'));
            }

            if (!ShowResources)
            {
                SelectedLocationLabel.Visible = false;
                StarCountLabel.Visible = false;
                StarSummaryLabel.Visible = false;
                DepositLabels.ForEach(x => x.Visible = false);
                DepositsLabel.Visible = false;
            }

            LoadMap(Enums.StellarBodies.none);

            base._Ready();
        }

        public void LoadMap(Enums.StellarBodies currentLocation)
        {
            //Sense check - If we have no location, but the starmap is not available then 
            if (currentLocation == Enums.StellarBodies.none && !GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Contains(Enums.Game_Unlocks.Interstellar_Travel))
            {
                SelectedStar = Enums.StellarBodies.the_sun;
                SelectedPlanet = Enums.StellarBodies.none;
                SelectedMoon = Enums.StellarBodies.none;

                CurrentLocation = Enums.StellarBodies.the_sun;
            }
            else if (currentLocation == Enums.StellarBodies.none && GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Contains(Enums.Game_Unlocks.Interstellar_Travel))
            {
                SelectedStar = Enums.StellarBodies.the_sun;
                SelectedPlanet = Enums.StellarBodies.none;
                SelectedMoon = Enums.StellarBodies.none;

                CurrentLocation = Enums.StellarBodies.none;
            }
            else if (GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Stars.ContainsKey(currentLocation))
            {
                SelectedStar = currentLocation;
                SelectedPlanet = Enums.StellarBodies.none;
                SelectedMoon = Enums.StellarBodies.none;

                CurrentLocation = currentLocation;
            }
            else
            {
                var currentPlanet = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[currentLocation];

                SelectedStar = currentPlanet.ParentStar;

                if (currentPlanet.IsMoon)
                {
                    SelectedPlanet = currentPlanet.MoonParentPlanetId;
                    SelectedMoon = currentLocation;
                }
                else
                {
                    SelectedPlanet = currentLocation;
                    SelectedMoon = Enums.StellarBodies.none;
                }

                CurrentLocation = currentLocation;
            }

            UpdateMap();
        }

        private void SelectMoon_Pressed(int moonIndex)
        {
            var currentPlanet = (Planet)GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[SelectedPlanet];
            var currentMoon = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets.Single(T => T.Value.MoonParentPlanetId == currentPlanet.PlanetId && T.Value.Order == moonIndex).Value.PlanetId;

            CurrentLocation = currentMoon;
            SelectedMoon = currentMoon;

            UpdateMap();
        }

        private void SelectPlanet_Pressed(Enums.StellarBodies selectedPlanet)
        {
            CurrentLocation = selectedPlanet;
            SelectedPlanet = selectedPlanet;
            SelectedMoon = Enums.StellarBodies.none;

            UpdateMap();
        }

        private void SelectStar_Pressed(Enums.StellarBodies selectedStar)
        {
            CurrentLocation = selectedStar;

            SelectedStar = selectedStar;

            SelectedPlanet = Enums.StellarBodies.none;
            SelectedMoon = Enums.StellarBodies.none;

            UpdateMap();
        }

        private void SelectStarGoBack_Pressed()
        {
            CurrentLocation = Enums.StellarBodies.none;
            SelectedPlanet = Enums.StellarBodies.none;
            SelectedMoon = Enums.StellarBodies.none;

            UpdateMap();
        }

        private void PlanetGoBack_Pressed(Enums.StellarBodies selectedStar)
        {
            CurrentLocation = selectedStar;

            SelectedMoon = Enums.StellarBodies.none;

            UpdateMap();
        }

        public void UpdateMap()
        {
            resourceStation.Visible = false;
            //Blank out all the optional labels
            StarCountLabel.Text = "";
            StarSummaryLabel.Text = "";
            PlanetLabel.Text = "";
            MoonLabel.Text = "";
            DepositsLabel.Text = "";

            for (int i = 0; i < 8; i++)
            {
                DepositLabels[i].Text = "";
            }

            StarSystemGoBack.Visible = GameCore.SingletonInstance.GameData.ActiveSaveFile.Unlocks.Contains(Enums.Game_Unlocks.Interstellar_Travel);

            if (SelectedMoon != Enums.StellarBodies.none)
                SelectedLocationLabel.Text = SelectedMoon.ToScreenString(" ");
            else if (SelectedPlanet != Enums.StellarBodies.none)
                SelectedLocationLabel.Text = SelectedPlanet.ToScreenString(" ");
            else
                SelectedLocationLabel.Text = SelectedStar.ToScreenString(" ");

            //This is the star map
            if (CurrentLocation == Enums.StellarBodies.none)
            {
                PlanetHolder.Visible = false;
                StarSystemHolder.Visible = false;
                StarMapHolder.Visible = true;

                SunLabel.Text = SelectedStar.ToScreenString(" ");

                var starData = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Stars[SelectedStar];

                StarCountLabel.Text = (starData.PlanetDistanceList.Count / 2).ToString();
                StarSummaryLabel.Text = "Planetary Systems";
            }
            //This is a star
            else if ((int)CurrentLocation >= 100000)
            {
                PlanetHolder.Visible = false;
                StarSystemHolder.Visible = true;
                StarMapHolder.Visible = false;

                var starData = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Stars[CurrentLocation];
                var planetData = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets.Where(T => T.Value.ParentStar == CurrentLocation && !T.Value.IsMoon).OrderBy(T => T.Value.Order).ToList();

                StarSystem = SpriteManager.LoadImageToTextureRect(StarMapSpriteBasePath + "//Star_" + CurrentLocation.ToScreenString("_") + ".png", StarSystem);

                SunLabel.Text = CurrentLocation.ToScreenString(" ");

                for (int i = 0; i < 11; i++)
                {
                    if (i < starData.PlanetDistanceList.Count() / 2)
                    {
                        var planetId = planetData[i].Value.PlanetId;

                        PlanetButtons[i].Position = new Vector2(starData.PlanetDistanceList[i * 2], PlanetButtons[i].Position.Y);
                        PlanetButtons[i].Size = new Vector2(starData.PlanetDistanceList[i * 2 + 1] - starData.PlanetDistanceList[i * 2], PlanetButtons[i].Size.Y);
                        PlanetButtons[i].Pressed += () => SelectPlanet_Pressed(planetId);
                        PlanetButtons[i].Visible = true;
                    }
                    else
                    {
                        PlanetButtons[i].Visible = false;
                        Utility.Buttons.ClearPressedConnections(PlanetButtons[i]);
                    }
                }

                //A planet is selected, show the deposits and set text
                if (SelectedPlanet != Enums.StellarBodies.none)
                {
                    PlanetLabel.Text = SelectedPlanet.ToScreenString(" ");

                    ShowDeposits(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[SelectedPlanet]);
                }
                else
                {
                    StarCountLabel.Text = (starData.PlanetDistanceList.Count / 2).ToString();
                    StarSummaryLabel.Text = "Planetary Systems";
                }
            }
            //This is a planet
            else if ((int)CurrentLocation < 100000)
            {
                PlanetHolder.Visible = true;
                StarSystemHolder.Visible = false;
                StarMapHolder.Visible = false;

                IPlanet currentPlanet;

                if (SelectedMoon == Enums.StellarBodies.none)
                    currentPlanet = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[CurrentLocation];
                else
                    currentPlanet = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[SelectedMoon].MoonParentPlanetId];

                Planet = SpriteManager.LoadImageToTextureRect(StarMapSpriteBasePath + "//Planet_" + currentPlanet.PlanetImageName() + ".png", Planet);

                if (PlanetGoBackAction != null)
                    PlanetGoBack.Pressed -= PlanetGoBackAction;
                PlanetGoBackAction = () => PlanetGoBack_Pressed(currentPlanet.ParentStar);
                PlanetGoBack.Pressed += PlanetGoBackAction;

                SunLabel.Text = currentPlanet.ParentStar.ToScreenString(" ");
                PlanetLabel.Text = SelectedPlanet.ToScreenString(" ");

                var moonList = currentPlanet.IsMoon
                    //If this is a moon, grab the moonlist from the parent planet
                    ? GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[currentPlanet.MoonParentPlanetId].MoonList
                    : currentPlanet.MoonList;

                var moonCount = 0;

                for (int i = 0; i < 11; i++)
                {
                    Utility.Buttons.ClearPressedConnections(MoonButtons[i]);

                    if (moonList.Contains(i))
                    {
                        MoonButtons[i].Visible = true;

                        var index = moonCount;

                        MoonButtons[i].Pressed += () => SelectMoon_Pressed(index);

                        moonCount++;
                    }
                    else
                    {
                        MoonButtons[i].Visible = false;
                    }
                }

                //A moon is selected, show the deposits and set text
                if (SelectedMoon != Enums.StellarBodies.none)
                {
                    MoonLabel.Text = SelectedMoon.ToScreenString(" ");
                    ShowDeposits(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[SelectedMoon]);
                }
                //Show the planetary deposits
                else
                {
                    ShowDeposits(GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets[SelectedPlanet]);
                }
            }
        }

        private void ShowDeposits(IPlanet selectedPlanet)
        {
            UpdateStationIcon(selectedPlanet);
            DepositsLabel.Text = "Deposits";

            for (int i = 0; i < selectedPlanet.PlanetResources.Materials.Count(); i++)
            {
                DepositLabels[i].Text = selectedPlanet.PlanetResources.Materials[i].MaterialType.ToScreenString(" ");
            }
        }

        protected override void DayTick(uint previousDay, uint currentDay)
        {
            // Refresh station state without rebinding the map's navigation controls.
            var selection = SelectedMoon != Enums.StellarBodies.none ? SelectedMoon : SelectedPlanet;
            var planets = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.Planets;
            UpdateStationIcon(CurrentLocation != Enums.StellarBodies.none && planets.TryGetValue(selection, out var planet) ? planet : null);
        }

        private void UpdateStationIcon(IPlanet planet)
        {
            var station = planet?.Station;
            resourceStation.Visible = ShowResources && station != null && (station.Built || station.BuildParts > 0);
            if (!resourceStation.Visible) return;
            var image = station.Built ? "Station_1" : "Station_UnderConstruction";
            resourceStation.Texture = GD.Load<Texture2D>("res://Sprites/Buttons/Overview/" + image + ".png");
        }
    }
}
