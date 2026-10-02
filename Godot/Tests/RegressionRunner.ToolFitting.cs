using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Utility;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private Ship ToolFittingShip(Ship_Types hull)
        {
            Ship ship = hull == Ship_Types.Shuttle ? new Shuttle() : hull == Ship_Types.IOS ? new IOS() : new SCG();
            ship.ShipType = hull;
            ship.ShipState = Ship_States.Docked;
            ship.PlanetLocation = ship.DestinationPlanetLocation = StellarBodies.earth;
            ship.StarLocation = ship.DestinationStarLocation = StellarBodies.the_sun;
            ship.Name = "Tool fitting";
            ship.FuelType = hull == Ship_Types.SCG ? ItemTypes.hed_fuel : ItemTypes.meh_fuel;
            ship.Modules = Enumerable.Range(0, hull == Ship_Types.Shuttle ? 1 : hull == Ship_Types.IOS ? 3 : 5)
                .Select(_ => new ShipModule { ModuleType = Module_Types.Tool }).ToList();
            Save.Ships.Clear();
            Save.Ships.Add(ship);
            GameCore.Earth.Station.Built = true;
            GameCore.Earth.Station.BuildParts = 8;
            return ship;
        }

        private ShipBay OpenToolFittingBay(Ship ship)
        {
            var bay = OpenUi<ShipBay>("res://Screens/ShipBay.tscn", new List<SceneVariables>
                { SceneVariables.Orbit, ship.ShipType == Ship_Types.Shuttle ? SceneVariables.Shuttle : SceneVariables.Ship });
            Press(bay, "Buttons/ShipNav/Nav_Torso1");
            Press(bay, "ShipContainer/ScrollContainer2/HBoxContainer/Torso1/SpriteHolder/Buttons/ActivatePod");
            return bay;
        }

        private string[] ToolFittingRows(ShipBay bay) => Enumerable.Range(0, 11)
            .Select(i => bay.GetNode<Label>("EquipmentStock/BackgroundBox/PanelContainer/GridContainer/NameColumn/" + i.ToString("00")))
            .Where(label => label.Visible).Select(label => label.Text).ToArray();

        private async Task ToolFittingHull(Ship_Types hull)
        {
            InitializeUi();
            var data = Save.BaseGameData;
            foreach (var item in data.ItemList.Where(i => i.Research != null))
            {
                item.Research.Researched = true;
                item.Research.ResearchOrder = 100 - item.Research.Index;
                item.Locked = false;
                GameCore.Earth.Station.Resources.Stores[item.ItemType] = 2;
            }
            var expected = hull switch
            {
                Ship_Types.Shuttle => new[] { ItemTypes.derrick, ItemTypes.of_frame, ItemTypes.a__c__c, ItemTypes.bandaid, ItemTypes.r_frame },
                Ship_Types.IOS => new[] { ItemTypes.derrick, ItemTypes.of_frame, ItemTypes.pulse_blaster_laser, ItemTypes.a__c__c, ItemTypes.bandaid,
                    ItemTypes.grapple, ItemTypes.d__f__c__c, ItemTypes.a__m__a, ItemTypes.r_frame, ItemTypes.commspod, ItemTypes.sonic_blaster },
                _ => new[] { ItemTypes.alien_artifact, ItemTypes.derrick, ItemTypes.of_frame, ItemTypes.a__c__c, ItemTypes.bandaid,
                    ItemTypes.grapple, ItemTypes.d__f__c__c, ItemTypes.r_frame, ItemTypes.commspod, ItemTypes.prison_pod, ItemTypes.sonic_blaster }
            };
            var ship = ToolFittingShip(hull);
            if (hull == Ship_Types.IOS)
            {
                var oldBlaser = data.ItemList.Single(i => i.ItemType == ItemTypes.pulse_blaster_laser);
                oldBlaser.ToolPod = oldBlaser.ToolPodSingular = false;
            }
            var tweens = GetTree().GetProcessedTweens().ToHashSet();
            var bay = OpenToolFittingBay(ship);
            try
            {
                Equal(string.Join("|", expected.Select(t => data.ItemList.Single(i => i.ItemType == t).ShortName)),
                    string.Join("|", ToolFittingRows(bay)), "original hull-specific item list");
                var last = expected.Last();
                PressEquipmentNamed(bay, data.ItemList.Single(i => i.ItemType == last).ShortName);
                Equal(last, ship.Modules[0].ItemStored, "last visible row is selectable");
                Equal(1, ship.Modules[0].ItemCount, "single tool fitted");
                Equal(1, GameCore.Earth.Station.Resources.Stores[last], "one unit debited");
                Equal(data.ItemList.Single(i => i.ItemType == last).ShortName,
                    bay.GetNode<Label>("ShipContainer/ScrollContainer2/HBoxContainer/Torso1/SpriteHolder/Labels/Contents").Text, "fitted label matches selected equipment");
                if (hull == Ship_Types.IOS)
                {
                    PressEquipmentNamed(bay, "Blaser");
                    Equal(1, ship.Modules[0].ItemCount, "legacy Blaser flags cannot cause stacked equipment");
                    Equal(1, GameCore.Earth.Station.Resources.Stores[ItemTypes.pulse_blaster_laser], "one Blaser remains in stock");
                }
                if (hull == Ship_Types.Shuttle)
                {
                    PressEquipmentNamed(bay, "Derrick");
                    Equal(2, ship.Modules[0].ItemCount, "Derricks retain multi-unit fitting");
                    PressEquipmentNamed(bay, "Derrick");
                    Equal(2, GameCore.Earth.Station.Resources.Stores[ItemTypes.derrick], "all Derricks returned on removal");
                }
                await InputFrames();
                await CaptureDisplayEvidence("tool-fitting-" + hull);
            }
            finally { CloseDismantleBay(bay, tweens); }
        }

        private async Task ToolFittingLegacyEquipment()
        {
            InitializeUi();
            var ship = ToolFittingShip(Ship_Types.SCG);
            ship.Modules[0].ItemStored = ItemTypes.a__m__a;
            ship.Modules[0].ItemCount = 1;
            var ama = GameCore.SingletonInstance.GameData.GetItem(ItemTypes.a__m__a);
            var grapple = GameCore.SingletonInstance.GameData.GetItem(ItemTypes.grapple);
            ama.Research.Researched = grapple.Research.Researched = true;
            ama.Research.ResearchOrder = 2;
            grapple.Research.ResearchOrder = 3;
            var stores = GameCore.Earth.Station.Resources.Stores;
            stores[ItemTypes.a__m__a] = 2;
            stores[ItemTypes.grapple] = 0;
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            ship = (Ship)Save.Ships.Single();
            stores = GameCore.Earth.Station.Resources.Stores;
            var tweens = GetTree().GetProcessedTweens().ToHashSet();
            var bay = OpenToolFittingBay(ship);
            try
            {
                Equal(false, ToolFittingRows(bay).Contains(ama.ShortName), "incompatible equipment is not offered for fitting");
                PressEquipmentNamed(bay, grapple.ShortName);
                Equal(ItemTypes.a__m__a, ship.Modules[0].ItemStored, "unavailable replacement preserves old hardware");
                Equal(2, stores[ItemTypes.a__m__a], "failed replacement does not refund hardware");
                stores[ItemTypes.grapple] = 1;
                PressEquipmentNamed(bay, grapple.ShortName);
                Equal(ItemTypes.grapple, ship.Modules[0].ItemStored, "valid replacement fits");
                Equal(3, stores[ItemTypes.a__m__a], "old hardware returned exactly once");
                Equal(0, stores[ItemTypes.grapple], "replacement paid once");
                await InputFrames();
            }
            finally { CloseDismantleBay(bay, tweens); }
        }

        private async Task ArtifactManufactureToFitting()
        {
            InitializeUi();
            var core = GameCore.SingletonInstance;
            for (var i = 0; i < 8; i++) core.TriggerAlienTechDiscovery(ItemTypes.alien_artifact);
            var ship = ToolFittingShip(Ship_Types.SCG);
            var factory = GameCore.Earth.Station.Factory;
            factory.AOC = true;
            var screen = OpenMtxProduction();
            try
            {
                screen.Buttons.Single(b => b.ObjectData?.ItemType == ItemTypes.alien_artifact).EmitSignal(BaseButton.SignalName.Pressed);
                ProductionDays(20);
            }
            finally { screen.Free(); }
            var stores = GameCore.Earth.Station.Resources.Stores;
            Equal(1, stores[ItemTypes.alien_artifact], "recovered device manufactured normally");
            var tweens = GetTree().GetProcessedTweens().ToHashSet();
            var bay = OpenToolFittingBay(ship);
            try
            {
                Equal("Unknown", ToolFittingRows(bay).First(), "original item order is independent of research completion order");
                PressEquipmentNamed(bay, "Unknown");
                Equal(ItemTypes.alien_artifact, ship.Modules[0].ItemStored, "manufactured device fitted");
                Equal("Unknown", bay.GetNode<Label>("ShipContainer/ScrollContainer2/HBoxContainer/Torso1/SpriteHolder/Labels/Contents").Text, "original device label retained after fitting");
                Equal(1, ship.Modules[0].ItemCount, "one fitted device");
                Equal(0, stores[ItemTypes.alien_artifact], "fitting consumes stock");
                var restored = SaveStorage.Deserialize(SaveStorage.Serialize(Save)).Ships.Single();
                Equal(ItemTypes.alien_artifact, restored.Modules[0].ItemStored, "fitted device survives save/load");
                PressEquipmentNamed(bay, "Unknown");
                Equal(1, stores[ItemTypes.alien_artifact], "removal returns one device");
                // Multiple manufactured devices must not be fitted as a stack.
                stores[ItemTypes.alien_artifact] = 2;
                PressEquipmentNamed(bay, "Unknown");
                Equal(1, ship.Modules[0].ItemCount, "one device per tool slot");
                Equal(1, stores[ItemTypes.alien_artifact], "spare device remains in stores");
                await InputFrames();
                await CaptureDisplayEvidence("artifact-fitted");
            }
            finally { CloseDismantleBay(bay, tweens); }
        }
    }
}
