using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Deuteros.Code.Utility;
using Deuteros.Code;
using Deuteros.Code.Objects;
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
    }
}
