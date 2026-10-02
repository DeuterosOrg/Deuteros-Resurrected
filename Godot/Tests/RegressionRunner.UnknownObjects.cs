using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Objects.ModuleTextFrame;
using Deuteros.Code.Platform.Helpers;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Utility;
using Godot;
using static Deuteros.Code.Enums;
using GrapplePanel = Deuteros.Code.Platform.Screens.ModuleScenes.Grapple;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        public async Task RunUnknownObjectRegressions()
        {
            foreach (var type in new[] { UnknownItemTypes.AlienArtifact, UnknownItemTypes.CommsPod, UnknownItemTypes.Blazer })
                await CheckAsync($"Captured {type} unloads once into its research programme", () => UnknownObjectResearch(type));
            foreach (var type in new[] { UnknownItemTypes.CommsPod, UnknownItemTypes.Blazer })
                CheckUi($"Capturing {type} preserves the system's unrelated artifact", () => GiftCapturePreservesArtifact(type));
            CheckUi("An occupied grapple preserves both its cargo and the scanned object", OccupiedGrapplePreservesCargo);
            CheckUi("Two ships cannot capture the same orbital artifact", ArtifactCapturedOnce);
            foreach (var crew in new[] { "missing", "empty", "unqualified" })
                CheckUi($"Grapple rejects {crew} crew without consuming the object", () => GrappleRequiresCaptain(crew));
            CheckUi("A grapple accepts 250 tonnes and consumes that asteroid scan", () => AsteroidCaptureLimit(250));
            CheckUi("A grapple rejects a 251 tonne asteroid without erasing an artifact", () => AsteroidCaptureLimit(251));
            foreach (var spare in new[] { false, true })
                await CheckAsync($"Methanoid comms gift preserves occupied grapple with spare={spare}", () => CommsGiftPreservesCargo(spare));
            await CheckAsync("Researching the recovered fusion laser unlocks the drone programme once", FusionLaserResearchFollowup);
            await CheckAsync("A Methanoid comms gift can be analysed researched built fitted and traded without unlock cheats", CommsGiftToTrade);
            CheckUi("Repeated comms discoveries record the unlock once", RepeatedCommsDiscovery);
            await CheckAsync("Research opens and selects a project without orphan controls", ResearchControlLifetime);
            await CheckAsync("War warning grants a fusion prototype that normal analysis and research turn into drone technology", FusionGiftToDroneResearch);
        }

        private async Task ResearchControlLifetime()
        {
            InitializeUi();
            var orphanCount = Performance.GetMonitor(Performance.Monitor.ObjectOrphanNodeCount);
            var panel = OpenUi<Research>("res://Screens/Earth/Research.tscn", new List<SceneVariables> { SceneVariables.Ground });
            try
            {
                Equal(orphanCount, Performance.GetMonitor(Performance.Monitor.ObjectOrphanNodeCount), "opening research creates no detached placeholder button");
                var button = panel.Buttons.First(b => b.ObjectData != null && !b.ObjectData.Locked);
                button.EmitSignal(BaseButton.SignalName.Pressed);
                Equal(button, panel.SelectedButton, "first selection uses the visible research control");
            }
            finally { panel.Free(); }
            Equal(orphanCount, Performance.GetMonitor(Performance.Monitor.ObjectOrphanNodeCount), "closing research leaves no orphan controls");
            await DrainStoppedAudio();
        }

        private IOS UnknownObjectShip()
        {
            var ship = NavigationShip();
            ship.ShipState = Ship_States.UnDocked;
            ship.Pilot = new Staff { Type = StaffType.Marines, Leader = "Salvage", Count = 10 };
            ship.Pilot.AddAction(10);
            foreach (var module in ship.Modules)
            {
                module.ModuleType = Module_Types.Tool;
                module.ItemStored = ItemTypes.grapple;
                module.ItemCount = 1;
            }
            Save.CurrentPlanet = StellarBodies.earth;
            Save.BaseGameData.Stars[StellarBodies.the_sun].ArtifactLocation = StellarBodies.earth;
            return ship;
        }

        private void CaptureScannedObject(IOS ship, int module = 0)
        {
            var grapple = OpenUi<GrapplePanel>("res://PreFabs/ShipModuleWindows/Grapple.tscn");
            try
            {
                grapple.Load(ship, ship.Modules[module]);
                Press(grapple, "Enabled/Buttons/Grab");
            }
            finally { grapple.Free(); }
        }

        private async Task UnknownObjectResearch(UnknownItemTypes type)
        {
            InitializeUi();
            var ship = UnknownObjectShip();
            var item = type == UnknownItemTypes.AlienArtifact ? ItemTypes.alien_artifact
                : type == UnknownItemTypes.CommsPod ? ItemTypes.commspod : ItemTypes.m__f__l;
            var research = GameCore.SingletonInstance.GameData.GetItem(item).Research;
            var initialLimit = research.ResearchLimit;
            Equal(true, research.Locked, "research starts unavailable");
            var found = type == UnknownItemTypes.AlienArtifact ? UnknownItem.ScanForItems(ship) : new UnknownItem(type);
            ship.ItemScanResults = found;
            CaptureScannedObject(ship);
            Equal(found, ship.Modules[0].HeldItem, "captured object retained");
            Equal<GrappleItem>(null, ship.ItemScanResults, "scan consumed");
            Equal(true, research.Locked, "capture alone does not analyse the object");
            if (type == UnknownItemTypes.AlienArtifact)
            {
                Equal(StellarBodies.none, Save.BaseGameData.Stars[StellarBodies.the_sun].ArtifactLocation, "artifact removed from orbit once");
                Equal<UnknownItem>(null, UnknownItem.ScanForItems(ship), "captured artifact cannot be scanned again");
            }

            ship.ShipState = Ship_States.Docked;
            var tweens = GetTree().GetProcessedTweens().ToHashSet();
            var bay = OpenEquippedBay(ship);
            var window = bay.GetNode<DynamicWindow>("GrappleWindow/GrappleEmptier");
            try
            {
                Press(bay, "Buttons/ShipNav/Nav_Torso1");
                Press(bay, "ShipContainer/ScrollContainer2/HBoxContainer/Torso1/SpriteHolder/Buttons/ActivatePod");
                Equal(true, window.IsVisibleInTree(), "analysis window opens");
                await ToSignal(GetTree().CreateTimer(5.1), SceneTreeTimer.SignalName.Timeout);
                Equal<GrappleItem>(null, ship.Modules[0].HeldItem, "object removed for analysis");
                Equal(false, research.Locked, "correct research programme unlocked");
                Equal(false, GameCore.SingletonInstance.GetNode<InputBlocker>("InputBlocker").Blocked, "analysis releases input");
                if (type == UnknownItemTypes.AlienArtifact)
                    Equal(initialLimit + 11, research.ResearchLimit, "exactly one artifact's research credit");
                if (type == UnknownItemTypes.CommsPod)
                    Equal(1, Save.Unlocks.Count(x => x == Game_Unlocks.CommsPod), "comms unlock recorded once");
                if (type == UnknownItemTypes.Blazer)
                    Equal(true, GameCore.SingletonInstance.GameData.GetItem(ItemTypes.pulse_blaster_laser).Research.Locked, "distinct pulse weapon remains locked");
                // A delayed duplicate close must not repeat the research reward or crash.
                GameCore.LockScreen();
                window.Closed(window.DataObject);
                Equal(true, GameCore.SingletonInstance.GetNode<InputBlocker>("InputBlocker").Blocked, "duplicate close cannot release a newer input lock");
                GameCore.UnLockScreen();
                if (type == UnknownItemTypes.AlienArtifact)
                    Equal(initialLimit + 11, research.ResearchLimit, "repeat close cannot duplicate artifact credit");
            }
            finally
            {
                GameCore.UnLockScreen();
                CloseDismantleBay(bay, tweens);
            }
        }

        private void GiftCapturePreservesArtifact(UnknownItemTypes type)
        {
            var ship = UnknownObjectShip();
            Save.BaseGameData.Stars[StellarBodies.the_sun].ArtifactLocation = StellarBodies.the_moon;
            var gift = new UnknownItem(type);
            ship.ItemScanResults = gift;
            CaptureScannedObject(ship);
            Equal(gift, ship.Modules[0].HeldItem, "gift captured");
            Equal(StellarBodies.the_moon, Save.BaseGameData.Stars[StellarBodies.the_sun].ArtifactLocation, "unrelated artifact remains in orbit");
        }

        private void OccupiedGrapplePreservesCargo()
        {
            var ship = UnknownObjectShip();
            var held = new UnknownItem(UnknownItemTypes.CommsPod);
            var scanned = UnknownItem.ScanForItems(ship);
            ship.Modules[0].HeldItem = held;
            ship.ItemScanResults = scanned;
            CaptureScannedObject(ship);
            Equal(held, ship.Modules[0].HeldItem, "existing cargo preserved");
            Equal(scanned, ship.ItemScanResults, "uncollected scan preserved");
            Equal(StellarBodies.earth, Save.BaseGameData.Stars[StellarBodies.the_sun].ArtifactLocation, "uncollected artifact remains");
        }

        private void AsteroidCaptureLimit(int mass)
        {
            var ship = UnknownObjectShip();
            var asteroid = new Asteroid { GrappleItemType = GrappleItemTypes.Asteroid, Type = ItemTypes.iron, Mass = mass, MassName = "Small" };
            ship.ItemScanResults = asteroid;
            CaptureScannedObject(ship);
            Equal<GrappleItem>(mass <= 250 ? asteroid : null, ship.Modules[0].HeldItem, "grapple capacity respected");
            Equal<GrappleItem>(mass <= 250 ? null : asteroid, ship.ItemScanResults, "only captured scan consumed");
            Equal(StellarBodies.earth, Save.BaseGameData.Stars[StellarBodies.the_sun].ArtifactLocation, "asteroid does not remove alien artifact");
        }

        private void ArtifactCapturedOnce()
        {
            var first = UnknownObjectShip();
            var second = UnknownObjectShip();
            first.ItemScanResults = UnknownItem.ScanForItems(first);
            second.ItemScanResults = UnknownItem.ScanForItems(second);
            CaptureScannedObject(first);
            CaptureScannedObject(second);
            Equal(true, first.Modules[0].HeldItem is UnknownItem, "first ship captures the artifact");
            Equal<GrappleItem>(null, second.Modules[0].HeldItem, "stale scan cannot duplicate the artifact");
        }

        private void GrappleRequiresCaptain(string crew)
        {
            var ship = UnknownObjectShip();
            if (crew == "missing") ship.Pilot = null;
            else if (crew == "empty") ship.Pilot.Count = 0;
            else ship.Pilot = new Staff { Type = StaffType.Marines, Count = 10 };
            var found = UnknownItem.ScanForItems(ship);
            ship.ItemScanResults = found;
            var grapple = OpenUi<GrapplePanel>("res://PreFabs/ShipModuleWindows/Grapple.tscn");
            try
            {
                grapple.Load(ship, ship.Modules[0]);
                var disabled = grapple.GetNode<Control>("Disabled").Visible;
                // Even a queued activation must recheck current crew capability.
                Press(grapple, "Enabled/Buttons/Grab");
                Equal<GrappleItem>(null, ship.Modules[0].HeldItem, "unqualified capture rejected");
                Equal(found, ship.ItemScanResults, "object remains available");
                Equal(true, disabled, "grapple controls unavailable");
            }
            finally { grapple.Free(); }
        }

        private async Task CommsGiftPreservesCargo(bool spare)
        {
            InitializeUi();
            var ship = UnknownObjectShip();
            ship.ShipState = Ship_States.Docked;
            ship.Engine = true;
            Save.Ships.Clear();
            Save.Ships.Add(ship);
            GameCore.SingletonInstance.ShipSelected = ship.ShipID;
            GameCore.Earth.ActiveMethanoid = true;
            GameCore.Earth.Station.Built = true;
            var cargo = new UnknownItem(UnknownItemTypes.AlienArtifact);
            ship.Modules[0].HeldItem = cargo;
            ship.Modules[2].ItemStored = ItemTypes.none;
            if (!spare) ship.Modules[1].HeldItem = new Asteroid { GrappleItemType = GrappleItemTypes.Asteroid, Type = ItemTypes.iron, Mass = 100, MassName = "Small" };
            var otherCargo = ship.Modules[1].HeldItem;
            var messages = new[] { ModuleFrameText.Methanoid_Intro_With_Grapple, ModuleFrameText.Methanoid_Intro }
                .Select(key => Save.BaseGameData.ModuleFrameTexts[key]).ToArray();
            var originalLines = messages.Select(frame => frame.Lines).ToArray();
            try
            {
                foreach (var frame in messages) frame.Lines = new List<Line>();
                GameCore.SingletonInstance.ShipSelected = ship.ShipID;
                GameCore.SingletonInstance.ChangeScene(Scenes.ShipInterior, new List<SceneVariables>());
                await InputFrames();
                Press(ActiveScreen<ShipInterior>(), "Modules/00");
                await InputFrames();
                Equal(cargo, ship.Modules[0].HeldItem, "gift never replaces carried artifact");
                if (spare)
                    Equal(UnknownItemTypes.CommsPod, ((UnknownItem)ship.Modules[1].HeldItem).ItemType, "gift uses empty grapple");
                else
                {
                    Equal(otherCargo, ship.Modules[1].HeldItem, "all cargo retained when no space");
                    Equal(false, ship.Modules.Any(m => m.HeldItem is UnknownItem u && u.ItemType == UnknownItemTypes.CommsPod), "gift waits for room");
                }
            }
            finally
            {
                for (int i = 0; i < messages.Length; i++) messages[i].Lines = originalLines[i];
                GameCore.Earth.ActiveMethanoid = false;
            }
        }

        private async Task FusionLaserResearchFollowup()
        {
            InitializeUi();
            var core = GameCore.SingletonInstance;
            var research = core.GameData.GetItem(ItemTypes.m__f__l).Research;
            research.Researched = true;
            var text = Save.BaseGameData.BulletinTexts[BulletinTypes.Methanoid_Laser];
            var originalText = text.BulletinText;
            try
            {
                text.BulletinText = "Recovered fusion laser analysed.";
                core.TriggerResearchFinished(research);
                var bulletin = ActiveScreen<Bulletins>();
                await FinishBulletin(bulletin);
                Equal(false, core.GameData.GetItem(ItemTypes.d__f__c__c).Research.Locked, "fusion laser research unlocks drone computer");
                Equal(false, core.GameData.GetItem(ItemTypes.ios_drone).Research.Locked, "fusion laser research unlocks drones");
                core.TriggerResearchFinished(research);
                Equal(1, Save.Unlocks.Count(x => x == Game_Unlocks.D_F_C_C), "repeat research event does not duplicate unlock");
                Equal(bulletin, ActiveScreen<Bulletins>(), "repeat event does not reopen bulletin");
                Equal(false, core.GetNode<InputBlocker>("InputBlocker").Blocked, "repeat event does not restart typing");
            }
            finally { text.BulletinText = originalText; }
        }

        private void RepeatedCommsDiscovery()
        {
            GameCore.SingletonInstance.TriggerAlienTechDiscovery(ItemTypes.commspod);
            GameCore.SingletonInstance.TriggerAlienTechDiscovery(ItemTypes.commspod);
            Equal(1, Save.Unlocks.Count(x => x == Game_Unlocks.CommsPod), "one persistent comms unlock");
            Equal(false, GameCore.SingletonInstance.GameData.GetItem(ItemTypes.commspod).Research.Locked, "comms research available");
        }

        private async Task CommsGiftToTrade()
        {
            // Real Methanoid interaction grants into the spare grapple without replacing its neighbour.
            await CommsGiftPreservesCargo(true);
            GD.Print("COMMS PHASE: gift acquired");
            var ship = (IOS)Save.Ships.Single();
            // The starting ship already carries manufactured grapples; record that prerequisite.
            // Comms discovery, research and production below still use the normal game path.
            GameCore.SingletonInstance.GameData.GetItem(ItemTypes.grapple).Research.Researched = true;
            var artifact = ship.Modules[0].HeldItem;
            var comms = GameCore.SingletonInstance.GameData.GetItem(ItemTypes.commspod);
            Equal(true, comms.Locked && comms.Research.Locked, "gift has not bypassed research");
            ship.ShipState = Ship_States.Docked;
            var tweens = GetTree().GetProcessedTweens().ToHashSet();
            var bay = OpenEquippedBay(ship);
            try
            {
                Press(bay, "Buttons/ShipNav/Nav_Torso2");
                Press(bay, "ShipContainer/ScrollContainer2/HBoxContainer/Torso2/SpriteHolder/Buttons/ActivatePod");
                await ToSignal(GetTree().CreateTimer(5.1), SceneTreeTimer.SignalName.Timeout);
                Equal<GrappleItem>(null, ship.Modules[1].HeldItem, "physical gift removed for analysis");
                Equal(false, comms.Research.Locked, "analysis exposes research");
                Equal(true, comms.Locked, "analysis does not permit immediate production");
            }
            finally { CloseDismantleBay(bay, tweens); }
            GD.Print("COMMS PHASE: gift analysed");

            GameCore.Earth.ResearchStaff = new Staff { Type = StaffType.Research, Leader = "Science", Count = 200 };
            GameCore.Earth.ResearchStaff.AddAction(9);
            var researchScene = GD.Load<PackedScene>("res://Screens/Earth/Research.tscn");
            GD.Print("COMMS PHASE: research scene loaded");
            var researchPanel = researchScene.Instantiate<Research>();
            GD.Print("COMMS PHASE: research scene instantiated");
            researchPanel.SceneVariables = new List<SceneVariables> { SceneVariables.Ground };
            AddChild(researchPanel);
            GD.Print("COMMS PHASE: research screen opened");
            try
            {
                researchPanel.Buttons.Single(button => button.ObjectData?.ItemType == ItemTypes.commspod)
                    .EmitSignal(BaseButton.SignalName.Pressed);
                Equal(comms.Research, GameCore.Earth.CurrentResearchItem, "real research control selects recovered technology");
                GD.Print("COMMS PHASE: research selected");
                for (uint day = 1; day <= 100 && !comms.Research.Researched; day++)
                    Research.UpdateResearch(day - 1, day);
                Equal(100, comms.Research.ResearchPercentageComplete, "normal research reaches completion");
                Equal(false, comms.Locked, "completed research unlocks manufacturing");
            }
            finally { researchPanel.Free(); }
            GD.Print("COMMS PHASE: research complete");

            var factory = GameCore.Earth.Station.Factory;
            factory.Builder = new Staff { Type = StaffType.Production, Leader = "Factory", Count = 200 };
            factory.Builder.AddAction(12);
            var stores = GameCore.Earth.Station.Resources.Stores;
            stores.Items.Clear();
            stores[ItemTypes.aluminium] = 2;
            stores[ItemTypes.carbon] = stores[ItemTypes.copper] = stores[ItemTypes.gold] = 1;
            var production = GD.Load<PackedScene>("res://Screens/Production.tscn").Instantiate<Production>();
            production.SceneVariables = new List<SceneVariables> { SceneVariables.Orbit };
            production.GetNode("SoundController").Free();
            AddChild(production);
            GD.Print("COMMS PHASE: production screen opened");
            try
            {
                production.Buttons.Single(button => button.ObjectData?.ItemType == ItemTypes.commspod)
                    .EmitSignal(BaseButton.SignalName.Pressed);
                Equal(ItemTypes.commspod, factory.CurrentProductionItem()?.Product.ItemType ?? ItemTypes.none, "real production control starts comms pod");
                foreach (var mineral in new[] { ItemTypes.aluminium, ItemTypes.carbon, ItemTypes.copper, ItemTypes.gold })
                    Equal(0, stores[mineral], "exact recipe material charged: " + mineral);
                for (uint day = 1; day <= 100 && stores[ItemTypes.commspod] == 0; day++)
                    Production.UpdateProduction(day - 1, day);
                Equal(1, stores[ItemTypes.commspod], "one manufactured pod in orbital stores");
            }
            finally { production.Free(); }
            GD.Print("COMMS PHASE: production complete");

            tweens = GetTree().GetProcessedTweens().ToHashSet();
            bay = OpenEquippedBay(ship);
            try
            {
                Press(bay, "Buttons/ShipNav/Nav_Torso2");
                Press(bay, "ShipContainer/ScrollContainer2/HBoxContainer/Torso2/SpriteHolder/Buttons/ActivatePod");
                PressEquipmentNamed(bay, comms.ShortName);
                Equal(ItemTypes.commspod, ship.Modules[1].ItemStored, "manufactured pod fitted through equipment control");
                Equal(0, stores[ItemTypes.commspod], "fitting consumes stock");
            }
            finally
            {
                GetTree().CurrentScene.GetNode<GlobalInput>("VirtualCursorView").Unlock();
                CloseDismantleBay(bay, tweens);
            }

            ship.Modules[2].ModuleType = Module_Types.Supply;
            GD.Print("COMMS PHASE: pod fitted");
            ship.Modules[2].ItemStored = ItemTypes.iron;
            ship.Modules[2].ItemCount = 123;
            GameCore.Earth.ActiveMethanoid = true;
            var messages = new[] { ModuleFrameText.Methanoid_TradeQuestion, ModuleFrameText.Methanoid_Trade1 }
                .Select(key => Save.BaseGameData.ModuleFrameTexts[key]).ToArray();
            var originalLines = messages.Select(frame => frame.Lines).ToArray();
            try
            {
                foreach (var frame in messages) frame.Lines = new List<Line>();
                GameCore.SingletonInstance.ShipSelected = ship.ShipID;
                GameCore.SingletonInstance.ChangeScene(Scenes.ShipInterior, new List<SceneVariables>());
                await InputFrames();
                Press(ActiveScreen<ShipInterior>(), "Modules/01");
                GD.Print("COMMS PHASE: trade activated");
                await InputFrames();
                Equal(1, Save.MethanoidTradeCount, "first real trade interaction recorded");
                Equal(ItemTypes.silica, ship.Modules[2].ItemStored, "trade exchanges offered iron");
                Equal(123, ship.Modules[2].ItemCount, "trade preserves cargo quantity");
                Equal(ItemTypes.commspod, ship.Modules[1].ItemStored, "comms equipment remains fitted");
                Equal(artifact, ship.Modules[0].HeldItem, "unrelated artifact preserved throughout full flow");
                Equal(false, GameCore.SingletonInstance.InfiniteResources, "no infinite resource mode used");
            }
            finally
            {
                for (int i = 0; i < messages.Length; i++) messages[i].Lines = originalLines[i];
                GameCore.Earth.ActiveMethanoid = false;
            }
        }

        private async Task FusionGiftToDroneResearch()
        {
            InitializeUi();
            var core = GameCore.SingletonInstance;
            var ship = UnknownObjectShip();
            ship.ShipState = Ship_States.Docked;
            ship.Engine = true;
            ship.Modules[1].ItemStored = ItemTypes.commspod;
            var cargo = new UnknownItem(UnknownItemTypes.AlienArtifact);
            ship.Modules[0].HeldItem = cargo;
            Save.Ships.Clear();
            Save.Ships.Add(ship);
            // Start at the existing war threshold; travel and the prior trades are fixture setup.
            Save.MethanoidTradeCount = 16;
            Save.CurrentDay = 100;
            GameCore.Earth.ActiveMethanoid = true;
            GameCore.Earth.Station.Built = true;
            var warning = Save.BaseGameData.ModuleFrameTexts[ModuleFrameText.Methanoid_War_Warning];
            var warningLines = warning.Lines;
            var bulletin = Save.BaseGameData.BulletinTexts[BulletinTypes.Methanoid_Laser];
            var bulletinText = bulletin.BulletinText;
            try
            {
                warning.Lines = new List<Line>();
                bulletin.BulletinText = "Fusion laser analysed.";
                core.ShipSelected = ship.ShipID;
                core.ChangeScene(Scenes.ShipInterior, new List<SceneVariables>());
                await InputFrames();
                Press(ActiveScreen<ShipInterior>(), "Modules/01");
                await InputFrames();
                Equal(true, Save.AtWar, "real interaction declares war");
                Equal(Save.CurrentDay, Save.WarDeclaredDay, "war declaration records current day");
                Equal(UnknownItemTypes.Blazer, ((UnknownItem)ship.Modules[1].HeldItem).ItemType, "war warning supplies the physical prototype");
                Equal(cargo, ship.Modules[0].HeldItem, "war gift preserves neighbouring artifact");
                var research = core.GameData.GetItem(ItemTypes.m__f__l).Research;
                Equal(true, research.Locked, "gift awaits analysis");
                GameCore.Earth.ActiveMethanoid = false;
                // Stage the return to a friendly dock after the trade interaction's takeoff.
                ship.ShipState = Ship_States.Docked;
                var tweens = GetTree().GetProcessedTweens().ToHashSet();
                var bay = OpenEquippedBay(ship);
                try
                {
                    Press(bay, "Buttons/ShipNav/Nav_Torso2");
                    Press(bay, "ShipContainer/ScrollContainer2/HBoxContainer/Torso2/SpriteHolder/Buttons/ActivatePod");
                    await ToSignal(GetTree().CreateTimer(5.1), SceneTreeTimer.SignalName.Timeout);
                    Equal<GrappleItem>(null, ship.Modules[1].HeldItem, "prototype physically unloaded");
                    Equal(false, research.Locked, "analysis exposes fusion research");
                }
                finally { CloseDismantleBay(bay, tweens); }

                GameCore.Earth.ResearchStaff = new Staff { Type = StaffType.Research, Leader = "Science", Count = 200 };
                GameCore.Earth.ResearchStaff.AddAction(9);
                var panel = OpenUi<Research>("res://Screens/Earth/Research.tscn", new List<SceneVariables> { SceneVariables.Ground });
                try
                {
                    panel.Buttons.Single(b => b.ObjectData?.ItemType == ItemTypes.m__f__l).EmitSignal(BaseButton.SignalName.Pressed);
                    for (uint day = 1; day <= 100 && !research.Researched; day++)
                        Research.UpdateResearch(day - 1, day);
                    Equal(true, research.Researched, "normal research completes the recovered prototype");
                }
                finally { panel.Free(); }
                await FinishBulletin(ActiveScreen<Bulletins>());
                Equal(false, core.GameData.GetItem(ItemTypes.d__f__c__c).Research.Locked, "drone computer unlocked by completed research");
                Equal(false, core.GameData.GetItem(ItemTypes.ios_drone).Research.Locked, "drone research available");
                Equal(1, Save.Unlocks.Count(x => x == Game_Unlocks.D_F_C_C), "drone programme unlocked once");
            }
            finally
            {
                warning.Lines = warningLines;
                bulletin.BulletinText = bulletinText;
                GameCore.Earth.ActiveMethanoid = false;
            }
        }
    }
}
