using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Utility;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        public void RunOverviewBacklogRegressions()
        {
            CheckUi("Overview station hover shows location and clears when captured", OverviewStationHover);
            foreach (var type in new[] { Ship_Types.IOS, Ship_Types.SCG })
            {
                CheckUi($"Overview {type} hover follows ship location and clears on exit", () => OverviewShipHover(type));
                CheckUi($"Overview {type} DFCC count follows fleet and hides when removed", () => OverviewDroneCount(type));
            }
        }

        private static void RefreshOverview(global::Overview overview)
        {
            typeof(global::Overview).GetMethod("DayTick", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(overview, new object[] { (uint)0, (uint)1 });
        }

        private InterStellarShip OverviewShip(Ship_Types type)
        {
            Save.Ships.Clear();
            InterStellarShip ship = type == Ship_Types.IOS ? new IOS() : new SCG();
            ship.ShipType = type;
            ship.Name = "Explorer";
            ship.PlanetLocation = StellarBodies.earth;
            ship.DestinationPlanetLocation = StellarBodies.the_moon;
            ship.StarLocation = ship.DestinationStarLocation = StellarBodies.the_sun;
            ship.ShipState = Ship_States.UnDocked;
            Save.Ships.Add(ship);
            return ship;
        }

        private void OverviewStationHover()
        {
            foreach (var planet in Save.BaseGameData.Planets.Values) planet.Station.BuildParts = 0;
            GameCore.Earth.Station.BuildParts = 8;
            GameCore.Earth.Station.Built = true;
            GameCore.HoverText = "";
            var overview = OpenUi<global::Overview>("res://Screens/Overview.tscn");
            try
            {
                var button = overview.GetNode<TextureButton>("Stations/Col0/Station00");
                button.EmitSignal(Control.SignalName.MouseEntered);
                Equal("Earth", GameCore.HoverText, "station location hover");
                button.EmitSignal(Control.SignalName.MouseExited);
                Equal("", GameCore.HoverText, "station hover clears on exit");
                button.EmitSignal(Control.SignalName.MouseEntered);
                GameCore.Earth.ActiveMethanoid = true;
                RefreshOverview(overview);
                Equal(false, button.Visible, "captured station hidden");
                Equal("", GameCore.HoverText, "hidden station cannot leave stale hover");
            }
            finally { overview.Free(); }
        }

        private void OverviewShipHover(Ship_Types type)
        {
            var ship = OverviewShip(type);
            GameCore.HoverText = "";
            var overview = OpenUi<global::Overview>("res://Screens/Overview.tscn");
            try
            {
                var button = overview.GetNode<TextureButton>($"{type}/Col0/{type}00");
                button.EmitSignal(Control.SignalName.MouseEntered);
                Equal("Explorer: Earth", GameCore.HoverText, "ship name and location hover");
                ship.ShipState = Ship_States.InTransit;
                RefreshOverview(overview);
                Equal("Explorer: Earth to The Moon", GameCore.HoverText, "hover tracks travel route without re-entering");
                ship.PlanetLocation = StellarBodies.the_moon;
                ship.ShipState = Ship_States.Docked;
                RefreshOverview(overview);
                Equal("Explorer: The Moon", GameCore.HoverText, "hover tracks arrival");
                button.EmitSignal(Control.SignalName.MouseExited);
                Equal("", GameCore.HoverText, "ship hover clears on exit");
                button.EmitSignal(Control.SignalName.MouseEntered);
            }
            finally { overview.Free(); }
            Equal("", GameCore.HoverText, "leaving overview clears its hover");
        }

        private void OverviewDroneCount(Ship_Types type)
        {
            var ship = OverviewShip(type);
            ship.DFCC = true;
            ship.DroneCount = 12;
            var overview = OpenUi<global::Overview>("res://Screens/Overview.tscn");
            try
            {
                var button = overview.GetNode<TextureButton>($"{type}/Col0/{type}00");
                var count = button.GetNodeOrNull<Label>("DroneCount");
                Equal(true, count != null, "DFCC drone count is displayed");
                Equal(true, count.IsVisibleInTree(), "DFCC count visible");
                Equal("12", count.Text, "initial fleet count");
                Equal(Control.MouseFilterEnum.Ignore, count.MouseFilter, "count does not intercept ship input");
                ship.DroneCount = 0;
                RefreshOverview(overview);
                Equal("0", count.Text, "zero fleet count remains informative");
                ship.DFCC = false;
                RefreshOverview(overview);
                Equal(false, count.Visible, "no count for ship without DFCC");
                ship.DFCC = true;
                ship.DroneCount = 200;
                RefreshOverview(overview);
                Equal(true, count.Visible, "DFCC count returns when installed");
                Equal("200", count.Text, "full fleet count");
                Save.Ships.Clear();
                RefreshOverview(overview);
                Equal(false, count.IsVisibleInTree(), "removed fleet leaves no visible count");
            }
            finally { overview.Free(); }
        }
        private static List<TextureButton> VisibleOverviewButtons(global::Overview overview, string group) =>
            overview.GetNode(group).GetChildren().SelectMany(column => column.GetChildren())
                .OfType<TextureButton>().Where(button => button.Visible).ToList();

        private async Task OverviewStationCapacity()
        {
            InitializeUi(); GameCore.SingletonInstance.SetProcess(false); Save.Ships.Clear();
            foreach (var planet in Save.BaseGameData.Planets.Values) planet.Station.BuildParts = 0;
            var planets = Save.BaseGameData.Planets.Values.Take(34).ToArray();
            for (var i = 0; i < planets.Length; i++)
            {
                planets[i].ActiveMethanoid = i == 33;
                planets[i].Station.Built = true; planets[i].Station.BuildParts = 8; planets[i].Station.StationOrdinal = i;
            }
            var overview = OpenUi<global::Overview>("res://Screens/Overview.tscn");
            try
            {
                var seen = new HashSet<int>();
                for (var page = 0; page < 3; page++)
                {
                    Equal(page == 2 ? 1 : 16, VisibleOverviewButtons(overview, "Stations").Count, "bounded station page");
                    foreach (var button in VisibleOverviewButtons(overview, "Stations"))
                        Equal(true, seen.Add(button.GetMeta("planetid").AsInt32()), "station appears once");
                    if (page < 2) Press(overview, "Pages/Next");
                }
                Equal(33, seen.Count, "all friendly stations reachable");
                Equal(false, seen.Contains((int)planets[33].PlanetId), "hostile station excluded before indexing");
                var last = VisibleOverviewButtons(overview, "Stations").Single();
                last.EmitSignal(Control.SignalName.MouseEntered);
                Equal(planets[32].PlanetId.ToScreenString(" "), GameCore.HoverText, "last-page station hover");
                Press(overview, "Pages/Next");
                Equal("Page 3/3", overview.GetNode<Label>("Pages/Page").Text, "retained next cannot exceed last page");
                for (var i = 16; i < 33; i++) planets[i].ActiveMethanoid = true;
                RefreshOverview(overview);
                Equal(false, overview.GetNode<Control>("Pages").Visible, "one-page controls hidden after capture");
                Equal(5, overview.GetNode<VBoxContainer>("Stations/Col0").GetThemeConstant("separation"), "single-page station spacing restored");
                Equal(16, VisibleOverviewButtons(overview, "Stations").Count, "sixteen friendly plus hostile tail remains valid");
                Equal(planets[0].PlanetId.ToScreenString(" "), GameCore.HoverText, "hover follows remapped identity");
                Press(overview, "Stations/Col0/Station00"); await InputFrames();
                Equal(planets[0].PlanetId, Save.CurrentPlanet, "visible station opens correct body");
                Equal(Scenes.Station, GameCore.SingletonInstance.currentScene, "station navigation retained");
                await CaptureDisplayEvidence("overview-station-capacity");
            }
            finally { overview.Free(); }
        }

        private async Task OverviewFleetCapacity(bool scg)
        {
            InitializeUi(); var core = GameCore.SingletonInstance; core.SetProcess(false); Save.Ships.Clear();
            foreach (var planet in Save.BaseGameData.Planets.Values) planet.Station.BuildParts = 0;
            var ships = new List<InterStellarShip>();
            for (var i = 0; i < 33; i++)
            {
                InterStellarShip ship = scg ? new SCG() : new IOS();
                ship.ShipType = scg ? Ship_Types.SCG : Ship_Types.IOS;
                ship.Name = "Fleet " + i; ship.ShipState = Ship_States.UnDocked;
                ship.PlanetLocation = ship.DestinationPlanetLocation = StellarBodies.earth;
                ship.StarLocation = ship.DestinationStarLocation = StellarBodies.the_sun;
                ship.FuelType = scg ? ItemTypes.hed_fuel : ItemTypes.meh_fuel;
                ship.Modules = Enumerable.Range(0, scg ? 6 : 3).Select(_ => new ShipModule()).ToList();
                ship.DFCC = true; ship.DroneCount = i;
                ships.Add(ship); Save.Ships.Add(ship);
            }
            var overview = OpenUi<global::Overview>("res://Screens/Overview.tscn"); var group = scg ? "SCG" : "IOS";
            try
            {
                Equal(16, VisibleOverviewButtons(overview, group).Count, "first fleet page bounded");
                await InputFrames();
                Equal(false, overview.GetNode<Control>("Pages").GetGlobalRect().Intersects(ActiveScreen<Deuteros.Code.Platform.Screens.MainMenu>().GetNode<Label>("HoverInfo").GetGlobalRect()), "page controls must not cover hover text");
                foreach (var button in VisibleOverviewButtons(overview, group))
                {
                    Equal(false, button.GetGlobalRect().Intersects(overview.GetNode<Control>("Pages").GetGlobalRect()), "paging does not cover fleet icons");
                    Equal(true, button.GetGlobalRect().End.Y <= 184, "last fleet row leaves footer controls reachable");
                }
                GlobalInput.LockUi(); Press(overview, "Pages/Next"); GlobalInput.UnlockUi();
                Equal("Page 1/3", overview.GetNode<Label>("Pages/Page").Text, "page respects UI lock");
                OverlayManager.Instance.ShowOverlay(GD.Load<PackedScene>("res://Screens/Settings/SettingsScreen.tscn"), true, true);
                Press(overview, "Pages/Next");
                Equal("Page 1/3", overview.GetNode<Label>("Pages/Page").Text, "retained page command cannot bypass overlay");
                OverlayManager.Instance.CloseOverlay(); await InputFrames();
                Press(overview, "Pages/Next"); Press(overview, "Pages/Next");
                var last = VisibleOverviewButtons(overview, group).Single();
                Equal(ships[32].ShipID.ToString(), last.GetMeta("shipid").AsString(), "last ship reachable");
                Equal("32", last.GetNode<Label>("DroneCount").Text, "last ship drone count");
                last.EmitSignal(Control.SignalName.MouseEntered);
                Equal("Fleet 32: Earth", GameCore.HoverText, "last ship hover");
                await CaptureDisplayEvidence("overview-fleet-capacity-" + group);
                Save.Ships.Remove(ships[32]); RefreshOverview(overview);
                Equal("Page 2/2", overview.GetNode<Label>("Pages/Page").Text, "lost last ship clamps current page");
                Press(overview, "Pages/Previous");
                Equal("Page 1/2", overview.GetNode<Label>("Pages/Page").Text, "previous returns first page");
                Press(overview, group + "/Col0/" + group + "00"); await InputFrames();
                Equal(ships[0].ShipID, ActiveScreen<Deuteros.Code.Platform.Screens.ShipInterior>().Ship.ShipID, "visible fleet control selects correct ship");
                Equal(Scenes.ShipInterior, core.currentScene, "fleet navigation retained");
                core.ChangeScene(Scenes.Overview, new List<SceneVariables>());
                await InputFrames();
                var liveOverview = ActiveScreen<global::Overview>();
                ships[0].ShipState = Ship_States.Docked;
                GameCore.Earth.Station.Built = true;
                GameCore.Earth.Station.BuildParts = 8;
                GameCore.Earth.Station.SdmCountdown = 1;
                Save.CurrentPlanet = StellarBodies.the_moon;
                SdmSystem.AdvanceTime(1);
                Equal(false, Save.Ships.Contains(ships[0]), "real-time SDM removes the docked ship");
                Equal(ships[1].ShipID.ToString(), VisibleOverviewButtons(liveOverview, group).First().GetMeta("shipid").AsString(),
                    "global overview refreshes immediately even when another planet is selected");
                Press(liveOverview, group + "/Col0/" + group + "00"); await InputFrames();
                Equal(ships[1].ShipID, ActiveScreen<Deuteros.Code.Platform.Screens.ShipInterior>().Ship.ShipID,
                    "post-destruction fleet button selects a surviving ship");
            }
            finally { GlobalInput.UnlockUi(); if (OverlayManager.Instance.IsOpen) OverlayManager.Instance.CloseOverlay(); overview.Free(); }
        }
    }
}
