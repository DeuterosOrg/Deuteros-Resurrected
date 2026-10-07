using System.Collections.Generic;
using System.Reflection;
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
        private async Task<(ShipInterior, Battle)> OpenBattleEncounter(bool roaming = false, int playerDrones = 10, int stationDrones = 40, bool timeRunning = false, bool ptl = false)
        {
            var interior = await OpenInterior(Ship_Types.IOS);
            GameCore.SingletonInstance.SetProcess(false);
            var ship = (InterStellarShip)interior.Ship;
            ship.Name = "Battle Test";
            ship.ShipState = Ship_States.UnDocked;
            ship.DFCC = true;
            ship.PTL = ptl;
            if (ptl) ship.Fuel = 200;
            ship.DroneCount = playerDrones;
            ship.ACC = null;
            ship.Modules[0].ModuleType = Module_Types.Tool;
            ship.Modules[0].ItemStored = ItemTypes.d__f__c__c;
            ship.Modules[0].ItemCount = 1;
            Save.AtWar = true;
            interior.CurrentPlanet.ActiveMethanoid = !roaming;
            if (roaming) Save.Ships.Add(new EnemyFleet { ShipType = Ship_Types.IOS, MethanoidOwned = true,
                PlanetLocation = ship.PlanetLocation, StarLocation = ship.StarLocation, Attacking = true,
                AttackDay = 5, AttackTrigger = 20, DroneCount = 40, Fuel = 100, Modules = new List<ShipModule>() });
            interior.CurrentPlanet.Station.Resources.Stores[ItemTypes.ios_drone] = stationDrones;
            interior.UpdateState();
            Save.TimeSkip = Save.TimeSkipDay = timeRunning;
            Press(interior, "Modules/00");
            var battle = interior.GetNode<Control>("Window").GetChild<Battle>(0);
            // Freeze random combat after opening the real module; choose the outcome below.
            battle.GetNode<Timer>("Timer").Stop();
            await InputFrames();
            return (interior, battle);
        }

        private async Task BattleOwnsCommands()
        {
            var (interior, battle) = await OpenBattleEncounter(timeRunning: true, ptl: true);
            var ship = (InterStellarShip)interior.Ship;
            var station = interior.CurrentPlanet.Station;
            var core = GameCore.SingletonInstance;
            Equal(0, interior.CurrentPlanet.Station.Resources.Stores[ItemTypes.ios_drone], "battle holds the station reservation");
            Press(interior, "Dock");
            Equal(Ship_States.UnDocked, ship.ShipState, "reserved defenders do not expose background docking");
            Press(interior, "EngineControls/EngageEngine");
            Equal(Ship_States.UnDocked, ship.ShipState, "escape goes through battle Flee rather than background engine");
            Press(interior, "Modules/00");
            Equal(1, interior.GetNode("Window").GetChildCount(), "cannot open a second encounter over the reserved fleet");
            Equal(true, Cursor.IsLocked, "combat owns pointer input");
            Equal(false, Save.TimeSkip || Save.TimeSkipDay, "opening battle stops fast-forward and held-day input");
            var day = Save.CurrentDay;
            core._Process(GameClock.NormalIntervalSeconds);
            Equal(day, Save.CurrentDay, "normal simulation cannot destroy the ship during combat");
            var right = new InputEventMouseButton { ButtonIndex = MouseButton.Right, Pressed = true, Position = new Vector2(10, 10) };
            PushGameInput(right);
            await InputFrames();
            Equal(true, Cursor.IsLocked, "right click cannot release the combat input owner");
            Equal(Scenes.ShipInterior, core.currentScene, "right click cannot leave combat through the overview");
            async Task Click(BaseButton button)
            {
                var point = button.GetGlobalRect().GetCenter();
                PushGameInput(new InputEventMouseMotion { Position = point, GlobalPosition = point });
                foreach (var pressed in new[] { true, false })
                    PushGameInput(new InputEventMouseButton { Position = point, GlobalPosition = point, ButtonIndex = MouseButton.Left, Pressed = pressed });
                await InputFrames();
            }
            await Click(interior.GetNode<Button>("Dock"));
            Equal(Ship_States.UnDocked, ship.ShipState, "real background Dock pointer is blocked too");
            var logic = battle.GetNode<BattleCanvas>("BattleCanvas").BattleLogic;
            logic.GetType().GetProperty("BattleState", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(logic, BattleState.FleetsInBattle);
            await Click(battle.GetNode<TextureButton>("PlayerPTL"));
            Equal(100, ship.Fuel, "real PTL pointer stays available and charges once");
            await Click(battle.GetNode<TextureButton>("PlayerFlee"));
            Equal(true, logic.PlayerFled, "real Flee pointer remains available inside combat");
            core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await InputFrames();
            Equal(false, Cursor.IsLocked, "leaving frees the input owner");
            Equal(true, station.Resources.Stores[ItemTypes.ios_drone] > 0, "leaving returns surviving station defenders");
            core._Process(0);
            Equal(day + 1, Save.CurrentDay, "deferred normal simulation resumes once after exit");
        }

        private async Task BattleEmptyFleets()
        {
            foreach (var counts in new[] { (0, 40), (10, 0), (0, 0) })
            {
                var (interior, battle) = await OpenBattleEncounter(playerDrones: counts.Item1, stationDrones: counts.Item2);
                var ship = interior.Ship;
                var station = interior.CurrentPlanet.Station;
                var logic = battle.GetNode<BattleCanvas>("BattleCanvas").BattleLogic;
                battle.GetNode<BattleCanvas>("BattleCanvas").QueueRedraw();
                await InputFrames();
                Equal(true, logic.Completed(), "a fleet with no drones cannot enter attrition combat");
                await ToSignal(GetTree().CreateTimer(1.3), SceneTreeTimer.SignalName.Timeout);
                Equal(false, IsInstanceValid(battle), "empty encounter completes and frees its window");
                Equal(counts.Item1 > 0, Save.Ships.Contains(ship), "only a ship with surviving drones remains");
                Equal(counts.Item2, station.Resources.Stores[ItemTypes.ios_drone], "opponent drones survive unchanged");
                GameCore.SingletonInstance.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
                await InputFrames();
            }
        }

        private sealed class PtlBoundaryRandom : System.Random
        {
            private readonly int splash;
            public PtlBoundaryRandom(int splash) => this.splash = splash;
            public override int Next(int maxValue) => maxValue == 128 ? 8 : maxValue == 64 ? splash : 0;
        }

        private void BattlePtlBoundaries()
        {
            // Original Disk 2 raw $23780/$237B0 use carry: damage must be strictly below the fleet.
            foreach (var counts in new[] { (20, 10, 6, 14, 2), (6, 30, 6, 2, 20), (6, 10, 6, 2, 2),
                                           (5, 9, 6, 2, 2), (7, 11, 6, 1, 1), (20, 30, 10, 15, 20),
                                           (20, 30, 40, 15, 20), (20, 30, 0, 20, 20) })
            {
                var player = new IOS { DroneCount = counts.Item1, PTL = true, Fuel = 200 };
                Save.Ships.Add(player);
                var enemy = new EnemyFleet { DroneCount = counts.Item2 };
                var logic = new Deuteros.Code.Objects.Battle.BattleLogic(player, enemy, 0, null, null, null, null, null);
                var type = logic.GetType();
                type.GetProperty("BattleState", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(logic, BattleState.FleetsInBattle);
                type.GetField("r", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(logic, new PtlBoundaryRandom(counts.Item3));
                logic.LaunchPTL();
                Equal(100, player.Fuel, "launch pays once");
                type.GetField("PTLCounter", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(logic, 37);
                type.GetMethod("ProcessPTL", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(logic, null);
                Equal(counts.Item4, logic.Player1Ships, "player splash boundary");
                Equal(counts.Item5, logic.Player2Ships, "enemy impact boundary");
                type.GetMethod("DoBattleRound", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(logic, null);
                Equal(counts.Item4 * 4, logic.Player1Power, "next round uses surviving player fleet power");
                Equal(counts.Item5 * 4, logic.Player2Power, "next round uses surviving enemy fleet power");
                for (var i = 0; i < 12; i++)
                {
                    type.GetMethod("DoBattleRound", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(logic, null);
                    type.GetMethod("ApplyRoundResults", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(logic, null);
                }
                Equal(true, logic.Player1Ships >= 0 && logic.Player2Ships >= 0, "fleet counts never become negative");
                Equal(100, player.Fuel, "settlement cannot charge a second launch");
            }
        }

        private async Task BattleLossCleanup()
        {
            var (interior, battle) = await OpenBattleEncounter();
            var ship = interior.Ship;
            var logic = battle.GetNode<BattleCanvas>("BattleCanvas").BattleLogic;
            logic.Player1Ships = 0;
            typeof(Deuteros.Code.Objects.Battle.BattleLogic).GetProperty("BattleState", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(logic, BattleState.BattleEnded);
            await ToSignal(GetTree().CreateTimer(1.3), SceneTreeTimer.SignalName.Timeout);
            Equal(false, Save.Ships.Contains(ship), "real battle callback removes the defeated ship");
            Equal(false, IsInstanceValid(battle), "completed battle window is freed, not only detached");
            Equal(false, GameCore.SingletonInstance.currentScene == Scenes.ShipInterior, "defeated ship's controls close immediately");
            Equal(1, Save.News.GetNews(100).FindAll(n => n.Contains("Battle Test Destroyed")).Count, "actual battle defeat publishes once");
        }

        private async Task BattleCompletedThenLeave()
        {
            var (interior, battle) = await OpenBattleEncounter();
            var ship = interior.Ship;
            var logic = battle.GetNode<BattleCanvas>("BattleCanvas").BattleLogic;
            logic.Player1Ships = 0;
            typeof(Deuteros.Code.Objects.Battle.BattleLogic).GetProperty("BattleState", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(logic, BattleState.BattleEnded);
            GameCore.SingletonInstance.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await ToSignal(GetTree().CreateTimer(1.3), SceneTreeTimer.SignalName.Timeout);
            Equal(false, Save.Ships.Contains(ship), "closing during result presentation cannot undo a committed defeat");
            Equal(1, Save.News.GetNews(100).FindAll(n => n.Contains("Battle Test Destroyed")).Count, "completed interrupted battle reports defeat once");
            Equal(Scenes.SaveScreen, GameCore.SingletonInstance.currentScene, "old result cannot redirect the newer screen");
        }

        private async Task BattleEnemyFleesOnce()
        {
            var (interior, battle) = await OpenBattleEncounter(true);
            var enemy = Save.Ships.OfType<EnemyFleet>().Single();
            var logic = battle.GetNode<BattleCanvas>("BattleCanvas").BattleLogic;
            logic.Player1Ships = 8;
            logic.Player2Ships = 15;
            var type = typeof(Deuteros.Code.Objects.Battle.BattleLogic);
            type.GetField("P2fleeing", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(logic, true);
            type.GetProperty("BattleState", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(logic, BattleState.BattleEnded);
            await ToSignal(GetTree().CreateTimer(1.3), SceneTreeTimer.SignalName.Timeout);
            Equal(false, IsInstanceValid(battle), "successful battle is also freed");
            Equal(40, enemy.AttackTrigger, "enemy retreat doubles its threshold once, not again on tree exit");
            Equal(false, enemy.Attacking, "fleeing fleet stops attacking");
            Equal(15, enemy.DroneCount, "enemy survivors retained");
            Equal(8, ((InterStellarShip)interior.Ship).DroneCount, "player survivors retained");
            Equal(Scenes.ShipInterior, GameCore.SingletonInstance.currentScene, "surviving ship remains usable");
            Equal(40, interior.CurrentPlanet.Station.Resources.Stores[ItemTypes.ios_drone], "roaming battle does not credit station drones");
            Equal(0, Save.News.GetNews(100).Count, "retreat invents no ship destruction");
            GameCore.SingletonInstance.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            await InputFrames();
        }

        private async Task BattleInterrupted(bool replaceWorld)
        {
            var (interior, battle) = await OpenBattleEncounter();
            var core = GameCore.SingletonInstance;
            var oldSave = Save;
            var station = interior.CurrentPlanet.Station;
            Equal(0, station.Resources.Stores[ItemTypes.ios_drone], "encounter reserves station drones");
            if (!replaceWorld)
            {
                var logic = battle.GetNode<BattleCanvas>("BattleCanvas").BattleLogic;
                logic.Player1Ships = 8;
                logic.Player2Ships = 27;
            }
            if (replaceWorld)
            {
                var replacement = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                core.LoadSavedGame(replacement);
            }
            else core.ChangeScene(Scenes.SaveScreen, new List<SceneVariables>());
            var replacementBefore = SaveStorage.Serialize(Save);
            await ToSignal(GetTree().CreateTimer(1.3), SceneTreeTimer.SignalName.Timeout);
            Equal(false, IsInstanceValid(battle), "leaving frees the battle window");
            Equal(replaceWorld ? 40 : 27, station.Resources.Stores[ItemTypes.ios_drone], "unspent reserved drones return to the encounter's own station");
            Equal(0, oldSave.News.GetNews(100).Count, "interrupted encounter has no fabricated defeat");
            if (replaceWorld) Equal(replacementBefore, SaveStorage.Serialize(Save), "old encounter cannot mutate the replacement world");
        }
    }
}
