using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Utility;
using Godot;
using Newtonsoft.Json.Linq;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task RunEngineDamageRegressions()
        {
            foreach (var scg in new[] { false, true })
            {
                CheckUi($"Hostile arrival permits escape with engine damage scg={scg}", () => EngineDamageEscape(scg, false));
                CheckUi($"Fleet attack permits escape with engine damage scg={scg}", () => EngineDamageEscape(scg, true));
            }
            CheckUi("Engine escape roll preserves both outcomes and rejects repeated departure", EngineDamageRollOutcomes);
            CheckUi("Engine damage excludes protected peaceful enemy and invalid departures", EngineDamageExclusions);
            foreach (var type in new[] { Ship_Types.Shuttle, Ship_Types.IOS, Ship_Types.SCG })
            {
                await CheckAsync($"Replace damaged {type} drive consumes one local spare", () => EngineDamageReplacement(type));
                await CheckAsync($"Dismantle damaged {type} drive never returns a usable spare", () => EngineDamageDismantle(type));
            }
            foreach (var scg in new[] { false, true })
                CheckUi($"Damaged journey persists and arrives at doubled duration scg={scg}", () => EngineDamageJourney(scg));
            CheckUi("Older saves without damage state load healthy", EngineDamageLegacySave);
            await CheckAsync("Interior warns of damage while keeping escape controls available", EngineDamageWarning);
            CheckUi("Damaged return-course preview starts from the current day after arrival", EngineDamageReturnPreview);
        }

        private static bool Damaged(Ship ship) => ship.EngineDamaged;
        private static void SetDamaged(Ship ship, bool value) => ship.EngineDamaged = value;
        private static bool DepartWithRoll(Ship ship, Func<bool> roll) => ship.EngageEngine(roll);

        private InterStellarShip DamageShip(bool scg)
        {
            var ship = (InterStellarShip)HullShip(scg);
            ship.DestinationPlanetLocation = StellarBodies.mars;
            ship.DestinationStarLocation = StellarBodies.the_sun;
            ship.ACC = null;
            ship.FuelType = ItemTypes.meh_fuel;
            Save.AtWar = true;
            GameCore.Earth.ActiveMethanoid = false;
            Save.BaseGameData.Planets[StellarBodies.mars].ActiveMethanoid = false;
            return ship;
        }

        private void EngineDamageEscape(bool scg, bool attacked)
        {
            var ship = DamageShip(scg);
            if (attacked)
            {
                var fleet = new EnemyFleet { ShipType = Ship_Types.IOS, MethanoidOwned = true,
                    PlanetLocation = StellarBodies.mars, DestinationPlanetLocation = StellarBodies.earth, AttackDay = 1 };
                Save.Ships.Add(fleet);
                fleet.ProcessFleet();
                Equal(true, GameCore.SingletonInstance.GameData.PlanetUnderAttack(ship.PlanetLocation), "actual fleet arrival threatens orbit");
            }
            else
            {
                Save.BaseGameData.Planets[StellarBodies.mars].ActiveMethanoid = true;
                Equal(true, ship.EngageEngine(), "safe outbound departure");
                while (ship.ShipState == Ship_States.InTransit)
                {
                    var before = Save.CurrentDay++;
                    ShipInterior.UpdateShips(before, Save.CurrentDay);
                    if (Save.CurrentDay > 150) throw new InvalidOperationException("Outbound arrival exceeded route bound");
                }
                Equal(StellarBodies.mars, ship.PlanetLocation, "arrives at occupied destination");
            }
            Equal(false, Damaged(ship), "danger entry alone does not roll escape damage");
            ship.StartTravelDay = 0;
            var duration = ship.TravelTimeRemain();
            var calls = 0;
            Equal(true, DepartWithRoll(ship, () => { calls++; return true; }), "escape remains possible");
            Equal(1, calls, "one roll on eligible escape");
            Equal(true, Damaged(ship), "escape hit damages engine");
            Equal(true, ship.Engine, "damaged drive remains fitted");
            Equal(duration * 2, ship.TravelTimeRemain(), "damaged escape takes twice the route duration");
            var beforeEscape = Save.CurrentDay++;
            ShipInterior.UpdateShips(beforeEscape, Save.CurrentDay);
            Equal(true, Save.Ships.Contains(ship), "departed ship survives hostile countdown reset");
        }

        private void EngineDamageRollOutcomes()
        {
            var ship = DamageShip(false);
            GameCore.Earth.ActiveMethanoid = true;
            var calls = 0;
            Equal(true, DepartWithRoll(ship, () => { calls++; return false; }), "missed hit still departs");
            Equal(false, Damaged(ship), "miss preserves healthy engine");
            Equal(false, DepartWithRoll(ship, () => { calls++; return true; }), "duplicate departure is rejected");
            Equal(1, calls, "duplicate callback cannot reroll");
            ship.ShipState = Ship_States.UnDocked;
            SetDamaged(ship, true);
            Equal(true, DepartWithRoll(ship, () => throw new InvalidOperationException("Already damaged drive must not reroll")), "damaged ship can leave again");
            Equal(true, Damaged(ship), "damage persists");
        }

        private void EngineDamageExclusions()
        {
            foreach (var reason in new[] { "dfcc", "peace", "enemy", "safe", "fuel", "engine", "course", "docked" })
            {
                var ship = DamageShip(false);
                GameCore.Earth.ActiveMethanoid = true;
                if (reason == "dfcc") ship.DFCC = true;
                if (reason == "peace") Save.AtWar = false;
                if (reason == "enemy") ship.MethanoidOwned = true;
                if (reason == "safe") GameCore.Earth.ActiveMethanoid = false;
                if (reason == "fuel") ship.Fuel = 0;
                if (reason == "engine") ship.Engine = false;
                if (reason == "course") ship.DestinationPlanetLocation = StellarBodies.atlantic;
                if (reason == "docked") ship.ShipState = Ship_States.Docked;
                var valid = reason == "dfcc" || reason == "peace" || reason == "enemy" || reason == "safe";
                Equal(valid, DepartWithRoll(ship, () => throw new InvalidOperationException("Ineligible roll: " + reason)), reason + " departure");
                Equal(false, Damaged(ship), reason + " remains undamaged");
            }
        }

        private async Task<ShipBay> DamagedAssemblyBay(Ship_Types type)
        {
            var bay = await EmptyAssemblyBay(type);
            var stores = bay.ResourceList.Stores;
            stores.Items.Clear();
            stores[AssemblyChassis(type)] = stores[AssemblyDrive(type)] = 1;
            Press(bay, "Buttons/Nav_Create_" + type);
            Press(bay, ShipParts + "Engine/SpriteHolder/Buttons/InstallEngine");
            SetDamaged((Ship)bay.Ship, true);
            // Damage happens away from the bay; enter it with the damaged ship already present.
            GameCore.SingletonInstance.ChangeScene(Scenes.ShipBay, new List<SceneVariables>
            {
                type == Ship_Types.Shuttle ? SceneVariables.Ground : SceneVariables.Orbit,
                type == Ship_Types.Shuttle ? SceneVariables.Shuttle : SceneVariables.Ship
            });
            await InputFrames();
            return ActiveScreen<ShipBay>();
        }

        private async Task EngineDamageReplacement(Ship_Types type)
        {
            var bay = await DamagedAssemblyBay(type);
            var ship = (Ship)bay.Ship;
            var stores = bay.ResourceList.Stores;
            var drive = AssemblyDrive(type);
            var other = ReferenceEquals(stores, GameCore.Earth.PlanetResources.Stores) ? GameCore.Earth.Station.Resources.Stores : GameCore.Earth.PlanetResources.Stores;
            other[drive] = 7;
            var path = ShipParts + "Engine/SpriteHolder/Buttons/InstallEngine";
            Press(bay, path);
            Equal(true, Damaged(ship), "unavailable local spare cannot repair");
            Equal(7, other[drive], "other location not charged");
            stores[drive] = 2;
            ship.EngineEngaged = true;
            var previousTweens = GetTree().GetProcessedTweens().ToHashSet();
            Press(bay, "Buttons/ShipNav/Nav_Engine");
            foreach (var tween in GetTree().GetProcessedTweens().Where(t => !previousTweens.Contains(t)))
                if (tween.IsRunning()) await ToSignal(tween, Tween.SignalName.Finished);
            await InputFrames();
            var point = bay.GetNode<Control>(path).GetGlobalRect().GetCenter();
            GetViewport().PushInput(new InputEventMouseMotion { Position = point, GlobalPosition = point }, true);
            foreach (var pressed in new[] { true, false })
                GetViewport().PushInput(new InputEventMouseButton { Position = point, GlobalPosition = point, ButtonIndex = MouseButton.Left, Pressed = pressed }, true);
            await InputFrames();
            Equal(false, Damaged(ship), "replacement clears damage");
            Equal(false, ship.EngineEngaged, "new drive is disengaged");
            Equal(true, ship.Engine, "new drive fitted");
            Equal(1, stores[drive], "exactly one spare consumed with no damaged salvage credit");
            Press(bay, path);
            Equal(1, stores[drive], "retained duplicate press cannot replace healthy drive");
            var restored = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            Equal(false, Damaged((Ship)restored.Ships.Single(s => s.ShipID == ship.ShipID)), "repair persists");
        }

        private async Task EngineDamageDismantle(Ship_Types type)
        {
            var bay = await DamagedAssemblyBay(type);
            var stores = bay.ResourceList.Stores;
            stores[AssemblyDrive(type)] = 50000;
            stores[AssemblyChassis(type)] = 49999;
            Press(bay, "Buttons/Nav_Dismantle");
            Equal(0, Save.Ships.Count, "damaged drive needs no free drive storage");
            Equal(50000, stores[AssemblyDrive(type)], "damaged drive yields no usable spare");
            Equal(50000, stores[AssemblyChassis(type)], "chassis returned");
        }

        private void EngineDamageJourney(bool scg)
        {
            var ship = DamageShip(scg);
            var normal = ship.TravelTimeRemain();
            SetDamaged(ship, true);
            Equal(normal * 2, ship.TravelTimeRemain(), "preview doubles before departure");
            Equal(true, ship.EngageEngine(), "damaged departure accepted");
            var saved = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            GameCore.SingletonInstance.GameData.ActiveSaveFile = saved;
            ship = (InterStellarShip)saved.Ships.Single(s => s.ShipID == ship.ShipID);
            Equal(true, Damaged(ship), "damage survives load");
            for (var elapsed = 1; elapsed <= normal * 2; elapsed++)
            {
                var before = Save.CurrentDay++;
                ShipInterior.UpdateShips(before, Save.CurrentDay);
                Equal(elapsed == normal * 2 ? Ship_States.UnDocked : Ship_States.InTransit, ship.ShipState, "arrival uses complete doubled journey");
            }
            Equal(StellarBodies.mars, ship.PlanetLocation, "damaged ship reaches destination");
            Equal(true, Damaged(ship), "arrival cannot repair engine");
        }

        private void EngineDamageLegacySave()
        {
            var ship = DamageShip(false);
            var document = JObject.Parse(SaveStorage.Serialize(Save));
            foreach (var legacyVessel in document["Game"]["Ships"].OfType<JObject>()) legacyVessel.Remove("EngineDamaged");
            var loaded = SaveStorage.Deserialize(document.ToString());
            Equal(false, Damaged((Ship)loaded.Ships.Single(s => s.ShipID == ship.ShipID)), "absent legacy field defaults healthy");
            var vessel = (JObject)document["Game"]["Ships"].First;
            vessel["EngineDamaged"] = JValue.CreateNull();
            try { SaveStorage.Deserialize(document.ToString()); throw new InvalidOperationException("Null damage accepted"); }
            catch (Newtonsoft.Json.JsonSerializationException) { }
            vessel.Remove("EngineDamaged");
            vessel.Remove("Engine");
            try { SaveStorage.Deserialize(document.ToString()); throw new InvalidOperationException("Missing original field accepted"); }
            catch (Newtonsoft.Json.JsonSerializationException) { }
        }

        private void EngineDamageReturnPreview()
        {
            var ship = DamageShip(false);
            SetDamaged(ship, true);
            var duration = ship.TravelTimeRemain();
            Equal(true, ship.EngageEngine(), "damaged outward journey starts");
            for (var day = 0; day < duration; day++)
            {
                var before = Save.CurrentDay++;
                ShipInterior.UpdateShips(before, Save.CurrentDay);
            }
            Equal(Ship_States.UnDocked, ship.ShipState, "outward journey complete");
            Equal(duration, ship.TravelTimeRemain(), "return preview is a fresh full duration, not elapsed outward time");
            Save.CurrentDay += 3;
            Equal(duration, ship.TravelTimeRemain(), "waiting in orbit cannot reduce the next journey");
            Equal(true, ship.EngageEngine(), "return journey starts");
            Equal(duration, ship.TravelTimeRemain(), "preview agrees with actual departure");
        }

        private async Task EngineDamageWarning()
        {
            var interior = await OpenInterior(Ship_Types.IOS);
            var ship = (Ship)interior.Ship;
            ship.ShipState = Ship_States.UnDocked;
            ship.Engine = true;
            ship.Fuel = 100;
            ship.DestinationPlanetLocation = StellarBodies.mars;
            SetDamaged(ship, true);
            GameCore.SingletonInstance.ShipSelected = ship.ShipID;
            GameCore.SingletonInstance.ChangeScene(Scenes.ShipInterior, new List<SceneVariables>());
            await InputFrames();
            interior = ActiveScreen<ShipInterior>();
            Equal("Damaged !", interior.GetNode<Label>("TextLayout/EngineStatusValue").Text, "damage warning takes precedence over engagement");
            Equal(true, interior.GetNode<Control>("EngineControls/EngageEngine").Visible, "escape control remains available");
            var point = interior.GetNode<Control>("EngineControls/EngageEngine").GetGlobalRect().GetCenter();
            GetViewport().PushInput(new InputEventMouseMotion { Position = point, GlobalPosition = point }, true);
            foreach (var pressed in new[] { true, false })
                GetViewport().PushInput(new InputEventMouseButton { Position = point, GlobalPosition = point, ButtonIndex = MouseButton.Left, Pressed = pressed }, true);
            await InputFrames();
            Equal(Ship_States.InTransit, ship.ShipState, "actual engine control permits damaged departure");
            await InputFrames();
            Equal("Damaged !", interior.GetNode<Label>("TextLayout/EngineStatusValue").Text, "warning persists in transit");
            await CaptureDisplayEvidence("damaged-engine-in-transit");
        }
        private async Task EngineDriftReadout()
        {
            foreach (var type in new[] { Ship_Types.Shuttle, Ship_Types.IOS, Ship_Types.SCG })
            {
                var interior = await OpenInterior(type, true);
                var ship = interior.Ship;
                ship.Engine = true;
                ship.EngineEngaged = true;
                ship.ShipState = Ship_States.InTransit;
                interior.UpdateState();
                var label = interior.GetNode<Label>("TextLayout/EngineStatusValue");
                Equal("Engaged", label.Text, "powered transit readout: " + type);
                Press(interior, "EngineControls/DisengageEngine");
                Equal(false, ship.EngineEngaged, "real disengage callback: " + type);
                Equal("Disengaged", label.Text, "drift readout: " + type);
                Equal(Save.BaseGameData.Red, label.GetThemeColor("font_color"), "drift warning colour: " + type);
                Equal(true, interior.GetNode<Label>("TextLayout/Status").Text.StartsWith("Drifting To"), "consistent flight status: " + type);
                ship.EngineDamaged = true; interior.UpdateState();
                Equal("Damaged !", label.Text, "damage retains precedence: " + type);
                ship.EngineDamaged = false;
                foreach (var state in new[] { Ship_States.Launching, Ship_States.Docking, Ship_States.TakingOff, Ship_States.Landing })
                {
                    ship.ShipState = state; interior.UpdateState();
                    Equal("Engaged", label.Text, "powered transition retains readout: " + state);
                }
                ship.ShipState = Ship_States.InTransit; interior.UpdateState();
                await CaptureDisplayEvidence("drifting-engine-" + type);
            }
        }
    }
}
