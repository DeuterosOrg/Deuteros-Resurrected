using System.Reflection;
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
    }
}
