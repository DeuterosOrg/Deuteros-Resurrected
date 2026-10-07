using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Screens;
using Deuteros.Code.Utility;
using Godot;
using static Deuteros.Code.Enums;
using StoreScreen = Deuteros.Code.Platform.Screens.Store;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private async Task RunMtxInstallationRegressions()
        {
            foreach (var automated in new[] { false, true })
                CheckUi($"Paid MTX production installs locally without stock or repeat charges automated={automated}", () => MtxProductionInstalls(automated));
            foreach (var automated in new[] { false, true })
                CheckUi($"Installed MTX cannot be ordered again automated={automated}", () => MtxDuplicateOrder(automated));
            Check("A saved repeat MTX order is cleared if the station already has a module", MtxInstalledQueue);
            Check("An in-progress MTX survives save load and installs once", MtxProductionSave);
            foreach (var automated in new[] { false, true })
                CheckUi($"Ground factory cannot order an orbital MTX automated={automated}", () => MtxGroundOrder(automated));
            foreach (var location in new[] { "ground", "unequipped", "captured" })
                CheckUi($"Global MTX discovery does not enable local controls at {location} stores", () => MtxStoreEligibility(location));
            CheckUi("An installed captured MTX is visible before its discovery bulletin", MtxBeforeDiscovery);
            CheckUi("Open MTX stores hide the controls when their station is lost", MtxStoreLoss);
            await CheckAsync("MTX discovery research production transfer and save use the normal controls", MtxNormalProgression);
            CheckUi("Open Stores gains MTX controls on the day local installation completes", MtxStoreCompletion);
            Check("MTX construction suspends while its station is unavailable", MtxConstructionStationLoss);
            await CheckAsync("MTX artwork fits production and research display bounds", MtxArtworkBounds);
            await CheckAsync("MTX preserves pointer navigation and resource controls", MtxPointerNavigation);
            await CheckAsync("MTX restores scrolling and keeps wheel and arrow controls synchronized", MtxScrollPosition);
        }

        private async Task MtxStarIconTargets()
        {
            InitializeUi();
            async Task Click(Vector2 point)
            {
                PushGameInput(new InputEventMouseMotion { Position = point, GlobalPosition = point });
                foreach (var pressed in new[] { true, false })
                    PushGameInput(new InputEventMouseButton { Position = point, GlobalPosition = point,
                        ButtonIndex = MouseButton.Left, Pressed = pressed });
                await InputFrames();
            }
            PrepareMtxRoute();
            var stars = new[] { StellarBodies.the_sun, StellarBodies.proxima, StellarBodies.centauri,
                StellarBodies.barnard, StellarBodies.lalande, StellarBodies.sirius,
                StellarBodies.cygni, StellarBodies.procyon, StellarBodies.tau_ceti };
            var targets = stars.Select(star => Save.BaseGameData.Planets.Values.First(p =>
                p.ParentStar == star && p.PlanetId != StellarBodies.earth)).ToArray();
            foreach (var target in targets)
            {
                target.ActiveMethanoid = false;
                target.Station.Built = target.Station.MtxInstalled = true;
            }
            var stores = GameCore.Earth.Station.Resources.Stores;
            stores.AlternativeView = true;
            Save.CurrentPlanet = StellarBodies.earth;
            GameCore.SingletonInstance.ChangeScene(Scenes.Store, new List<SceneVariables> { SceneVariables.Orbit });
            var store = ActiveScreen<StoreScreen>();
            try
            {
                await InputFrames();
                var buttons = store.MTX.GetNode<VBoxContainer>("Destination/SystemButtons").GetChildren().OfType<Button>().ToArray();
                for (var i = 0; i < stars.Length; i++)
                {
                    GD.Print($"MTX icon {stars[i]}: button={buttons[i].GetGlobalRect()} minimum={buttons[i].GetCombinedMinimumSize()}");
                    // These positions are the nine 24x16 icons painted into MTX/Background.png.
                    // Using button centres would hide a layout drift away from that artwork.
                    var point = store.MTX.ToGlobal(new Vector2(228, 36 + 16 * i));
                    await Click(point);
                    var destination = store.MTX.GetNode<TextureButton>("Destination/StationButtons/00");
                    var planet = (StellarBodies)(int)destination.GetMeta("StationId");
                    Equal(stars[i], Save.BaseGameData.Planets[planet].ParentStar, $"visible {stars[i]} icon selects its system");
                    if (planet == StellarBodies.earth)
                        destination = store.MTX.GetNode<TextureButton>("Destination/StationButtons/01");
                    await Click(destination.GetGlobalRect().GetCenter());
                    var selected = (StellarBodies)(int)destination.GetMeta("StationId");
                    Equal(selected, stores.MTX.Target, "displayed station becomes the actual route");
                    Equal(stars[i], Save.BaseGameData.Planets[selected].ParentStar, "route belongs to the selected icon");
                }
                var loaded = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                Equal(stores.MTX.Target, loaded.BaseGameData.Planets[StellarBodies.earth].Station.Resources.Stores.MTX.Target,
                    "selected route survives save load");
            }
            finally { GameCore.SingletonInstance.ChangeScene(Scenes.Overview, new List<SceneVariables>()); }
        }

        private async Task MtxScrollPosition() => await WithMenuSound(async (menu, viewport, player) =>
        {
            PrepareMtxRoute();
            foreach (var item in Save.BaseGameData.ItemList) item.Locked = false;
            var stores = GameCore.Earth.Station.Resources.Stores;
            stores.AlternativeView = true;
            stores.MTX.CurrentScroll = 2;
            Save.CurrentPlanet = StellarBodies.earth;
            GameCore.SingletonInstance.ChangeScene(Scenes.Store, new List<SceneVariables> { SceneVariables.Orbit });
            var store = ActiveScreen<StoreScreen>();
            var parent = store.GetParent();
            store.Reparent(viewport);
            try
            {
                await InputFrames();
                var list = store.MTX.GetNode<ScrollContainer>("Config/ResourceList");
                Equal(16, list.ScrollVertical, "opening restores the saved resource row");
                ClickMenu(viewport, store.MTX.GetNode<Button>("Config/Buttons/ScrollDown"));
                await ToSignal(GetTree().CreateTimer(0.4), SceneTreeTimer.SignalName.Timeout);
                Equal(24, list.ScrollVertical, "down arrow advances one row after reopening");
                MenuPointer(viewport, list.GetGlobalRect().GetCenter(), true, MouseButton.WheelDown);
                MenuPointer(viewport, list.GetGlobalRect().GetCenter(), false, MouseButton.WheelDown);
                await InputFrames();
                var wheelPosition = list.ScrollVertical;
                Equal(true, wheelPosition > 24, "mouse wheel moves the resource list");
                Equal((int)System.Math.Ceiling(wheelPosition / 8.0), stores.MTX.CurrentScroll, "wheel position is retained for reopening");
                ClickMenu(viewport, store.MTX.GetNode<Button>("Config/Buttons/ScrollUp"));
                await ToSignal(GetTree().CreateTimer(0.4), SceneTreeTimer.SignalName.Timeout);
                Equal(wheelPosition - 8, list.ScrollVertical, "up arrow follows the visible wheel position");
                ClickMenu(viewport, store.MTX.GetNode<Button>("Config/Buttons/Restore"));
                Equal(wheelPosition - 8, list.ScrollVertical, "restoring transfer settings preserves scrolling");
                var savedRow = stores.MTX.CurrentScroll;
                var savedGame = SaveStorage.Serialize(Save);
                store.Reparent(parent);
                GameCore.SingletonInstance.ChangeScene(Scenes.Overview, new List<SceneVariables>());
                GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(savedGame);
                GameCore.SingletonInstance.ChangeScene(Scenes.Store, new List<SceneVariables> { SceneVariables.Orbit });
                store = ActiveScreen<StoreScreen>();
                parent = store.GetParent();
                store.Reparent(viewport);
                await InputFrames();
                Equal(savedRow * 8, store.MTX.GetNode<ScrollContainer>("Config/ResourceList").ScrollVertical,
                    "save load and reopening restore the wheel-selected row");
            }
            finally
            {
                if (GodotObject.IsInstanceValid(store)) store.Reparent(parent);
            }
        });

        private async Task MtxPointerNavigation() => await WithMenuSound(async (menu, viewport, player) =>
        {
            PrepareMtxRoute();
            Save.CurrentPlanet = StellarBodies.earth;
            GameCore.Earth.Station.Resources.Stores.AlternativeView = true;
            GameCore.SingletonInstance.ChangeScene(Scenes.Store, new List<SceneVariables> { SceneVariables.Orbit });
            var store = ActiveScreen<StoreScreen>();
            var parent = store.GetParent();
            store.Reparent(viewport);
            try
            {
                await InputFrames();
                Equal(true, store.MTX.Visible, "installed transmitter view is open");
                ClickMenu(viewport, menu.TimeButton);
                Equal(true, Save.TimeSkip, "MTX background allows pointer to start time");
                ClickMenu(viewport, menu.TimeButton);
                Equal(false, Save.TimeSkip, "MTX background allows pointer to stop time");
                var row = store.MTX.GetNode<BoxContainer>("Config/ResourceList/ResourceContainer")
                    .GetChildren().OfType<MTXRow>().Single(r => r.ItemType == ItemTypes.iron);
                ClickMenu(viewport, row.RightCross);
                Equal(true, store.CurrentStore.MTX.BalanceItems.Contains(ItemTypes.iron), "resource control still configures balancing");
                ClickMenu(viewport, menu.MasterControlButton);
                Equal(Scenes.Overview, GameCore.SingletonInstance.currentScene, "MTX background allows pointer to Master Control");
            }
            finally
            {
                if (GodotObject.IsInstanceValid(store)) store.Reparent(parent);
            }
        });

        private Item PrepareMtxProduction(bool automated, bool ground = false)
        {
            DisableFuelRefining();
            Save.CurrentPlanet = StellarBodies.earth;
            GameCore.Earth.GroundSelected = ground;
            GameCore.Earth.Station.Built = true;
            GameCore.Earth.Station.BuildParts = 8;
            GameCore.Earth.Station.MtxInstalled = false;
            var item = GameCore.SingletonInstance.GameData.GetItem(ItemTypes.m__t__x);
            item.Locked = item.Research.Locked = false;
            item.Research.Researched = true;
            var factory = ground ? GameCore.Earth.Factory : GameCore.Earth.Station.Factory;
            factory.AOC = automated;
            factory.Builder = new Staff { Type = StaffType.Production, Count = 200, Leader = "Installer" };
            factory.Builder.AddAction(12);
            var stores = ground ? GameCore.Earth.PlanetResources.Stores : GameCore.Earth.Station.Resources.Stores;
            foreach (var recipe in item.BuildRequirements) stores[recipe.ItemType] = recipe.ItemCount * 2;
            return item;
        }

        private Production OpenMtxProduction(bool ground = false)
        {
            var screen = GD.Load<PackedScene>("res://Screens/Production.tscn").Instantiate<Production>();
            screen.SceneVariables = new List<SceneVariables> { ground ? SceneVariables.Ground : SceneVariables.Orbit };
            GameCore.SingletonInstance.UpdateMenuButtons(ground, !ground);
            // Same isolated recipe fixture as earlier tests; native audio acceptance is separate.
            screen.GetNode("SoundController").Free();
            AddChild(screen);
            return screen;
        }

        private void OrderMtx(Production screen) => screen.Buttons.Single(b => b.ObjectData?.ItemType == ItemTypes.m__t__x)
            .EmitSignal(BaseButton.SignalName.Pressed);

        private void MtxProductionInstalls(bool automated)
        {
            var item = PrepareMtxProduction(automated);
            var screen = OpenMtxProduction();
            try
            {
                OrderMtx(screen);
                if (automated) OrderMtx(screen); // A repeat order must stop after this installation.
                ProductionDays(30);
                Equal(true, GameCore.Earth.Station.MtxInstalled, "completion installs at producing station");
                Equal(false, Save.BaseGameData.Planets[StellarBodies.the_moon].Station.MtxInstalled, "other station unaffected");
                Equal(0, GameCore.Earth.Station.Resources.Stores[ItemTypes.m__t__x], "no transferable module stock");
                Equal(0, GameCore.Earth.Station.Factory.ProductionQueue.Count, "completed module leaves no repeat order");
                Equal("Static_Locked",
                    screen.Buttons.Single(b => b.ObjectData?.ItemType == ItemTypes.m__t__x)
                        .GetNode<AnimatedSprite2D>("ButtonSpriteAnimation").Animation.ToString(),
                    "installed item is visibly unavailable for another order");
                foreach (var recipe in item.BuildRequirements)
                    Equal(recipe.ItemCount, GameCore.Earth.Station.Resources.Stores[recipe.ItemType], "exactly one recipe charged");
            }
            finally { screen.Free(); }
        }

        private void MtxDuplicateOrder(bool automated)
        {
            var item = PrepareMtxProduction(automated);
            GameCore.Earth.Station.MtxInstalled = true;
            var screen = OpenMtxProduction();
            try
            {
                OrderMtx(screen);
                Equal(0, GameCore.Earth.Station.Factory.ProductionQueue.Count, "duplicate order rejected");
                foreach (var recipe in item.BuildRequirements)
                    Equal(recipe.ItemCount * 2, GameCore.Earth.Station.Resources.Stores[recipe.ItemType], "no duplicate recipe charge");
                Equal(item.FullName + " - Installed", screen.Buttons.Single(b => b.ObjectData?.ItemType == ItemTypes.m__t__x).HoverText,
                    "hover explains why installation cannot be ordered again");
            }
            finally { screen.Free(); }
        }

        private void MtxInstalledQueue()
        {
            var item = PrepareMtxProduction(true);
            GameCore.Earth.Station.MtxInstalled = true;
            GameCore.Earth.Station.Factory.ProductionQueue.Add(new ProductionItem(item) { AOCRepeat = true });
            ProductionDays(10);
            Equal(0, GameCore.Earth.Station.Factory.ProductionQueue.Count, "obsolete queued module cleared");
            foreach (var recipe in item.BuildRequirements)
                Equal(recipe.ItemCount * 2, GameCore.Earth.Station.Resources.Stores[recipe.ItemType], "existing installation costs nothing more");
        }

        private void MtxProductionSave()
        {
            var item = PrepareMtxProduction(true);
            GameCore.Earth.Station.Factory.ProductionQueue.Add(new ProductionItem(item) { AOCRepeat = true });
            ProductionDays(1);
            Equal(false, GameCore.Earth.Station.MtxInstalled, "first tick has not completed installation");
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            ProductionDays(20);
            Equal(true, GameCore.Earth.Station.MtxInstalled, "loaded production installs");
            foreach (var recipe in item.BuildRequirements)
                Equal(recipe.ItemCount, GameCore.Earth.Station.Resources.Stores[recipe.ItemType], "loaded order is not charged twice");
            var restored = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            Equal(true, restored.BaseGameData.Planets[StellarBodies.earth].Station.MtxInstalled, "installed flag persists");
            Equal(0, restored.BaseGameData.Planets[StellarBodies.earth].Station.Resources.Stores[ItemTypes.m__t__x], "save contains no phantom module");
        }

        private void MtxGroundOrder(bool automated)
        {
            var item = PrepareMtxProduction(automated, true);
            var screen = OpenMtxProduction(true);
            try
            {
                OrderMtx(screen);
                Equal(0, GameCore.Earth.Factory.ProductionQueue.Count, "ground MTX request cannot be queued");
                foreach (var recipe in item.BuildRequirements)
                    Equal(recipe.ItemCount * 2, GameCore.Earth.PlanetResources.Stores[recipe.ItemType], "ground stock unchanged");
            }
            finally { screen.Free(); }
        }

        private StoreScreen OpenMtxStores(bool ground = false)
        {
            var screen = GD.Load<PackedScene>("res://Screens/Store.tscn").Instantiate<StoreScreen>();
            screen.SceneVariables = new List<SceneVariables> { ground ? SceneVariables.Ground : SceneVariables.Orbit };
            AddChild(screen);
            return screen;
        }

        private void MtxStoreEligibility(string location)
        {
            var (source, target) = PrepareMtxRoute();
            Save.CurrentPlanet = StellarBodies.earth;
            if (location == "unequipped") source.MtxInstalled = false;
            if (location == "captured") GameCore.Earth.ActiveMethanoid = true;
            var ground = location == "ground";
            (ground ? GameCore.Earth.PlanetResources.Stores : source.Resources.Stores).AlternativeView = true;
            var screen = OpenMtxStores(ground);
            try
            {
                Equal(false, screen.MTX.Visible, "no local transmitter controls");
                Equal(true, screen.TradStore.Visible, "ordinary equipment stores remain available");
                screen.SwitchStoreType.EmitSignal(BaseButton.SignalName.ButtonUp);
                screen.SwitchStoreType.EmitSignal(BaseButton.SignalName.ButtonUp);
                Equal(false, screen.MTX.Visible, "toggling cannot bypass local eligibility");
            }
            finally { screen.Free(); }
        }

        private void MtxBeforeDiscovery()
        {
            PrepareMtxRoute();
            Save.Unlocks.Remove(Game_Unlocks.Mass_Tranceiver);
            GameCore.Earth.Station.Resources.Stores.AlternativeView = true;
            var screen = OpenMtxStores();
            try { Equal(true, screen.MTX.Visible, "captured hardware visible before analysis"); }
            finally { screen.Free(); }
        }

        private void MtxStoreLoss()
        {
            PrepareMtxRoute();
            GameCore.Earth.Station.Resources.Stores.AlternativeView = true;
            var screen = OpenMtxStores();
            try
            {
                Equal(true, screen.MTX.Visible, "initial installed module accessible");
                GameCore.Earth.Station.Built = false;
                AdvanceTickDay();
                Equal(false, screen.MTX.Visible, "lost station cannot retain visible controls");
                Equal(true, screen.TradStore.Visible, "ordinary stores restored");
            }
            finally { screen.Free(); }
        }

        private async Task MtxNormalProgression()
        {
            InitializeUi();
            DisableFuelRefining();
            var core = GameCore.SingletonInstance;
            var item = core.GameData.GetItem(ItemTypes.m__t__x);
            // Begin at a friendly, captured station. Capture/combat and staff training are fixtures;
            // discovery, research, paid manufacture and transfers below use normal controls.
            var moon = Save.BaseGameData.Planets[StellarBodies.the_moon];
            moon.ActiveMethanoid = false;
            moon.Station.Built = moon.Station.MtxInstalled = true;
            moon.Station.BuildParts = 8;
            moon.Station.Resources.Stores.AlternativeView = true;
            GameCore.Earth.Station.Built = true;
            GameCore.Earth.Station.BuildParts = 8;
            Save.CurrentPlanet = StellarBodies.the_moon;
            Save.BaseGameData.BulletinTexts[BulletinTypes.Matter_Transmitter].BulletinText = "MTX discovered.";
            core.ChangeScene(Scenes.Store, new List<SceneVariables> { SceneVariables.Orbit });
            AdvanceTickDay();
            Equal(false, item.Research.Locked, "captured Stores unlocks research");
            Equal(true, item.Locked, "discovery alone cannot manufacture");
            Equal(1, Save.Unlocks.Count(x => x == Game_Unlocks.Mass_Tranceiver), "one persistent discovery");
            await FinishBulletin(ActiveScreen<Bulletins>());
            Save.CurrentPlanet = StellarBodies.earth;
            GameCore.Earth.ResearchStaff = new Staff { Type = StaffType.Research, Count = 200, Leader = "Science" };
            GameCore.Earth.ResearchStaff.AddAction(9);
            core.ChangeScene(Scenes.Earth_Research, new List<SceneVariables> { SceneVariables.Ground });
            var research = ActiveScreen<Research>();
            research.Buttons.Single(b => b.ObjectData?.ItemType == ItemTypes.m__t__x).EmitSignal(BaseButton.SignalName.Pressed);
            for (uint day = 1; day <= 100 && !item.Research.Researched; day++) Research.UpdateResearch(day - 1, day);
            Equal(true, item.Research.Researched, "normal research completes");
            Equal(false, item.Locked, "research enables manufacture");
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables> { SceneVariables.Ground });
            await InputFrames();
            await DrainStoppedAudio();
            var factory = GameCore.Earth.Station.Factory;
            factory.AOC = true;
            foreach (var recipe in item.BuildRequirements) GameCore.Earth.Station.Resources.Stores[recipe.ItemType] = recipe.ItemCount;
            var production = OpenMtxProduction();
            try { OrderMtx(production); ProductionDays(20); }
            finally { production.Free(); }
            Equal(true, GameCore.Earth.Station.MtxInstalled, "paid manufacture installs at second station");
            foreach (var recipe in item.BuildRequirements) Equal(0, GameCore.Earth.Station.Resources.Stores[recipe.ItemType], "recipe consumed once");
            core.ChangeScene(Scenes.Store, new List<SceneVariables> { SceneVariables.Orbit });
            var stores = ActiveScreen<StoreScreen>();
            stores.SwitchStoreType.EmitSignal(BaseButton.SignalName.ButtonUp);
            Equal(true, stores.MTX.Visible, "newly installed module accessible");
            var destination = stores.MTX.GetNode<Node>("Destination/StationButtons").GetChildren().OfType<TextureButton>()
                .Single(b => (int)b.GetMeta("StationId") == (int)StellarBodies.the_moon);
            destination.EmitSignal(BaseButton.SignalName.Pressed);
            var row = stores.MTX.GetNode<Node>("Config/ResourceList/ResourceContainer").GetChildren()
                .OfType<MTXRow>().Single(r => r.ItemType == ItemTypes.iron);
            GameCore.Earth.Station.Resources.Stores[ItemTypes.iron] = 101;
            moon.Station.Resources.Stores[ItemTypes.iron] = 0;
            row.LeftCross.EmitSignal(BaseButton.SignalName.Pressed);
            Deuteros.Code.Platform.Screens.MTX.UpdateMTX(0, 1);
            Equal(101, moon.Station.Resources.Stores[ItemTypes.iron], "normal selection sends to captured module");
            row.RightCross.EmitSignal(BaseButton.SignalName.Pressed);
            Deuteros.Code.Platform.Screens.MTX.UpdateMTX(1, 2);
            Equal(51, GameCore.Earth.Station.Resources.Stores[ItemTypes.iron], "balance conserves odd remainder at sender");
            Equal(50, moon.Station.Resources.Stores[ItemTypes.iron], "balanced destination");
            // Static simulation calls above bypass the usual DayPassed view notification.
            stores.MTX.UpdateState();
            Equal("51", row.Count.Text, "MTX readout reflects the balanced inventory");
            var restored = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            Equal(true, restored.BaseGameData.Planets[StellarBodies.earth].Station.MtxInstalled, "new installation saved");
            Equal(StellarBodies.the_moon, restored.BaseGameData.Planets[StellarBodies.earth].Station.Resources.Stores.MTX.Target, "route saved");
            Equal(true, restored.BaseGameData.Planets[StellarBodies.earth].Station.Resources.Stores.MTX.BalanceItems.Contains(ItemTypes.iron), "balance configuration saved");
            await CaptureDisplayEvidence("mtx-installed-transfer");
        }

        private void MtxStoreCompletion()
        {
            PrepareMtxProduction(true);
            Save.Unlocks.Add(Game_Unlocks.Mass_Tranceiver);
            var production = OpenMtxProduction();
            try { OrderMtx(production); }
            finally { production.Free(); }
            GameCore.Earth.Station.Resources.Stores.AlternativeView = true;
            var screen = OpenMtxStores();
            try
            {
                Equal(false, screen.MTX.Visible, "not installed yet");
                for (var i = 0; i < 12 && !GameCore.Earth.Station.MtxInstalled; i++) AdvanceTickDay();
                Equal(true, GameCore.Earth.Station.MtxInstalled, "daily simulation installed module");
                Equal(true, screen.MTX.Visible, "same-day Stores refresh exposes module");
                screen.MTX.GetNode<Button>("Config/Buttons/SwitchStore").EmitSignal(BaseButton.SignalName.Pressed);
                Equal(true, screen.TradStore.Visible, "new controls are bound to the current inventory");
            }
            finally { screen.Free(); }
        }

        private void MtxConstructionStationLoss()
        {
            var item = PrepareMtxProduction(true);
            var station = GameCore.Earth.Station;
            station.Factory.ProductionQueue.Add(new ProductionItem(item) { AOCRepeat = true });
            ProductionDays(1);
            station.Built = false;
            ProductionDays(20);
            Equal(false, station.MtxInstalled, "absent station cannot finish installing a module");
            station.Built = true;
            ProductionDays(20);
            Equal(true, station.MtxInstalled, "restored station can finish the paid order");
            foreach (var recipe in item.BuildRequirements)
                Equal(recipe.ItemCount, station.Resources.Stores[recipe.ItemType], "suspension does not recharge materials");
        }

        private async Task MtxArtworkBounds()
        {
            InitializeUi();
            var item = PrepareMtxProduction(false);
            var production = OpenMtxProduction();
            try
            {
                OrderMtx(production);
                await InputFrames();
                Equal(new Vector2(64, 56), production.GetNode<TextureRect>("Sprites/ItemProgressImage").Size, "construction illustration bounds");
                Equal(new Vector2(48, 46), production.GetNode<TextureRect>("Sprites/SmallItemImage").Size, "blueprint illustration bounds");
                await CaptureDisplayEvidence("mtx-production");
            }
            finally { production.Free(); }
            GameCore.Earth.CurrentResearchItem = item.Research;
            var research = GD.Load<PackedScene>("res://Screens/Earth/Research.tscn").Instantiate<Research>();
            research.SceneVariables = new List<SceneVariables> { SceneVariables.Ground };
            AddChild(research);
            try
            {
                await InputFrames();
                Equal(new Vector2(48, 46), research.GetNode<TextureRect>("Sprites/ResearchImage").Size, "research illustration bounds");
                await CaptureDisplayEvidence("mtx-research");
            }
            finally { research.Free(); }
            await DrainStoppedAudio();
        }
    }
}
