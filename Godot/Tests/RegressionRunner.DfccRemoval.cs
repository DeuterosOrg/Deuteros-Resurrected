using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Utility;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task<ShipBay> FittedDfccBay(Ship_Types hull)
        {
            var bay = await DfccFuelBay(hull, false);
            var stores = bay.ResourceList.Stores;
            bay.Ship.Modules[0].ModuleType = Module_Types.Tool;
            foreach (var type in new[] { ItemTypes.d__f__c__c, ItemTypes.derrick })
            {
                var item = GameCore.SingletonInstance.GameData.GetItem(type);
                item.Research.Researched = true;
                item.Locked = false;
                stores[type] = 3;
            }
            Press(bay, "Buttons/ShipNav/Nav_Torso1");
            Press(bay, ShipParts + "Torso1/SpriteHolder/Buttons/ActivatePod");
            PressEquipmentNamed(bay, GameCore.SingletonInstance.GameData.GetItem(ItemTypes.d__f__c__c).ShortName);
            Equal(true, ((InterStellarShip)bay.Ship).DFCC, "fixture fitted controller through equipment selection");
            Equal(2, stores[ItemTypes.d__f__c__c], "fixture consumed one controller");
            return bay;
        }

        private async Task DfccRemoval(Ship_Types hull)
        {
            foreach (var selection in new[] { ItemTypes.d__f__c__c, ItemTypes.derrick })
            {
                var bay = await FittedDfccBay(hull);
                var ship = (InterStellarShip)bay.Ship;
                var stores = bay.ResourceList.Stores;
                stores[ship.FuelType] = 100;
                Press(bay, "Fuel/FuelGauge/Plus/RepeatingButton");
                Press(bay, "Fuel/FuelGauge/Plus/RepeatingButton");
                Equal(80, stores[ship.FuelType], "two converted units cost twenty stock");
                var drones = hull == Ship_Types.SCG ? ItemTypes.star_drone : ItemTypes.ios_drone;
                ship.DroneCount = 7;
                stores[drones] = 13;
                PressEquipmentNamed(bay, GameCore.SingletonInstance.GameData.GetItem(selection).ShortName);
                Equal(false, ship.DFCC, "last controller removal ends hull conversion");
                Equal(1, ship.FuelUnitCost, "ordinary fuel cost restored");
                Equal(0, ship.Fuel, "converted tank emptied before rate changes");
                Equal(100, stores[ship.FuelType], "converted fuel returned at original tenfold cost");
                Equal(0, ship.DroneCount, "removed controller cannot retain an active fleet");
                Equal(20, stores[drones], "correct hull's drones returned exactly once");
                Equal(3, stores[ItemTypes.d__f__c__c], "controller returned once");
                Equal(selection == ItemTypes.derrick ? ItemTypes.derrick : ItemTypes.none, ship.Modules[0].ItemStored, "selected equipment action applied");
                Equal(selection == ItemTypes.derrick ? 3 : 0, ship.Modules[0].ItemCount, "replacement amount is correct");
                Press(bay, "Fuel/FuelGauge/Plus/RepeatingButton");
                Equal(1, ship.Fuel, "ordinary refuelling still works");
                Equal(99, stores[ship.FuelType], "subsequent refuelling costs one unit");
                var restored = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                var savedShip = (InterStellarShip)restored.Ships.Single();
                Equal(false, savedShip.DFCC, "save retains removal");
                Equal(0, savedShip.DroneCount, "save retains fleet return");
                await InputFrames();
                await CaptureDisplayEvidence("dfcc-removed-" + hull + "-" + selection);
                var controllerName = GameCore.SingletonInstance.GameData.GetItem(ItemTypes.d__f__c__c).ShortName;
                PressEquipmentNamed(bay, controllerName);
                Equal(true, ship.DFCC, "refitting restores conversion");
                Equal(100, stores[ship.FuelType], "refitting returns ordinary fuel at one to one");
                PressEquipmentNamed(bay, controllerName);
                Equal(false, ship.DFCC, "second removal restores ordinary hull");
                Equal(3, stores[ItemTypes.d__f__c__c], "repeat fit and removal cannot create a controller");
                Equal(20, stores[drones], "repeat removal cannot return drones twice");
                Equal(100, stores[ship.FuelType], "repeat conversion cannot create fuel");
                Cursor.Unlock();
            }
        }

        private async Task DfccRemovalCapacity()
        {
            foreach (var hull in new[] { Ship_Types.IOS, Ship_Types.SCG })
            {
                var bay = await FittedDfccBay(hull);
                var ship = (InterStellarShip)bay.Ship;
                var stores = bay.ResourceList.Stores;
                var drones = hull == Ship_Types.SCG ? ItemTypes.star_drone : ItemTypes.ios_drone;
                ship.Fuel = 2;
                ship.DroneCount = 7;
                foreach (var blocked in new[] { ItemTypes.d__f__c__c, ship.FuelType, drones })
                {
                    stores[ItemTypes.d__f__c__c] = 49999;
                    stores[ship.FuelType] = 49980;
                    stores[drones] = 49993;
                    stores[blocked]++;
                    var before = stores.Items.ToDictionary(p => p.Key, p => p.Value);
                    PressEquipmentNamed(bay, GameCore.SingletonInstance.GameData.GetItem(ItemTypes.derrick).ShortName);
                    Equal(true, ship.DFCC, "rejected replacement retains conversion");
                    Equal(2, ship.Fuel, "rejected replacement retains tank");
                    Equal(7, ship.DroneCount, "rejected replacement retains fleet");
                    Equal(ItemTypes.d__f__c__c, ship.Modules[0].ItemStored, "rejected replacement retains controller");
                    foreach (var pair in before) Equal(pair.Value, stores[pair.Key], "rejected replacement preserves " + pair.Key);
                    Equal(true, OverlayManager.Instance.IsOpen, "insufficient capacity explained");
                    OverlayManager.Instance.CloseOverlay();
                    await InputFrames();
                }
                stores[ItemTypes.d__f__c__c] = 49999;
                stores[ship.FuelType] = 49980;
                stores[drones] = 49993;
                PressEquipmentNamed(bay, GameCore.SingletonInstance.GameData.GetItem(ItemTypes.d__f__c__c).ShortName);
                Equal(false, ship.DFCC, "exact combined capacity permits removal");
                foreach (var returned in new[] { ItemTypes.d__f__c__c, ship.FuelType, drones })
                    Equal(50000, stores[returned], "exact return capacity for " + returned);
                Cursor.Unlock();
            }
        }

        private async Task DfccRemainingControllers()
        {
            var bay = await FittedDfccBay(Ship_Types.SCG);
            var ship = (InterStellarShip)bay.Ship;
            var stores = bay.ResourceList.Stores;
            ship.Modules[5].ModuleType = Module_Types.Tool;
            ship.Modules[5].ItemStored = ItemTypes.d__f__c__c;
            ship.Modules[5].ItemCount = 1;
            ship.Fuel = 2;
            ship.DroneCount = 7;
            stores[ship.FuelType] = 50000;
            stores[ItemTypes.star_drone] = 50000;
            PressEquipmentNamed(bay, GameCore.SingletonInstance.GameData.GetItem(ItemTypes.d__f__c__c).ShortName);
            Equal(true, ship.DFCC, "sixth controller keeps conversion active");
            Equal(2, ship.Fuel, "remaining controller keeps tank loaded");
            Equal(7, ship.DroneCount, "remaining controller keeps fleet aboard");
            Equal(ItemTypes.none, ship.Modules[0].ItemStored, "only requested controller removed");
            Equal(3, stores[ItemTypes.d__f__c__c], "one controller returned");
            // Legacy saves can contain a converted hull without a controller module.
            ship.Modules[5] = new ShipModule();
            PressEquipmentNamed(bay, GameCore.SingletonInstance.GameData.GetItem(ItemTypes.derrick).ShortName);
            Equal(true, ship.DFCC, "unrelated legacy fitting does not infer a conversion change");
            Equal(2, ship.Fuel, "unrelated legacy fitting does not touch fuel");
            Equal(7, ship.DroneCount, "unrelated legacy fitting does not touch fleet");
            Cursor.Unlock();
        }
    }
}
