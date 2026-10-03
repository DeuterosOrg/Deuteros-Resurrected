using System;
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
    }
}
