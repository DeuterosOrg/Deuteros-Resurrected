using System;
using System.Threading.Tasks;
using Godot;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Deuteros.Code.Utility;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Screens;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private SCG NewRogueCandidate()
        {
            CoreData.CreateBaseGameData();
            GameCore.SingletonInstance.GameData.ActiveSaveFile = CoreData.CreateNewSaveFile();
            var ship = NewInterstellarRoute();
            ship.Pilot.AddAction(31);
            ship.EngageEngine();
            for (var update = 0; update < 16; update++) AdvanceInterstellar();
            Equal(4, ship.Pilot.GetLevel(), "real Hyperlight arrival supplies Warlord");
            Equal(StellarBodies.atlantic, ship.PlanetLocation, "fixture completed actual journey");
            ship.Fuel = 250;
            ship.FuelType = ItemTypes.hed_fuel;
            while (ship.Modules.Count < 6) ship.Modules.Add(new ShipModule());
            foreach (var planet in Save.BaseGameData.Planets.Values)
            {
                planet.ActiveMethanoid = false;
                planet.Station.Built = false;
                planet.Station.BuildParts = 0;
                planet.Station.RefiningSlot = -1;
            }
            foreach (var star in Save.BaseGameData.Stars.Keys.OrderBy(s => s).Skip(1).Take(4))
            {
                var enemy = Save.BaseGameData.Planets.Values.First(p => p.ParentStar == star && p.PlanetId != StellarBodies.atlantic);
                enemy.ActiveMethanoid = true;
                enemy.Station.Built = true; enemy.Station.BuildParts = 8; enemy.Station.Type = 9;
            }
            var home = Save.BaseGameData.Planets[StellarBodies.atlantic];
            home.Station.Built = true; home.Station.BuildParts = 8; home.Station.Type = 8;
            Save.AtWar = true;
            return ship;
        }

        private void RogueSelection()
        {
            var ship = NewRogueCandidate();
            var state = Save.RogueCrew;
            state.TryStart(Save);
            Equal("BOUNTY", ship.Name, "qualifying Warlord steals the SCG");
            Equal(5, ship.Pilot.GetLevel(), "mutiny changes marine rank to Pirate");
            Equal(12, ship.Modules[0].ItemCount, "takeover does not prematurely refit cargo");
            Equal(true, state.Occurred && state.MutinyPending, "one pending mutiny");
            Equal(true, ReferenceEquals(ship.Pilot, state.Crew), "selected pilot identity");
            Equal(11, state.Stage, "original initial controller stage");
            ship.Name = "Player renamed";
            state.TryStart(Save);
            Equal("Player renamed", ship.Name, "selection cannot repeat");
        }
        private void RogueSelectionGates()
        {
            var rejected = new Action<SCG>[] {
                ship => { var research = Save.BaseGameData.ItemList.Single(i => i.ItemType == ItemTypes.hyperlight).Research; research.Locked = true; research.ResearchPercentageComplete = 1; },
                ship => Save.BaseGameData.ItemList.Single(i => i.ItemType == ItemTypes.hyperlight).Research.ResearchPercentageComplete = 0,
                ship => ship.Fuel = 249,
                ship => ship.ShipState = Ship_States.Docked,
                ship => ship.Pilot.Warlord = false,
                ship => { ship.Modules[5].ModuleType = Module_Types.Cryo; ship.Modules[5].StaffStored = new Staff { Type = StaffType.Marines, Count = 1 }; },
                ship => { ship.Modules[5].ModuleType = Module_Types.Tool; ship.Modules[5].ItemStored = ItemTypes.pulse_blaster_laser; ship.Modules[5].ItemCount = 1; },
                ship => { var p = Save.BaseGameData.Planets.Values.First(p => p.ParentStar == Save.BaseGameData.Stars.Keys.OrderBy(s => s).Skip(5).First()); p.ActiveMethanoid = true; },
                ship => {
                    var lastStar = Save.BaseGameData.Stars.Keys.OrderBy(s => s).Skip(4).First();
                    foreach (var p in Save.BaseGameData.Planets.Values.Where(p => p.ParentStar == lastStar)) p.ActiveMethanoid = false;
                    Save.BaseGameData.Planets[StellarBodies.earth].Station.Built = true;
                    Save.BaseGameData.Planets[StellarBodies.mercury].ActiveMethanoid = true;
                    Equal(4, Save.BaseGameData.Planets.Values.Where(p => p.ActiveMethanoid).Select(p => p.ParentStar).Distinct().Count(), "first-system test stays below system limit");
                }
            };
            for (var index = 0; index < rejected.Length; index++)
            {
                var ship = NewRogueCandidate(); rejected[index](ship); Save.RogueCrew.TryStart(Save);
                Equal(false, Save.RogueCrew.Occurred, "ineligible selection boundary " + index);
                Equal(12, ship.Modules[0].ItemCount, "rejection preserves cargo");
            }
            foreach (var configure in new Action<SCG>[] {
                ship => Save.BaseGameData.ItemList.Single(i => i.ItemType == ItemTypes.hyperlight).Research.ResearchPercentageComplete = 1,
                ship => { ship.DFCC = true; ship.DroneCount = 7; },
                ship => ship.PTL = true,
                ship => ship.EngineDamaged = true,
                ship => ship.Modules[5].ModuleType = Module_Types.Cryo,
                ship => ship.Engine = false
            })
            {
                var ship = NewRogueCandidate(); configure(ship); Save.RogueCrew.TryStart(Save);
                Equal(true, Save.RogueCrew.Occurred, "source does not exclude this eligible hull");
            }
        }

        private void RogueSaveLegacy()
        {
            NewRogueCandidate();
            var legacy = JObject.Parse(SaveStorage.Serialize(Save));
            ((JObject)legacy["Game"]).Remove("RogueCrew");
            foreach (var property in legacy.Descendants().OfType<JProperty>().Where(p => p.Name == "Pirate").ToList()) property.Remove();
            var restored = SaveStorage.Deserialize(legacy.ToString());
            Equal(false, restored.RogueCrew.Occurred, "old save does not invent mutiny");
            Equal(4, restored.Ships.OfType<SCG>().Single().Pilot.GetLevel(), "old Warlord survives migration");
        }

        private void RogueSaveValidation()
        {
            var ship = NewRogueCandidate(); Save.RogueCrew.TryStart(Save);
            var json = SaveStorage.Serialize(Save);
            var restored = SaveStorage.Deserialize(json);
            Equal(true, ReferenceEquals(restored.RogueCrew.Crew, restored.Ships.OfType<SCG>().Single().Pilot), "saved selected crew retains identity");
            Equal(5, restored.RogueCrew.Crew.GetLevel(), "saved Pirate rank");
            var active = Save;
            foreach (var corrupt in new Action<JObject>[] {
                d => d["Game"]["RogueCrew"]["Stage"] = 21,
                d => d["Game"]["RogueCrew"]["PrisonDivider"] = 4,
                d => d["Game"]["RogueCrew"]["PrisonCountdown"] = 253,
                d => d["Game"]["RogueCrew"]["Occurred"] = false,
                d => d["Game"]["RogueCrew"]["OriginalCrewCount"] = 100,
                d => d["Game"]["Ships"][0]["Pilot"]["Pirate"] = false,
                d => d["Game"]["Ships"][0]["Pilot"]["Type"] = 0,
                d => d["Game"]["Ships"][0]["Modules"][0]["StaffStored"] = d["Game"]["RogueCrew"]["Crew"].DeepClone(),
                d => { d["Game"]["RogueCrew"]["Crew"] = d["Game"]["Ships"][0]["Pilot"].DeepClone(); d["Game"]["Ships"][0]["Pilot"] = null; },
                d => d["Game"]["RogueCrew"] = null
            })
            {
                var document = JObject.Parse(json); corrupt(document);
                var rejected = false;
                try { GameCore.SingletonInstance.LoadSavedGame(SaveStorage.Deserialize(document.ToString())); }
                catch (Exception e) when (e is IOException || e is InvalidDataException || e is Newtonsoft.Json.JsonException) { rejected = true; }
                Equal(true, rejected, "invalid rogue state rejected");
                Equal(true, ReferenceEquals(active, Save), "rejected save preserves current world");
            }
        }
        private void RogueCrewLocations()
        {
            var ship = NewRogueCandidate(); Save.RogueCrew.TryStart(Save);
            var state = Save.RogueCrew;
            Equal(true, state.Controls(ship),
                "selected pilot controls its ship");
            var crew = ship.Pilot;
            state.CrewLost(new Staff { Type = StaffType.Marines });
            Equal(true, ReferenceEquals(crew, state.Crew), "unrelated loss preserves selected crew");
            ship.Pilot = null;
            Equal(false, state.Controls(ship), "removing pilot relinquishes hull");
            var roster = Save.BaseGameData.Planets[ship.PlanetLocation].Station.Resources.Staff;
            roster[0] = crew;
            SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            roster[0] = null;
            ship.Modules[5] = new ShipModule { ModuleType = Module_Types.Tool, ItemStored = ItemTypes.prison_pod, ItemCount = 1, StaffStored = crew };
            var restored = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            Equal(true, ReferenceEquals(restored.RogueCrew.Crew, restored.Ships[0].Modules[5].StaffStored), "prison reference survives save");
            Equal(true, state.Contained(Save),
                "selected crew is contained");
            ship.Modules[5].StaffStored = null;
            Equal(false, state.Contained(Save), "empty prison releases containment");
            state.CrewLost(crew);
            Equal(true, state.Occurred, "death does not permit repeat mutiny");
            Equal(true, state.Crew == null, "loss clears selected identity");
            Equal(0, state.Stage, "loss stops controller");
            SaveStorage.Deserialize(SaveStorage.Serialize(Save));
        }

        private void RogueStableSelection()
        {
            var ship = NewRogueCandidate(); ship.AutomationSlot = 8;
            var other = new SCG { Name = "Earlier slot", ShipType = Ship_Types.SCG, Engine = true, Fuel = 250,
                PlanetLocation = ship.PlanetLocation, StarLocation = ship.StarLocation, ShipState = Ship_States.UnDocked,
                DestinationPlanetLocation = ship.DestinationPlanetLocation, DestinationStarLocation = ship.DestinationStarLocation,
                AutomationSlot = 2, Pilot = new Staff { Leader = "Second", Type = StaffType.Marines, Count = 10, Warlord = true },
                Modules = Enumerable.Range(0, 6).Select(_ => new ShipModule()).ToList() };
            other.Pilot.AddAction(40);
            other.ACC = new Deuteros.Code.Objects.ACC { Ship = other, Active = true, CycleMode = true, Refuelling = true,
                SourceItems = new(), DestinationItems = new(), CurrentSource = ItemTypes.iron, CurrentDestination = ItemTypes.iron };
            Save.Ships.Add(other);
            Save.RogueCrew.TryStart(Save);
            Equal("BOUNTY", other.Name, "allocated slot wins over list order");
            Equal("Interstellar regression", ship.Name, "later slot remains human");
            Equal(false, other.ACC.Active || other.ACC.CycleMode || other.ACC.Refuelling, "takeover disables every ACC activity mode");
            var restored = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            Equal(other.ShipID, restored.Ships.Single(s => ReferenceEquals(s.Pilot, restored.RogueCrew.Crew)).ShipID, "save preserves selected ship");
        }

        private void AdvanceRogue(int random = 0) => Save.RogueCrew.Advance(Save, () => random);

        private void RogueStageWaits()
        {
            var ship = NewRogueCandidate(); Save.RogueCrew.TryStart(Save);
            foreach (var stage in new[] {4,7,11,14,16})
            {
                Save.RogueCrew.Stage = stage;
                ship.ShipState = Ship_States.Launching;
                AdvanceRogue(); Equal(stage, Save.RogueCrew.Stage, "busy ship waits at stage " + stage);
                ship.ShipState = Ship_States.UnDocked;
                AdvanceRogue(); Equal(stage + 1, Save.RogueCrew.Stage, "one transition at stage " + stage);
            }
            foreach (var stage in new[] {12,15,20})
            {
                Save.RogueCrew.Stage = stage; AdvanceRogue(); Equal(1, Save.RogueCrew.Stage, "reset stage " + stage);
            }
            foreach (var p in Save.BaseGameData.Planets.Values) p.ActiveMethanoid = false;
            AdvanceRogue(); Equal(1, Save.RogueCrew.Stage, "no hostile route waits safely");
            Equal(Ship_States.UnDocked, ship.ShipState, "no teleport or launch without a route");
        }

        private void RogueLocalRaidJourney()
        {
            var ship = NewRogueCandidate(); Save.RogueCrew.TryStart(Save);
            var home = Save.BaseGameData.Planets[ship.PlanetLocation];
            home.Station.Resources.Stores[ItemTypes.ios_drone] = 5;
            home.Station.Resources.Stores[ItemTypes.titanium] = 600;
            var enemy = Save.BaseGameData.Planets.Values.Single(p => p.ParentStar == home.ParentStar && p.ActiveMethanoid);
            ship.DFCC = ship.PTL = ship.EngineDamaged = true; ship.DroneCount = 7;
            Save.RogueCrew.Stage = 1; AdvanceRogue();
            Equal(2, Save.RogueCrew.Stage, "local hostile course selected");
            Equal(enemy.PlanetId, ship.DestinationPlanetLocation, "hostile target");
            AdvanceRogue(); Equal(3, Save.RogueCrew.Stage, "local engine started");
            Equal(Ship_States.InTransit, ship.ShipState, "real local travel");
            for (var i = 0; i < 100 && ship.ShipState == Ship_States.InTransit; i++)
            {
                AdvanceInterstellar();
                if (ship.ShipState == Ship_States.InTransit) { AdvanceRogue(); Equal(3, Save.RogueCrew.Stage, "refit waits during local transit"); }
            }
            Equal(enemy.PlanetId, ship.PlanetLocation, "arrives through shared ship update");
            AdvanceRogue(); Equal(Ship_States.Docking, ship.ShipState, "hostile docking started");
            AdvanceInterstellar(); Equal(Ship_States.Docked, ship.ShipState, "hostile docking finishes");
            Equal(0, enemy.Station.SdmCountdown, "rogue does not arm ordinary hostile SDM");
            AdvanceRogue(); Equal(4, Save.RogueCrew.Stage, "refit advances once");
            Equal(250, ship.Fuel, "hostile fuel replacement");
            Equal(6, ship.Modules.Count(m => m.ModuleType == Module_Types.Supply && m.ItemCount == 0), "six empty supply mounts");
            Equal(false, ship.DFCC || ship.PTL || ship.DroneCount != 0, "refit removes conversions and fleet");
            Equal(true, ship.EngineDamaged, "refit preserves separate drive damage");
            AdvanceInterstellar(); AdvanceRogue(); Equal(5, Save.RogueCrew.Stage, "launch wait finishes");
            AdvanceRogue(); Equal(home.PlanetId, ship.DestinationPlanetLocation, "five IOS drones selects human raid target");
            AdvanceRogue(); Equal(7, Save.RogueCrew.Stage, "human route started");
            for (var i = 0; i < 100 && ship.ShipState == Ship_States.InTransit; i++) AdvanceInterstellar();
            AdvanceRogue(); Equal(8, Save.RogueCrew.Stage, "human arrival observed");
            AdvanceRogue(); Equal(9, Save.RogueCrew.Stage, "human docking scheduled");
            AdvanceRogue(); Equal(9, Save.RogueCrew.Stage, "MTX waits for dock completion");
            AdvanceInterstellar(); AdvanceRogue(); Equal(10, Save.RogueCrew.Stage, "docked raid ready");
            AdvanceRogue(); Equal(11, Save.RogueCrew.Stage, "one raid before launch wait");
            Equal(0, home.Station.Resources.Stores[ItemTypes.titanium], "human stock stolen");
            Equal(600, ship.Modules.Sum(m => m.ItemCount), "exact cargo stolen");
            AdvanceRogue(); Equal(600, ship.Modules.Sum(m => m.ItemCount), "launching does not raid twice");
        }

        private void RogueMixedRaid()
        {
            var ship = NewRogueCandidate(); Save.RogueCrew.TryStart(Save);
            ship.ShipState = Ship_States.Docked; ship.Fuel = 200; ship.DFCC = true;
            var stores = Save.BaseGameData.Planets[ship.PlanetLocation].Station.Resources.Stores;
            stores.Items.Clear(); stores[ItemTypes.titanium] = 600; stores[ItemTypes.aluminium] = 200; stores[ItemTypes.hed_fuel] = 17;
            ship.Modules = Enumerable.Range(0, 6).Select(_ => new ShipModule { ModuleType = Module_Types.Supply,
                ItemStored = ItemTypes.iron, ItemCount = 3 }).ToList();
            ship.Modules[1] = new ShipModule { ModuleType = Module_Types.Tool, ItemStored = ItemTypes.grapple, ItemCount = 1 };
            ship.Modules[4] = new ShipModule { ModuleType = Module_Types.None };
            Save.RogueCrew.Stage = 10; AdvanceRogue();
            Equal(11, Save.RogueCrew.Stage, "raid consumed");
            Equal(250, ship.Modules[0].ItemCount, "first titanium pod replaces old iron");
            Equal(250, ship.Modules[2].ItemCount, "second titanium pod");
            Equal(100, ship.Modules[3].ItemCount, "third titanium pod");
            Equal(ItemTypes.aluminium, ship.Modules[5].ItemStored, "fourth supply pod advances mineral");
            Equal(200, ship.Modules[5].ItemCount, "aluminium amount");
            Equal(ItemTypes.grapple, ship.Modules[1].ItemStored, "tool preserved");
            Equal(Module_Types.None, ship.Modules[4].ModuleType, "empty mount preserved");
            Equal(217, ship.Fuel, "raid refuels from remaining HeD at one to one despite DFCC");
            Equal(0, stores[ItemTypes.titanium] + stores[ItemTypes.aluminium] + stores[ItemTypes.hed_fuel], "exact station debits");
            Equal(Ship_States.Launching, ship.ShipState, "raid launches hull");
        }

        private void RogueMtxRedirect()
        {
            var ship = NewRogueCandidate(); Save.RogueCrew.TryStart(Save); ship.ShipState = Ship_States.Docked;
            var source = Save.BaseGameData.Planets[ship.PlanetLocation];
            var target = Save.BaseGameData.Planets[StellarBodies.earth];
            target.Station.Built = true; target.Station.BuildParts = 8; target.Station.Type = 8;
            target.Station.RefiningSlot = 0; target.Station.MtxInstalled = false;
            target.Station.Resources.Stores[ItemTypes.aluminium] = 200;
            var mtx = source.Station.Resources.Stores.MTX;
            source.Station.MtxInstalled = false; Save.RogueCrew.Stage = 9; AdvanceRogue();
            Equal(StellarBodies.none, mtx.Target, "source MTX is required");
            source.Station.MtxInstalled = true; Save.RogueCrew.Stage = 9; AdvanceRogue();
            Equal(target.PlanetId, mtx.Target, "first Solar allocation without target MTX is assigned");
            Equal(true, mtx.SendItems.SequenceEqual(new[] {ItemTypes.titanium,ItemTypes.aluminium,ItemTypes.paladium,ItemTypes.platinum,ItemTypes.meh_fuel,ItemTypes.hed_fuel}), "original send mask");
            Equal(true, mtx.BalanceItems.SequenceEqual(mtx.SendItems), "original balance mask");
            source.Station.Resources.Stores[ItemTypes.titanium] = 100;
            Save.Unlocks.Add(Game_Unlocks.Mass_Tranceiver);
            Deuteros.Code.Platform.Screens.MTX.UpdateMTX(Save.CurrentDay, Save.CurrentDay + 1);
            Equal(100, source.Station.Resources.Stores[ItemTypes.titanium], "route assignment does not bypass normal target MTX eligibility");
        }

        private void RogueOccupiedSabotage()
        {
            foreach (var roll in new[] {0,4})
            {
                var ship = NewRogueCandidate(); Save.RogueCrew.TryStart(Save); ship.Pilot.Count = 81;
                var planet = Save.BaseGameData.Planets[ship.PlanetLocation];
                planet.Station.SdmInstalled = false;
                var occupant = new IOS { ShipType = Ship_Types.IOS, ShipState = Ship_States.Docked, Engine = true,
                    PlanetLocation = ship.PlanetLocation, StarLocation = ship.StarLocation, Modules = new() };
                Save.Ships.Add(occupant);
                Save.RogueCrew.Stage = 9; ship.Dock(); AdvanceInterstellar();
                Equal(Ship_States.UnDocked, ship.ShipState, "occupied docking returns rogue to orbit");
                Equal(16, Save.RogueCrew.Stage, "occupied bay selects sabotage");
                AdvanceRogue(); Equal(17, Save.RogueCrew.Stage, "sabotage waits one stage");
                var restored = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                GameCore.SingletonInstance.GameData.ActiveSaveFile = restored;
                ship = restored.Ships.OfType<SCG>().Single(); planet = restored.BaseGameData.Planets[ship.PlanetLocation];
                AdvanceRogue(); Equal(10, ship.Pilot.Count, "temporary crew shifts right by three");
                Equal<int?>(81, Save.RogueCrew.OriginalCrewCount, "original strength saved");
                restored = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                GameCore.SingletonInstance.GameData.ActiveSaveFile = restored;
                ship = restored.Ships.OfType<SCG>().Single(); planet = restored.BaseGameData.Planets[ship.PlanetLocation];
                var reports = Save.News.GetNews(100).Count;
                AdvanceRogue(roll); Equal(19, Save.RogueCrew.Stage, "random roll advances before mutation");
                Equal(roll == 4 ? 150 : 0, planet.Station.SdmCountdown, "one sabotage decision");
                Equal(false, planet.Station.SdmInstalled, "pirate timer does not manufacture permanent SDM hardware");
                Equal(roll == 4, SdmSystem.CanAccess(Save, planet), "armed sabotage is accessible for defusing without granting equipment");
                Equal(reports + (roll == 4 ? 1 : 0), Save.News.GetNews(100).Count, "exact warning count");
                restored = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                GameCore.SingletonInstance.GameData.ActiveSaveFile = restored;
                ship = restored.Ships.OfType<SCG>().Single(); planet = restored.BaseGameData.Planets[ship.PlanetLocation];
                planet.Station.Resources.Stores[ItemTypes.titanium] = 600;
                ship.Modules[0] = new ShipModule { ModuleType = Module_Types.Supply };
                ship.Modules[1] = new ShipModule { ModuleType = Module_Types.Supply };
                AdvanceRogue(4);
                Equal(81, ship.Pilot.Count, "saved stage19 restores original crew strength");
                Equal<int?>(null, Save.RogueCrew.OriginalCrewCount, "temporary strength cleared");
                Equal(1, Save.RogueCrew.Stage, "sabotage resets routing");
                Equal(350, planet.Station.Resources.Stores[ItemTypes.titanium], "one pod raid after sabotage");
                Equal(0, ship.Modules[1].ItemCount, "second mount untouched");
                Equal(reports + (roll == 4 ? 1 : 0), Save.News.GetNews(100).Count, "reload cannot reroll warning");
            }
        }

        private void RogueSdmEscape()
        {
            foreach (var docking in new[] {false,true})
            {
                var ship = NewRogueCandidate(); Save.RogueCrew.TryStart(Save);
                var planet = Save.BaseGameData.Planets[ship.PlanetLocation];
                ship.ShipState = docking ? Ship_States.Docking : Ship_States.Docked;
                var victim = new IOS { ShipType = Ship_Types.IOS, ShipState = Ship_States.Docked, PlanetLocation = ship.PlanetLocation,
                    StarLocation = ship.StarLocation, Modules = new(), Name = "Ordinary victim" };
                Save.Ships.Add(victim); planet.Station.SdmCountdown = 1;
                SdmSystem.AdvanceTime(1);
                Equal(true, Save.Ships.Contains(ship), "rogue escapes before SDM casualty enumeration");
                Equal(false, Save.Ships.Contains(victim), "ordinary berth occupant still lost");
                Equal(docking ? Ship_States.UnDocked : Ship_States.Launching, ship.ShipState, "original escape state");
                Equal(11, Save.RogueCrew.Stage, "escape resumes launch wait");
                Equal(true, ReferenceEquals(ship.Pilot, Save.RogueCrew.Crew), "escape retains selected identity");
                Equal(false, planet.Station.Built, "station still destroyed");
            }
        }

        private void RogueHostileProtection()
        {
            var ship = NewRogueCandidate(); Save.RogueCrew.TryStart(Save);
            var planet = Save.BaseGameData.Planets[ship.PlanetLocation]; planet.ActiveMethanoid = true;
            AdvanceInterstellar(); AdvanceInterstellar();
            Equal(true, Save.Ships.Contains(ship), "hostile orbit does not destroy rogue");
            Equal(0, ship.AttackedCount, "rogue bypasses danger accumulation");
            var rolls = 0;
            ship.DestinationPlanetLocation = StellarBodies.mercury;
            Equal(true, ship.EngageEngine(() => { rolls++; return true; }), "rogue can depart hostile orbit");
            Equal(0, rolls, "rogue bypasses engine damage roll");
            Equal(false, ship.EngineDamaged, "drive remains undamaged");
        }

        private void RogueCrossStarRoute()
        {
            var ship = NewRogueCandidate(); Save.RogueCrew.TryStart(Save);
            var origin = Save.BaseGameData.Planets[ship.PlanetLocation].ParentStar;
            foreach (var p in Save.BaseGameData.Planets.Values.Where(p => p.ParentStar == origin)) p.ActiveMethanoid = false;
            var target = Save.BaseGameData.Planets.Values.First(p => p.ParentStar == StellarBodies.centauri && p.ActiveMethanoid);
            Save.RogueCrew.Stage = 1; AdvanceRogue();
            Equal(13, Save.RogueCrew.Stage, "cross-star course enters separate start stage");
            Equal(target.PlanetId, ship.DestinationPlanetLocation, "first remaining hostile system fallback");
            ship.Engine = false; AdvanceRogue(); Equal(13, Save.RogueCrew.Stage, "failed start does not advance");
            ship.Engine = true; AdvanceRogue(); Equal(14, Save.RogueCrew.Stage, "composed interstellar route starts");
            Equal(true, ship.Flight != null, "uses saved phase flight");
            AdvanceRogue(); Equal(14, Save.RogueCrew.Stage, "flight waits");
            AdvanceInterstellar();
            var restored = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            GameCore.SingletonInstance.GameData.ActiveSaveFile = restored; ship = restored.Ships.OfType<SCG>().Single();
            for (var i = 0; i < 150 && ship.ShipState == Ship_States.InTransit; i++) AdvanceInterstellar();
            Equal(target.PlanetId, ship.PlanetLocation, "actual remote body arrival");
            Equal(true, ship.Fuel < 250, "cross-star route consumes real fuel");
            AdvanceRogue(); Equal(15, Save.RogueCrew.Stage, "remote arrival observed");
            AdvanceRogue(); Equal(1, Save.RogueCrew.Stage, "arrival resets routing");
        }

        private void RogueCrewLossPaths()
        {
            foreach (var prison in new[] {false,true})
            {
                var ship = NewRogueCandidate(); Save.RogueCrew.TryStart(Save);
                var crew = ship.Pilot;
                if (prison)
                {
                    ship.Pilot = null;
                    ship.Modules[5] = new ShipModule { ModuleType = Module_Types.Tool, ItemStored = ItemTypes.prison_pod,
                        ItemCount = 1, StaffStored = crew };
                }
                ship.Fuel = 0; ship.FallingCount = 4; AdvanceInterstellar();
                Equal(false, Save.Ships.Contains(ship), "actual ship loss completed");
                Equal<Staff>(null, Save.RogueCrew.Crew, "loss clears selected pilot or prisoner");
                Equal(true, Save.RogueCrew.Occurred, "loss never resets event eligibility");
                Equal(1, Save.News.GetNews(100).Count(n => n.Contains("Pirate " + crew.Leader + " Killed.")), "one ranked crew-loss report");
                SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            }
        }

        private void RogueAccIsolation()
        {
            var ship = NewRogueCandidate(); Save.RogueCrew.TryStart(Save); ship.ShipState = Ship_States.Docked; ship.Fuel = 100;
            ship.ACC = new Deuteros.Code.Objects.ACC { Ship = ship, Source = ship.PlanetLocation, Destination = StellarBodies.mercury,
                SourceItems = new() {ItemTypes.titanium}, DestinationItems = new(), CurrentSource = ItemTypes.titanium,
                CurrentDestination = ItemTypes.iron, Active = true, CycleMode = true, Refuelling = true };
            var stores = Save.BaseGameData.Planets[ship.PlanetLocation].Station.Resources.Stores;
            stores[ship.FuelType] = 300; stores[ItemTypes.titanium] = 400;
            var cargo = ship.Modules[0].ItemCount;
            ship.ACC.Update(Ship_States.Docking); ship.ACC.Activate(); ship.ACC.Refuel(); ship.ACC.LoadSupply();
            Equal(100, ship.Fuel, "ordinary ACC cannot refuel rogue");
            Equal(cargo, ship.Modules[0].ItemCount, "ordinary ACC cannot replace pirate cargo");
            Equal(300, stores[ship.FuelType], "ordinary ACC cannot debit source fuel");
            Equal(400, stores[ItemTypes.titanium], "ordinary ACC cannot load source stock");
            Equal(false, ship.ACC.Active || ship.ACC.CycleMode || ship.ACC.Refuelling, "all modes stopped at shared boundary");
            Equal(Ship_States.Docked, ship.ShipState, "ordinary ACC cannot launch rogue");
        }

        private void RogueBusySafety()
        {
            var ship = NewRogueCandidate(); Save.RogueCrew.TryStart(Save);
            Save.RogueCrew.Stage = 1; AdvanceRogue(); AdvanceRogue();
            AdvanceRogue(); Equal(3, Save.RogueCrew.Stage, "travelling refit stage does not inspect departed human station");
            var home = Save.BaseGameData.Planets[ship.PlanetLocation];
            home.Station.Resources.Stores[ItemTypes.titanium] = 600;
            Save.RogueCrew.Stage = 10; AdvanceRogue();
            Equal(600, home.Station.Resources.Stores[ItemTypes.titanium], "retained raid cannot steal from previous location in transit");
            var rejected = Save.RogueCrew.RejectCommand(Save, ship);
            Equal<bool?>(true, rejected, "manual command rejected while rogue travels");
            Equal(10, Save.RogueCrew.Stage, "rejected in-flight command does not dispatch raid");
            var active = Save;
            var replacement = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            GameCore.SingletonInstance.GameData.ActiveSaveFile = replacement;
            active.RogueCrew.Advance(active, () => 4);
            Equal(600, home.Station.Resources.Stores[ItemTypes.titanium], "obsolete world cannot raid");
            Equal(true, Save.RogueCrew.RejectCommand(active, ship), "obsolete retained command rejected");
            ship = Save.Ships.OfType<SCG>().Single(); ship.ShipState = Ship_States.Docked; ship.Flight = null;
            Save.RogueCrew.Stage = 18; Save.RogueCrew.OriginalCrewCount = 81; ship.Pilot.Count = 10;
            Equal(true, Save.RogueCrew.RejectCommand(Save, ship), "docked rogue command triggers escape");
            Equal(81, ship.Pilot.Count, "command interruption restores temporary crew");
            Equal(11, Save.RogueCrew.Stage, "command dispatched one raid");
            Equal(Ship_States.Launching, ship.ShipState, "command starts departure");
        }

        private void RogueRefitProtectsCrew()
        {
            var ship = NewRogueCandidate(); Save.RogueCrew.TryStart(Save);
            var planet = Save.BaseGameData.Planets[ship.PlanetLocation]; planet.ActiveMethanoid = true;
            ship.ShipState = Ship_States.Docked; ship.DFCC = true; ship.DroneCount = 7; ship.Fuel = 40;
            var passenger = new Staff { Type = StaffType.Marines, Count = 20 };
            ship.Modules[5] = new ShipModule { ModuleType = Module_Types.Cryo, StaffStored = passenger };
            Save.RogueCrew.Stage = 3; AdvanceRogue();
            Equal(true, ReferenceEquals(passenger, ship.Modules[5].StaffStored), "rogue refit cannot discard another crew");
            Equal(40, ship.Fuel, "blocked refit preserves fuel");
            Equal(true, ship.DFCC && ship.DroneCount == 7, "blocked refit preserves conversion");
            Equal(3, Save.RogueCrew.Stage, "blocked refit remains pending");
        }

        private void RogueZeroFuelEscape()
        {
            var ship = NewRogueCandidate(); Save.RogueCrew.TryStart(Save); ship.Fuel = 0; ship.ShipState = Ship_States.Docked;
            var planet = Save.BaseGameData.Planets[ship.PlanetLocation]; planet.Station.SdmCountdown = 1;
            SdmSystem.AdvanceTime(1);
            Equal(true, Save.Ships.Contains(ship), "original internal launch bypasses manual fuel gate");
            Equal(Ship_States.Launching, ship.ShipState, "zero fuel escape starts actual launch");
            Equal(0, ship.Fuel, "escape never invents fuel");
        }

        private void RogueInterruptedSabotage()
        {
            foreach (var count in new[] {0,81})
            {
                var ship = NewRogueCandidate(); Save.RogueCrew.TryStart(Save); ship.Pilot.Count = count;
                Save.RogueCrew.Stage = 17; AdvanceRogue(); Equal(count >> 3, ship.Pilot.Count, "temporary strength");
                var crew = ship.Pilot; ship.Pilot = null;
                Save.BaseGameData.Planets[ship.PlanetLocation].Station.Resources.Staff[0] = crew;
                AdvanceRogue();
                Equal(count, crew.Count, "interrupted sabotage restores crew on roster");
                Equal<int?>(null, Save.RogueCrew.OriginalCrewCount, "interrupted work cleared");
                Equal(1, Save.RogueCrew.Stage, "interrupted controller safely resets");
                SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            }
        }

        private void RogueRoutingPriority()
        {
            var ship = NewRogueCandidate(); Save.RogueCrew.TryStart(Save);
            var origin = Save.BaseGameData.Planets[ship.PlanetLocation].ParentStar;
            foreach (var p in Save.BaseGameData.Planets.Values.Where(p => p.ParentStar == origin)) p.ActiveMethanoid = false;
            var preferred = Save.BaseGameData.Planets.Values.Where(p => p.ParentStar == StellarBodies.barnard).Take(3).ToList();
            for (var i = 0; i < 3; i++)
            {
                preferred[i].Station.Built = true; preferred[i].Station.BuildParts = 8;
                preferred[i].Station.RefiningSlot = i; preferred[i].ActiveMethanoid = i != 2;
            }
            Save.RogueCrew.Stage = 1; AdvanceRogue();
            Equal(StellarBodies.barnard, ship.DestinationStarLocation, "two enemies plus human preferred over earlier singleton");
            Equal(preferred[0].PlanetId, ship.DestinationPlanetLocation, "first allocated enemy");
            ship.PlanetLocation = preferred[0].PlanetId; ship.StarLocation = preferred[0].ParentStar;
            Save.RogueCrew.Stage = 5; AdvanceRogue();
            Equal(preferred[0].PlanetId, ship.DestinationPlanetLocation, "human below five drones falls back to enemy");
            AdvanceRogue(); Equal(5, Save.RogueCrew.Stage, "same-body human route reselects");
            preferred[2].Station.Resources.Stores[ItemTypes.ios_drone] = 5; AdvanceRogue();
            Equal(preferred[2].PlanetId, ship.DestinationPlanetLocation, "five drone human target");
            ship.Engine = false; AdvanceRogue(); Equal(6, Save.RogueCrew.Stage, "failed human departure waits");
            SaveStorage.Deserialize(SaveStorage.Serialize(Save));
        }

        private void RogueMtxAllocationBoundary()
        {
            var ship = NewRogueCandidate(); Save.RogueCrew.TryStart(Save); ship.ShipState = Ship_States.Docked;
            var source = Save.BaseGameData.Planets[ship.PlanetLocation]; source.Station.MtxInstalled = true;
            var candidate = Save.BaseGameData.Planets[StellarBodies.earth]; candidate.Station.Built = true; candidate.Station.BuildParts = 8;
            var mtx = source.Station.Resources.Stores.MTX;
            foreach (var condition in new[] {0,1,2,3})
            {
                candidate.Station.RefiningSlot = condition == 0 ? 8 : 7;
                candidate.ActiveMethanoid = condition == 2;
                candidate.Station.Resources.Stores[ItemTypes.aluminium] = condition == 1 ? 199 : 200;
                Save.RogueCrew.Stage = 9; AdvanceRogue();
                Equal(condition == 3 ? StellarBodies.earth : StellarBodies.none, mtx.Target, "MTX boundary " + condition);
            }
        }

        private async Task<ShipBay> RogueBay(bool cryo = false)
        {
            InitializeUi();
            var ship = NewRogueCandidate(); Save.RogueCrew.TryStart(Save); ship.ShipState = Ship_States.Docked;
            ship.Modules[5] = new ShipModule { ModuleType = cryo ? Module_Types.Cryo : Module_Types.Tool,
                ItemStored = cryo ? ItemTypes.none : ItemTypes.prison_pod, ItemCount = cryo ? 0 : 1 };
            Save.CurrentPlanet = ship.PlanetLocation;
            GameCore.SingletonInstance.ChangeScene(Scenes.ShipBay, new() {SceneVariables.Orbit,SceneVariables.Ship});
            await InputFrames();
            return ActiveScreen<ShipBay>();
        }

        private async Task RogueCockpitRecovery()
        {
            var bay = await RogueBay(); var original = bay.Ship; var crew = original.Pilot;
            Press(bay, ShipParts + "Cockpit/StaffList/Staff/Buttons/01");
            Equal<Staff>(null, original.Pilot, "actual cockpit removes rogue pilot");
            Equal(true, ReferenceEquals(crew, bay.ResourceList.Staff[0]), "crew moves into one roster slot");
            Equal(false, Save.RogueCrew.Controls(original), "recovered hull is human controlled");
            Equal(false,bay.GetNode<TextureButton>("Buttons/ShipNav/Nav_Torso6").Disabled,"recovery immediately re-enables prison navigation");
            original.ShipState = Ship_States.UnDocked;
            var ios = new IOS { ShipType = Ship_Types.IOS, ShipState = Ship_States.Docked, PlanetLocation = original.PlanetLocation,
                StarLocation = original.StarLocation, FuelType = ItemTypes.meh_fuel, Modules = Enumerable.Range(0,3).Select(_ => new ShipModule()).ToList(), Name = "Other hull" };
            Save.Ships.Add(ios);
            GameCore.SingletonInstance.ChangeScene(Scenes.ShipBay, new() {SceneVariables.Orbit,SceneVariables.Ship});
            await InputFrames(); bay = ActiveScreen<ShipBay>();
            Press(bay, ShipParts + "Cockpit/StaffList/Staff/Buttons/01");
            Equal<Staff>(null, ios.Pilot, "selected rogue cannot pilot IOS through actual roster button");
            Equal(true, ReferenceEquals(crew, bay.ResourceList.Staff[0]), "rejected assignment keeps crew in roster");
            SaveStorage.Deserialize(SaveStorage.Serialize(Save));
        }

        private async Task RogueCryoRejected()
        {
            var bay = await RogueBay(cryo:true); var crew = bay.Ship.Pilot;
            Press(bay, ShipParts + "Cockpit/StaffList/Staff/Buttons/01");
            Press(bay, "Buttons/ShipNav/Nav_Torso6");
            Press(bay, ShipParts + "Torso6/SpriteHolder/Buttons/ActivatePod");
            Press(bay, "StaffList/Staff/Buttons/01");
            Equal<Staff>(null, bay.Ship.Modules[5].StaffStored, "ordinary cryopod refuses selected rogue");
            Equal(true, ReferenceEquals(crew, bay.ResourceList.Staff[0]), "cryo refusal preserves roster identity");
        }

        private void RoguePrisonModel()
        {
            var ship = NewRogueCandidate(); Save.RogueCrew.TryStart(Save); ship.ShipState = Ship_States.Docked;
            var crew = ship.Pilot; ship.Pilot = null;
            var resource = Save.BaseGameData.Planets[ship.PlanetLocation].Station.Resources; resource.Staff[0] = crew;
            var prison = ship.Modules[5] = new ShipModule { ModuleType = Module_Types.Tool, ItemStored = ItemTypes.prison_pod, ItemCount = 1 };
            var state = Save.RogueCrew;
            Equal(true, state.TryCapture(Save, prison, resource, crew),
                "fitted prison captures selected rogue from local roster");
            Equal<Staff>(null, resource.Staff[0], "capture vacates roster slot");
            Equal(true, ReferenceEquals(crew, prison.StaffStored), "capture retains crew object");
            var restored = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            Equal(true, restored.RogueCrew.Contained(restored), "containment survives save");
            for (var i=0;i<4;i++) resource.Staff[i] = new Staff {Type=StaffType.Marines,Count=10};
            Equal(false, state.TryRelease(Save, prison, resource),
                "full roster retains prisoner");
            Equal(true, ReferenceEquals(crew,prison.StaffStored), "full release cannot lose crew");
            resource.Staff[2] = null;
            Equal(true, state.TryRelease(Save, prison, resource),
                "available roster slot releases prisoner");
            Equal(true, ReferenceEquals(crew,resource.Staff[2]), "release preserves selected reference");
            Equal(5, crew.GetLevel(), "release does not restore Warlord rank");
            Equal<Staff>(null, prison.StaffStored, "release clears prison");
        }

        private void RogueRosterHijack()
        {
            var ship = NewRogueCandidate(); Save.RogueCrew.TryStart(Save); ship.ShipState = Ship_States.Docked;
            var crew = ship.Pilot;
            var displaced = new Staff { Type=StaffType.Marines,Leader="Displaced",Count=10 };
            ship.Pilot = displaced;
            var resource = Save.BaseGameData.Planets[ship.PlanetLocation].Station.Resources;
            resource.Staff[2] = crew;
            AdvanceRogue();
            Equal(true, ReferenceEquals(crew,ship.Pilot), "free rogue hijacks docked SCG");
            Equal(true, ReferenceEquals(displaced,resource.Staff[2]), "hijack swaps original occupied roster slot");
            Equal(Ship_States.Launching,ship.ShipState,"hijack dispatches stage10 departure");
            Equal(11,Save.RogueCrew.Stage,"hijack returns to launch wait");
            Equal(2,Save.RogueCrew.PrisonCountdown,"hijack shortens prison discovery");
            SaveStorage.Deserialize(SaveStorage.Serialize(Save));
        }

        private async Task<ShipBay> OpenRoguePrison()
        {
            var bay = await RogueBay();
            Press(bay, ShipParts + "Cockpit/StaffList/Staff/Buttons/01");
            Press(bay, "Buttons/ShipNav/Nav_Torso6");
            Press(bay, ShipParts + "Torso6/SpriteHolder/Buttons/ActivatePod");
            Equal(true, bay.GetNode<Control>("StaffList").Visible, "fitted prison opens local roster");
            return bay;
        }

        private async Task RoguePrisonGesture()
        {
            foreach (var retain in new[] {false,true})
            {
                var bay = await OpenRoguePrison(); var crew = Save.RogueCrew.Crew; var prison = bay.Ship.Modules[5];
                Press(bay, "StaffList/Staff/Buttons/01");
                Equal(true, ReferenceEquals(crew,prison.StaffStored), "actual roster press captures prisoner");
                Press(bay, "StaffList/Staff/Buttons/01");
                Equal(true, ReferenceEquals(crew,prison.StaffStored), "repeated press cannot undo capture window");
                var count=crew.Count; StaffAttrition.Advance(Save,0,100,()=>1);
                Equal(count,crew.Count,"prisoner frozen during attrition");
                if(retain) await RightClick(bay.GetNode<Control>("StaffList").GetGlobalRect().GetCenter());
                await ToSignal(GetTree().CreateTimer(.65), SceneTreeTimer.SignalName.Timeout);
                Equal(retain,ReferenceEquals(crew,prison.StaffStored),"right click retains and timeout releases");
                Equal(!retain,bay.ResourceList.Staff.Any(t=>ReferenceEquals(t,crew)),"one physical crew location");
                SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                if(retain)
                {
                    Press(bay, ShipParts + "Torso6/SpriteHolder/Buttons/ActivatePod");
                    await CaptureDisplayEvidence("rogue-prison-retained");
                    Press(bay, "StaffList/Staff/Buttons/01");
                    Equal<Staff>(null,prison.StaffStored,"empty roster slot releases retained prisoner");
                }
            }
        }

        private async Task RoguePrisonInterruptedUi()
        {
            foreach(var replacement in new[]{false,true})
            {
                var bay=await OpenRoguePrison(); var active=Save; var prison=bay.Ship.Modules[5];var crew=Save.RogueCrew.Crew;
                Press(bay,"StaffList/Staff/Buttons/01");
                if(replacement) GameCore.SingletonInstance.GameData.ActiveSaveFile=SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                else GameCore.SingletonInstance.ChangeScene(Scenes.SaveScreen,new());
                await ToSignal(GetTree().CreateTimer(.65),SceneTreeTimer.SignalName.Timeout);
                Equal(true,ReferenceEquals(crew,prison.StaffStored),"interrupted capture retains original prisoner");
                Equal(false,active.BaseGameData.Planets[StellarBodies.atlantic].Station.Resources.Staff.Any(t=>ReferenceEquals(t,crew)),"interrupted timer cannot reinsert crew");
                if(replacement) Equal(true,Save.RogueCrew.Contained(Save),"replaced world keeps saved containment");
            }
        }

        private async Task RoguePrisonFullRosterUi()
        {
            var bay=await OpenRoguePrison();var prison=bay.Ship.Modules[5];var crew=Save.RogueCrew.Crew;
            Press(bay,"StaffList/Staff/Buttons/01");
            for(var i=0;i<4;i++) bay.ResourceList.Staff[i]=new Staff{Type=StaffType.Marines,Count=10};
            await ToSignal(GetTree().CreateTimer(.65),SceneTreeTimer.SignalName.Timeout);
            Equal(true,ReferenceEquals(crew,prison.StaffStored),"timeout with full roster retains prisoner");
            Equal(4,bay.ResourceList.Staff.Count(t=>t!=null),"timeout does not overwrite unrelated crew");
            SaveStorage.Deserialize(SaveStorage.Serialize(Save));
        }

        private async Task RoguePrisonEquipmentSafety()
        {
            var bay=await OpenRoguePrison();var prison=bay.Ship.Modules[5];var crew=Save.RogueCrew.Crew;
            Press(bay,"StaffList/Staff/Buttons/01");
            await RightClick(bay.GetNode<Control>("StaffList").GetGlobalRect().GetCenter());
            foreach(var type in new[]{ItemTypes.prison_pod,ItemTypes.derrick})
            {
                var item=GameCore.SingletonInstance.GameData.GetItem(type);item.Locked=false;item.Research.Researched=true;
                bay.ResourceList.Stores[type]=3;
            }
            typeof(ShipBay).GetMethod("UpdateEquipmentStock", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(bay, null);
            PressEquipmentNamed(bay,GameCore.SingletonInstance.GameData.GetItem(ItemTypes.prison_pod).ShortName);
            PressEquipmentNamed(bay,GameCore.SingletonInstance.GameData.GetItem(ItemTypes.derrick).ShortName);
            Press(bay,ShipParts+"Torso6/SpriteHolder/Buttons/AddToolPod");
            Press(bay,"Buttons/Nav_Dismantle");
            Equal(true,Save.Ships.Contains(bay.Ship),"occupied prison prevents dismantling");
            Equal(true,ReferenceEquals(crew,prison.StaffStored),"all removal paths retain crew");
            Equal(ItemTypes.prison_pod,prison.ItemStored,"all removal paths retain prison equipment");
            Equal(3,bay.ResourceList.Stores[ItemTypes.prison_pod],"no duplicate prison refund");
            Equal(3,bay.ResourceList.Stores[ItemTypes.derrick],"no replacement debit");
        }

        private async Task RogueEmptyPrisonReturn()
        {
            var bay = await OpenRoguePrison();
            var item = GameCore.SingletonInstance.GameData.GetItem(ItemTypes.prison_pod); item.Research.Researched = true; item.Locked = false;
            var stock = bay.ResourceList.Stores[ItemTypes.prison_pod];
            var point=bay.GetNode<Button>("StaffList/PrisonEquipment").GetGlobalRect().GetCenter();
            GetViewport().PushInput(new InputEventMouseMotion{Position=point,GlobalPosition=point},true);
            foreach(var pressed in new[]{true,false}) GetViewport().PushInput(new InputEventMouseButton{Position=point,GlobalPosition=point,ButtonIndex=MouseButton.Left,Pressed=pressed},true);
            await InputFrames();
            Equal(true,bay.GetNode<Control>("EquipmentStock").Visible,"empty prison equipment is reachable through actual pointer hit testing");
            PressEquipmentNamed(bay,item.ShortName);
            Equal(ItemTypes.none,bay.Ship.Modules[5].ItemStored,"empty prison can be removed");
            Equal(stock+1,bay.ResourceList.Stores[ItemTypes.prison_pod],"one empty prison refunded");
            Equal(true,bay.ResourceList.Staff.Any(t=>ReferenceEquals(t,Save.RogueCrew.Crew)),"equipment removal does not consume free rogue");
        }

        private async Task RogueBayCommandRejection()
        {
            foreach(var button in new[]{"Fuel/FuelGauge/Minus/RepeatingButton","Fuel/FuelGauge/Plus/RepeatingButton",
                "Buttons/ShipNav/Nav_Torso1","Buttons/ShipNav/Nav_Engine","Buttons/Nav_Dismantle",ShipParts+"Cockpit/Buttons/AddACC"})
            {
                var bay=await RogueBay();var ship=bay.Ship;
                bay.ResourceList.Stores[ItemTypes.a__c__c]=2;
                Press(bay,button);
                Equal(Ship_States.Launching,ship.ShipState,"rogue bay command dispatches escape: "+button);
                Equal(true,Save.Ships.Contains(ship),"command cannot dismantle rogue");
                Equal(250,ship.Fuel,"command cannot return rogue fuel");
                Equal(2,bay.ResourceList.Stores[ItemTypes.a__c__c],"command cannot fit ACC");
            }
        }

        private async Task<ShipInterior> RogueInterior(bool select = true)
        {
            InitializeUi();var ship=NewRogueCandidate();if(select) Save.RogueCrew.TryStart(Save);
            Save.CurrentPlanet=ship.PlanetLocation;GameCore.SingletonInstance.ShipSelected=ship.ShipID;
            GameCore.SingletonInstance.ChangeScene(Scenes.ShipInterior,new());await InputFrames();
            return ActiveScreen<ShipInterior>();
        }

        private async Task RogueInteriorCommandRejection()
        {
            var interior=await RogueInterior();var ship=interior.Ship;
            var destination=ship.DestinationPlanetLocation;
            Press(interior,"EngineControls/DisengageEngine");
            Equal(true,ship.EngineEngaged,"rogue retained engine callback cannot change drive");
            Press(interior,"SetCourse");
            Equal(0,interior.GetNode<Control>("StarMap").GetChildCount(),"rogue cannot open course selector");
            Press(interior,"TextLayout/RenameShip");
            Equal(false,Deuteros.Code.Platform.Helpers.OverlayManager.Instance.IsOpen,"rogue cannot open rename dialog");
            Press(interior,"TextLayout/CargoActions");
            Equal(false,Deuteros.Code.Platform.Helpers.OverlayManager.Instance.IsOpen,"rogue cannot open cargo disposal");
            Press(interior,"Modules/00");
            Equal(Ship_States.UnDocked,ship.ShipState,"rogue module action cannot dock or launch combat");
            Equal(destination,ship.DestinationPlanetLocation,"rogue course unchanged");
            Equal(true,interior.GetNode<Button>("EngineControls/DisengageEngine").Disabled,"disabled state matches command guard");
        }

        private async Task RogueRetainedRename()
        {
            var interior=await RogueInterior(false);var ship=interior.Ship;
            var dialog = OpenRename(interior);
            var edit=dialog.GetNode<LineEdit>("NameEdit");
            edit.Text="Stale renamed";
            Save.RogueCrew.TryStart(Save);
            dialog.GetNode<Button>("Confirm").EmitSignal(Button.SignalName.Pressed);
            Equal("BOUNTY",ship.Name,"rename callback retained before mutiny cannot rename rogue");
            Deuteros.Code.Platform.Helpers.OverlayManager.Instance.CloseOverlay();
        }
        private void RogueRetainedModules(string kind)
        {
            foreach (var replaced in new[] {false,true})
            {
                var ship=NewRogueCandidate();
                var module=ship.Modules[0];
                Node panel;
                if(kind=="mining")
                {
                    ship.PlanetLocation=StellarBodies.asteroids; ship.StarLocation=StellarBodies.the_sun;
                    module.ModuleType=Module_Types.Tool;module.ItemStored=ItemTypes.a__m__a;module.ItemCount=1;module.LastMinedDay=17;
                    ship.ItemScanResults=new Asteroid{Type=ItemTypes.iron,Class=6,Mass=10000,MassName="Large"};
                    var ama=OpenUi<Deuteros.Code.Platform.Screens.ModuleScenes.AMA>("res://PreFabs/ShipModuleWindows/AMA.tscn");
                    ama.Load(ship,module);panel=ama;
                }
                else if(kind=="grapple")
                {
                    module.ModuleType=Module_Types.Tool;module.ItemStored=ItemTypes.grapple;module.ItemCount=1;
                    module.HeldItem=new Asteroid{GrappleItemType=GrappleItemTypes.Asteroid,Type=ItemTypes.iron,Mass=37,MassName="Small"};
                    var grapple=OpenUi<Deuteros.Code.Platform.Screens.ModuleScenes.Grapple>("res://PreFabs/ShipModuleWindows/Grapple.tscn");
                    grapple.Load(ship,module);panel=grapple;
                }
                else if(kind=="acc")
                {
                    ship.ACC=new Deuteros.Code.Objects.ACC{Ship=ship,Source=ship.PlanetLocation,Destination=ship.PlanetLocation,
                        SourceItems=new(){ItemTypes.titanium},DestinationItems=new(),CurrentSource=ItemTypes.titanium,CurrentDestination=ItemTypes.iron};
                    var acc=OpenUi<Deuteros.Code.Platform.Screens.ACC>("res://PreFabs/ACC.tscn");acc.SetACC(ship.ACC);acc.UpdateState();panel=acc;
                }
                else
                {
                    ship.DroneCount=10;
                    var fleet=OpenUi<global::FleetTransfers>("res://PreFabs/ShipModuleWindows/FleetTransfers.tscn");fleet.TransferDrones(ship);panel=fleet;
                }
                try
                {
                    // Mutiny selection uses the qualifying mixed station, then the retained window acts at its current location.
                    var location=ship.PlanetLocation;var star=ship.StarLocation;
                    ship.PlanetLocation=StellarBodies.atlantic;ship.StarLocation=Save.BaseGameData.Planets[StellarBodies.atlantic].ParentStar;
                    if(!replaced) { Save.RogueCrew.TryStart(Save); Equal(true,Save.RogueCrew.Controls(ship),"retained fixture selects rogue"); }
                    ship.PlanetLocation=location;ship.StarLocation=star;
                    if(replaced) { CoreData.CreateBaseGameData(); GameCore.SingletonInstance.GameData.ActiveSaveFile=CoreData.CreateNewSaveFile(); }
                    if(kind=="mining")
                    {
                        Press(panel,"Buttons/Mine");
                        Equal(Ship_States.UnDocked,ship.ShipState,"retained AMA cannot start mining");
                        Equal((uint)17,module.LastMinedDay,"rejected AMA preserves mining timer");
                    }
                    else if(kind=="grapple")
                    {
                        var held=module.HeldItem;Press(panel,"Enabled/Buttons/Release");
                        Equal(held,module.HeldItem,"retained grapple cannot discard cargo");
                        module.HeldItem=null;ship.ItemScanResults=held;Press(panel,"Enabled/Buttons/Grab");
                        Equal<GrappleItem>(null,module.HeldItem,"retained grapple cannot collect cargo");
                        Equal(held,ship.ItemScanResults,"rejected grapple preserves scan");
                    }
                    else if(kind=="acc")
                    {
                        Press(panel,"Window/Buttons/Clear");
                        Equal(true,ship.ACC.SourceItems.Contains(ItemTypes.titanium),"retained ACC cannot clear configuration");
                        Press(panel,"Window/SourceButtons/Col01/00");
                        Equal(false,ship.ACC.SourceItems.Contains(ItemTypes.iron),"retained ACC cannot change filters");
                    }
                    else
                    {
                        Press(panel,"RepeatingButton2");
                        Equal(10,ship.DroneCount,"retained fleet transfer cannot remove drones");
                    }
                }
                finally{panel.Free();}
            }
        }

        private void RogueRetainedBattle()
        {
            foreach(var replaced in new[]{false,true})
            {
                var ship=NewRogueCandidate();ship.PTL=true;ship.DroneCount=10;
                var logic=new Deuteros.Code.Objects.Battle.BattleLogic(ship,new EnemyFleet{DroneCount=10},0,null,null,null,null,null);
                typeof(Deuteros.Code.Objects.Battle.BattleLogic).GetProperty("BattleState", System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)
                    .SetValue(logic,BattleState.FleetsInBattle);
                Equal(true,logic.canPTL(),"ordinary battle begins with available PTL");
                if(replaced) { CoreData.CreateBaseGameData();GameCore.SingletonInstance.GameData.ActiveSaveFile=CoreData.CreateNewSaveFile(); }
                else Save.RogueCrew.TryStart(Save);
                logic.LaunchPTL();logic.PlayerFlee();
                Equal(250,ship.Fuel,"retained battle cannot fire PTL");
                Equal(false,logic.PlayerFled,"retained battle cannot flee");
                Equal(false,logic.canPTL()||logic.canFlee(),"battle controls reflect rejection");
            }
        }

        private void RogueObsoleteAcc()
        {
            var ship=NewRogueCandidate();ship.ShipState=Ship_States.Docked;ship.Fuel=100;
            ship.ACC=new Deuteros.Code.Objects.ACC{Ship=ship,Source=ship.PlanetLocation,Destination=ship.PlanetLocation,
                SourceItems=new(){ItemTypes.iron},DestinationItems=new(),CurrentSource=ItemTypes.iron,CurrentDestination=ItemTypes.iron};
            CoreData.CreateBaseGameData();GameCore.SingletonInstance.GameData.ActiveSaveFile=CoreData.CreateNewSaveFile();
            var stores=Save.BaseGameData.Planets[ship.PlanetLocation].Station.Resources.Stores;
            stores[ItemTypes.hed_fuel]=1000;stores[ItemTypes.iron]=500;
            var cargo=ship.Modules[0].ItemCount;
            ship.ACC.Refuel();ship.ACC.LoadSupply();ship.ACC.Activate();ship.ACC.Update(Ship_States.Docking);
            Equal(100,ship.Fuel,"obsolete ACC cannot refuel an old hull");
            Equal(cargo,ship.Modules[0].ItemCount,"obsolete ACC cannot change old cargo");
            Equal(1000,stores[ItemTypes.hed_fuel],"obsolete ACC cannot debit replacement fuel");
            Equal(500,stores[ItemTypes.iron],"obsolete ACC cannot touch replacement stores");
        }

        private async Task RogueRetainedCargoCourse()
        {
            foreach(var replaced in new[]{false,true})
            foreach(var course in new[]{false,true})
            {
                var interior=await RogueInterior(false);var ship=interior.Ship;var destination=ship.DestinationPlanetLocation;
                Control dialog=null;
                if(course)
                {
                    Press(interior,"SetCourse");
                    interior.GetNode<Control>("StarMap").GetChild<StarMap>(0).LoadMap(StellarBodies.mars);
                }
                else dialog=await OpenSupplyPods(interior);
                if(replaced) { CoreData.CreateBaseGameData();GameCore.SingletonInstance.GameData.ActiveSaveFile=CoreData.CreateNewSaveFile(); }
                else Save.RogueCrew.TryStart(Save);
                if(course)
                {
                    await RightClick(interior.GetNode<Control>("StarMap").GetGlobalRect().GetCenter());
                    Equal(destination,ship.DestinationPlanetLocation,"retained map cannot change course");
                    Equal(false,Deuteros.Code.Platform.Helpers.OverlayManager.Instance.IsOpen,"rejected obsolete or rogue course closes silently");
                }
                else
                {
                    Press(dialog,"Rows/Pod0/Ditch");
                    Equal(12,ship.Modules[0].ItemCount,"retained cargo dialog preserves supply");
                }
                if(Deuteros.Code.Platform.Helpers.OverlayManager.Instance.IsOpen) Deuteros.Code.Platform.Helpers.OverlayManager.Instance.CloseOverlay();
            }
        }

        private async Task RogueRetainedBayFitting()
        {
            foreach(var replaced in new[]{false,true})
            foreach(var engine in new[]{false,true})
            {
                var bay=await RogueBay();var ship=bay.Ship;var stores=bay.ResourceList.Stores;
                var module=ship.Modules[0];
                module.HeldItem=new Asteroid{GrappleItemType=GrappleItemTypes.Asteroid,Type=ItemTypes.iron,Mass=37};
                var held=module.HeldItem;var stock=stores[ItemTypes.iron];
                ship.Engine=false;stores[ItemTypes.star_drive]=3;
                bay.GetNode<Deuteros.Code.Platform.Screens.ShipBayScenes.Engine>(ShipParts+"Engine").Installed=false;
                if(replaced) { CoreData.CreateBaseGameData();GameCore.SingletonInstance.GameData.ActiveSaveFile=CoreData.CreateNewSaveFile(); }
                if(engine) Press(bay,ShipParts+"Engine/SpriteHolder/Buttons/InstallEngine");
                else bay.GetNode<DynamicWindow>("GrappleWindow/GrappleEmptier").Closed(0);
                Equal(false,ship.Engine,"retained fitting cannot install drive");
                Equal(3,stores[ItemTypes.star_drive],"retained fitting preserves drive stock");
                Equal(held,module.HeldItem,"retained bay analysis preserves grapple cargo");
                Equal(stock,stores[ItemTypes.iron],"retained bay analysis preserves mineral stock");
            }
        }

        private void RoguePrisonDiscoveryClock()
        {
            NewRogueCandidate();Save.RogueCrew.TryStart(Save);var state=Save.RogueCrew;
            Equal(false,state.AdvancePrisonDiscovery(Save),"unpublished mutiny cannot advance discovery");
            Equal(true,state.PublishMutiny(Save),"pending mutiny publishes once");
            Equal(252,state.PrisonCountdown,"mutiny initializes original discovery countdown");
            Equal(false,state.PublishMutiny(Save),"mutiny cannot restart its countdown");
            for(var i=0;i<3;i++) Equal(false,state.AdvancePrisonDiscovery(Save),"first three visits wait");
            Equal(252,state.PrisonCountdown,"divider preserves countdown between fourth visits");
            GameCore.SingletonInstance.GameData.ActiveSaveFile=SaveStorage.Deserialize(SaveStorage.Serialize(Save));state=Save.RogueCrew;
            Equal(false,state.AdvancePrisonDiscovery(Save),"saved fourth visit decrements only");
            Equal(251,state.PrisonCountdown,"saved divider does not restart");
            state.PrisonCountdown=2;
            for(var i=0;i<7;i++) Equal(false,state.AdvancePrisonDiscovery(Save),"shortened discovery waits eight eligible visits");
            Equal(true,state.AdvancePrisonDiscovery(Save),"zero reaching visit discovers prison");
            var item=GameCore.SingletonInstance.GameData.GetItem(ItemTypes.prison_pod);
            Equal(false,item.Research.Locked,"discovery exposes existing research");
            Equal(false,item.Research.Researched,"discovery does not grant research completion");
            Equal(true,item.Locked,"discovery does not bypass paid manufacture");
            Equal(false,state.AdvancePrisonDiscovery(Save),"prison bulletin cannot repeat");
        }

        private async Task RogueStoryIntegration()
        {
            InitializeUi();var ship=NewRogueCandidate();ship.EngineEngaged=false;
            var core=GameCore.SingletonInstance;core.SetProcess(false);
            var hyperlight=core.GameData.GetItem(ItemTypes.hyperlight);
            hyperlight.Research.Researched=false;hyperlight.Research.ResearchPercentageComplete=17;
            GameCore.Earth.ResearchStaff=null;
            Save.BaseGameData.BulletinTexts[BulletinTypes.Hyperlight_Speed].BulletinText="Earlier discovery.";
            Save.BaseGameData.BulletinTexts[BulletinTypes.Mutiny].BulletinText="Mutiny.";
            Save.BaseGameData.BulletinTexts[BulletinTypes.Rogue_Ship].BulletinText="Prison discovered.";
            Save.News.PendingBulletins.Add(BulletinTypes.Hyperlight_Speed);
            core.ChangeScene(Scenes.SaveScreen,new());await InputFrames();
            core._Process(0);
            Equal(false,Save.RogueCrew.Occurred,"nonadvancing update cannot select rogue");
            AdvanceTickDay();
            Equal(true,Save.RogueCrew.Controls(ship),"normal simulation selects eligible partial research ship");
            Equal(BulletinTypes.Hyperlight_Speed,Save.News.LastBulletin,"existing notice keeps priority");
            Equal(true,Save.RogueCrew.MutinyPending,"competing notice preserves pending mutiny");
            Equal(0,Save.RogueCrew.PrisonCountdown,"competing notice cannot start prison clock");
            await FinishBulletin(ActiveScreen<Bulletins>());
            core.GameData.ActiveSaveFile=SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            ship=Save.Ships.OfType<SCG>().Single();
            core.ChangeScene(Scenes.SaveScreen,new());await InputFrames();AdvanceTickDay();
            Equal(BulletinTypes.Mutiny,Save.News.LastBulletin,"saved pending mutiny publishes");
            Equal(252,Save.RogueCrew.PrisonCountdown,"mutiny initializes prison delay once");
            await FinishBulletin(ActiveScreen<Bulletins>());
            await CaptureDisplayEvidence("rogue-mutiny-bulletin");
            // Keep the free crew ashore so the story clock can be exercised without a second hijack.
            Save.BaseGameData.Planets[ship.PlanetLocation].Station.Resources.Staff[0]=ship.Pilot;ship.Pilot=null;
            Save.RogueCrew.PrisonCountdown=1;Save.RogueCrew.PrisonDivider=0;
            core.ChangeScene(Scenes.SaveScreen,new());await InputFrames();
            Save.News.PendingBulletins.Add(BulletinTypes.Hyperlight_Speed);AdvanceTickDay();
            Equal(1,Save.RogueCrew.PrisonCountdown,"higher-priority bulletin postpones prison countdown");
            Equal(0,Save.RogueCrew.PrisonDivider,"higher-priority bulletin does not consume divider visit");
            await FinishBulletin(ActiveScreen<Bulletins>());
            core.ChangeScene(Scenes.SaveScreen,new());await InputFrames();
            for(var i=0;i<3;i++) AdvanceTickDay();
            Equal(1,Save.RogueCrew.PrisonCountdown,"only every fourth low-priority visit decrements");
            var before=Save.RogueCrew.PrisonDivider;core._Process(0);
            Equal(before,Save.RogueCrew.PrisonDivider,"nonadvancing update cannot advance discovery");
            AdvanceTickDay();
            Equal(BulletinTypes.Rogue_Ship,Save.News.LastBulletin,"normal story phase publishes prison discovery");
            Equal(false,core.GameData.GetItem(ItemTypes.prison_pod).Research.Locked,"normal story exposes prison research");
            await FinishBulletin(ActiveScreen<Bulletins>());
            await CaptureDisplayEvidence("rogue-prison-bulletin");
            SaveStorage.Deserialize(SaveStorage.Serialize(Save));
        }

        private async Task RoguePrisonPaidProgression()
        {
            InitializeUi();var ship=NewRogueCandidate();var core=GameCore.SingletonInstance;core.SetProcess(false);DisableFuelRefining();
            Save.RogueCrew.TryStart(Save);Save.RogueCrew.PublishMutiny(Save);
            Save.RogueCrew.PrisonCountdown=1;Save.RogueCrew.PrisonDivider=3;Save.RogueCrew.AdvancePrisonDiscovery(Save);
            var item=core.GameData.GetItem(ItemTypes.prison_pod);
            Save.CurrentPlanet=StellarBodies.earth;
            GameCore.Earth.ResearchStaff=new Staff{Type=StaffType.Research,Count=200,Leader="Science"};GameCore.Earth.ResearchStaff.AddAction(9);
            core.ChangeScene(Scenes.Earth_Research,new(){SceneVariables.Ground});await InputFrames();
            var research=ActiveScreen<Research>();
            research.Buttons.Single(b=>b.ObjectData?.ItemType==ItemTypes.prison_pod).EmitSignal(BaseButton.SignalName.Pressed);
            for(uint day=1;day<=100&&!item.Research.Researched;day++) Research.UpdateResearch(day-1,day);
            Equal(true,item.Research.Researched,"normal prison research completes");
            Equal(false,item.Locked,"research enables prison manufacture");
            await CaptureDisplayEvidence("rogue-prison-researched");
            core.ChangeScene(Scenes.SaveScreen,new());await InputFrames();await DrainStoppedAudio();
            Save.CurrentPlanet=ship.PlanetLocation;
            var station=Save.BaseGameData.Planets[ship.PlanetLocation].Station;station.Factory.AOC=true;
            var stores=station.Resources.Stores;stores[ItemTypes.prison_pod]=0;
            foreach(var recipe in item.BuildRequirements) stores[recipe.ItemType]=recipe.ItemCount;
            var production=OpenMtxProduction();
            try
            {
                production.Buttons.Single(b=>b.ObjectData?.ItemType==ItemTypes.prison_pod).EmitSignal(BaseButton.SignalName.Pressed);
                ProductionDays(20);
            }
            finally{production.Free();}
            Equal(1,stores[ItemTypes.prison_pod],"normal production makes one prison pod");
            foreach(var recipe in item.BuildRequirements) Equal(0,stores[recipe.ItemType],"prison recipe charged exactly once");
            ship.ShipState=Ship_States.Docked;
            station.Resources.Staff[0]=ship.Pilot;ship.Pilot=null;
            ship.Modules[5]=new ShipModule{ModuleType=Module_Types.Tool};
            core.ChangeScene(Scenes.ShipBay,new(){SceneVariables.Orbit,SceneVariables.Ship});await InputFrames();
            var bay=ActiveScreen<ShipBay>();
            Press(bay,"Buttons/ShipNav/Nav_Torso6");Press(bay,ShipParts+"Torso6/SpriteHolder/Buttons/ActivatePod");
            PressEquipmentNamed(bay,item.ShortName);
            Equal(ItemTypes.prison_pod,ship.Modules[5].ItemStored,"manufactured prison fits through actual equipment selector");
            Equal(0,stores[ItemTypes.prison_pod],"fitting consumes exactly one prison");
            await RightClick(bay.GetNode<Control>("EquipmentStock").GetGlobalRect().GetCenter());
            Press(bay,ShipParts+"Torso6/SpriteHolder/Buttons/ActivatePod");Press(bay,"StaffList/Staff/Buttons/01");
            await RightClick(bay.GetNode<Control>("StaffList").GetGlobalRect().GetCenter());
            Equal(true,Save.RogueCrew.Contained(Save),"paid fitted prison captures rogue through actual roster controls");
            var restored=SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            Equal(true,restored.RogueCrew.Contained(restored),"paid prison containment survives save");
        }

        private async Task RoguePrisonHelpBounds()
        {
            var bay=await OpenRoguePrison();
            var help=bay.GetNode<Label>("StaffList/PrisonHelp");
            Equal(true,help.GetCombinedMinimumSize().X<=208,"initial capture instructions fit the panel width");
            await CaptureDisplayEvidence("rogue-prison-empty-controls");
        }

    }
}
