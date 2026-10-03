using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Platform.Screens;
using Godot;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task BayCargoCapacity(Ship_Types hull)
        {
            var bay = await EmptyAssemblyBay(hull);
            var stores = bay.ResourceList.Stores;
            stores[AssemblyChassis(hull)] = 1;
            stores[ItemTypes.meh_fuel] = stores[ItemTypes.hed_fuel] = 600;
            Press(bay, "Buttons/Nav_Create_" + hull);
            var index = hull == Ship_Types.SCG ? 5 : 0;
            var pod = bay.Ship.Modules[index];
            pod.ModuleType = Module_Types.Supply;
            pod.ItemStored = ItemTypes.iron;
            pod.ItemCount = 250;
            stores[ItemTypes.iron] = 49751;
            stores[ItemTypes.titanium] = 300;
            Press(bay, "Buttons/ShipNav/Nav_Torso" + (index + 1));
            Press(bay, ShipParts + "Torso" + (index + 1) + "/SpriteHolder/Buttons/ActivatePod");
            foreach (var selected in new[] { ItemTypes.iron, ItemTypes.titanium })
            {
                Press(bay, "CargoService/Buttons/" + selected.ToScreenString());
                Equal(49751, stores[ItemTypes.iron], "blocked return does not overfill stores");
                Equal(250, pod.ItemCount, "blocked return retains all cargo");
                Equal(ItemTypes.iron, pod.ItemStored, "blocked swap retains original material");
                Equal(300, stores[ItemTypes.titanium], "blocked swap takes no replacement cargo");
                Equal(true, OverlayManager.Instance.IsOpen, "blocked transfer explains capacity requirement");
                await InputFrames();
                var warning = OverlayManager.Instance.GetNode<Label>("GlobalOverlay/Center/Node2D/ErrorButton/OuterColorRect/InnerColorRect/ErrorLabel");
                Equal(true, warning.GetParent<Control>().GetGlobalRect().Encloses(warning.GetGlobalRect()), "capacity message fits inside warning panel");
                await CaptureDisplayEvidence("bay-capacity-" + hull + "-" + selected);
                OverlayManager.Instance.CloseOverlay();
                await InputFrames();
            }
            stores[ItemTypes.iron] = 49750;
            Press(bay, "CargoService/Buttons/" + ItemTypes.titanium.ToScreenString());
            Equal(50000, stores[ItemTypes.iron], "exact capacity accepts original cargo");
            Equal(250, pod.ItemCount, "replacement pod filled once");
            Equal(ItemTypes.titanium, pod.ItemStored, "replacement cargo loaded");
            Equal(50, stores[ItemTypes.titanium], "replacement stock charged once");
            Press(bay, "CargoService/Buttons/" + ItemTypes.titanium.ToScreenString());
            Equal(300, stores[ItemTypes.titanium], "normal unload conserves stock");
            Equal(0, pod.ItemCount, "normal unload empties pod");
            var fuelButton = "CargoService/Buttons/" + bay.Ship.FuelType.ToScreenString();
            foreach (var replacement in new[] { bay.Ship.FuelType, ItemTypes.titanium })
            {
                Press(bay, fuelButton);
                Equal(350, stores[bay.Ship.FuelType], "fuel cargo leaves local stores");
                Equal("350", bay.GetNode<Label>("Fuel/InStock").Text, "loading fuel immediately refreshes bay stock");
                Equal(true, bay.GetNode<Control>("CargoService").Visible, "refresh preserves cargo service");
                Press(bay, "CargoService/Buttons/" + replacement.ToScreenString());
                Equal(600, stores[bay.Ship.FuelType], "unload or replacement returns fuel cargo");
                Equal("600", bay.GetNode<Label>("Fuel/InStock").Text, "returning fuel immediately refreshes bay stock");
                Equal(0, bay.Ship.Fuel, "cargo transfer does not fill the ship tank");
            }
            Cursor.Unlock();
        }

        private async Task BayEquipmentCapacity()
        {
            foreach (var hull in new[] { Ship_Types.IOS, Ship_Types.SCG })
            {
                var bay = await DfccFuelBay(hull, false);
                var ship = (InterStellarShip)bay.Ship;
                var stores = bay.ResourceList.Stores;
                var pod = ship.Modules[0];
                pod.ModuleType = Module_Types.Tool;
                pod.ItemStored = ItemTypes.derrick;
                pod.ItemCount = 2;
                ship.Fuel = 10;
                stores[ship.FuelType] = 25;
                stores[ItemTypes.derrick] = 49999;
                stores[ItemTypes.d__f__c__c] = 1;
                foreach (var type in new[] { ItemTypes.derrick, ItemTypes.d__f__c__c })
                {
                    var item = GameCore.SingletonInstance.GameData.GetItem(type);
                    item.Locked = false;
                    item.Research.Researched = true;
                }
                Press(bay, "Buttons/ShipNav/Nav_Torso1");
                Press(bay, ShipParts + "Torso1/SpriteHolder/Buttons/ActivatePod");
                foreach (var selected in new[] { ItemTypes.derrick, ItemTypes.d__f__c__c })
                {
                    PressEquipmentNamed(bay, GameCore.SingletonInstance.GameData.GetItem(selected).ShortName);
                    Equal(49999, stores[ItemTypes.derrick], "equipment return cannot exceed capacity");
                    Equal(ItemTypes.derrick, pod.ItemStored, "blocked replacement retains equipment");
                    Equal(2, pod.ItemCount, "blocked return retains stack");
                    Equal(10, ship.Fuel, "blocked replacement cannot drain tank");
                    Equal(25, stores[ship.FuelType], "blocked replacement cannot return fuel early");
                    Equal(false, ship.DFCC, "blocked replacement preserves hull mode");
                    Equal(1, stores[ItemTypes.d__f__c__c], "blocked replacement preserves spare equipment");
                    Equal(true, OverlayManager.Instance.IsOpen, "equipment capacity warning shown");
                    await InputFrames();
                    await CaptureDisplayEvidence("bay-equipment-capacity-" + hull + "-" + selected);
                    OverlayManager.Instance.CloseOverlay();
                    await InputFrames();
                }
                stores[ItemTypes.derrick] = 49998;
                PressEquipmentNamed(bay, GameCore.SingletonInstance.GameData.GetItem(ItemTypes.d__f__c__c).ShortName);
                Equal(50000, stores[ItemTypes.derrick], "exact capacity permits equipment return");
                Equal(true, ship.DFCC, "replacement fits after capacity is available");
                Equal(0, ship.Fuel, "successful conversion drains tank at original cost");
                Equal(35, stores[ship.FuelType], "successful conversion returns exactly ten fuel");
                Equal(0, stores[ItemTypes.d__f__c__c], "successful replacement consumes one spare");
                Cursor.Unlock();
            }
        }

        private async Task BayPodCapacity()
        {
            var bay = await EmptyAssemblyBay(Ship_Types.SCG);
            var stores = bay.ResourceList.Stores;
            stores[ItemTypes.g_chassis] = 1;
            Press(bay, "Buttons/Nav_Create_SCG");
            var pod = bay.Ship.Modules[5];
            Press(bay, "Buttons/ShipNav/Nav_Torso6");
            var fittings = new[] { (Module_Types.Supply, ItemTypes.supply_pod, "AddSupplyPod"),
                (Module_Types.Tool, ItemTypes.tool_pod, "AddToolPod"),
                (Module_Types.Cryo, ItemTypes.cryo_pod, "AddCryoPod") };
            foreach (var fitting in fittings)
            {
                GameCore.SingletonInstance.GameData.GetItem(fitting.Item2).Locked = false;
                pod.ModuleType = fitting.Item1;
                stores[fitting.Item2] = 50000;
                Press(bay, ShipParts + "Torso6/SpriteHolder/Buttons/" + fitting.Item3);
                Equal(fitting.Item1, pod.ModuleType, "full spare store retains fitted pod");
                Equal(50000, stores[fitting.Item2], "pod return cannot exceed capacity");
                if (OverlayManager.Instance.IsOpen) OverlayManager.Instance.CloseOverlay();
                await InputFrames();
                var replacement = fittings.First(f => f.Item1 != fitting.Item1);
                stores[replacement.Item2] = 1;
                GameCore.SingletonInstance.GameData.GetItem(replacement.Item2).Locked = false;
                Press(bay, ShipParts + "Torso6/SpriteHolder/Buttons/" + replacement.Item3);
                Equal(fitting.Item1, pod.ModuleType, "blocked swap retains original pod");
                Equal(1, stores[replacement.Item2], "blocked swap takes no spare");
                if (OverlayManager.Instance.IsOpen) OverlayManager.Instance.CloseOverlay();
                await InputFrames();
                stores[fitting.Item2] = 49999;
                Press(bay, ShipParts + "Torso6/SpriteHolder/Buttons/" + replacement.Item3);
                Equal(replacement.Item1, pod.ModuleType, "exact capacity permits pod swap");
                Equal(50000, stores[fitting.Item2], "old pod returned once");
                Equal(0, stores[replacement.Item2], "new pod charged once");
                bay.GetNode<Deuteros.Code.Platform.Screens.ShipBayScenes.Torso>(ShipParts + "Torso6")._Process(2.37);
            }
        }
    }
}
