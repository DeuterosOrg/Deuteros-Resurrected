using System;
using System.Collections.Generic;
using System.Linq;
using Deuteros.Code;
using Deuteros.Code.Objects;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private sealed class MiningTrainingProbe : Planet
        {
            public int ResearchersAtMining;
            public MiningTrainingProbe() : base(StellarBodies.mercury, 0) { }
            public override void DayTick(uint previousDay, uint currentDay)
            {
                ResearchersAtMining = GameCore.Earth.ResearchStaff.Count;
                base.DayTick(previousDay, currentDay);
            }
        }

        private void SimultaneousProductionResearchAndArrival()
        {
            ClearAttritionWorld();
            DisableFuelRefining();
            var core = GameCore.SingletonInstance;
            var earth = GameCore.Earth;
            Save.CurrentDay = 99;
            Save.Clock.DateCentidays = 9900;
            PrepareTickMining(earth);
            var mercury = Save.BaseGameData.Planets[StellarBodies.mercury];
            var mining = new MiningTrainingProbe { PlanetResources = mercury.PlanetResources,
                Station = mercury.Station, ParentStar = mercury.ParentStar };
            Save.BaseGameData.Planets[StellarBodies.mercury] = mining;
            PrepareTickMining(mining);
            var researchers = AttritionTeam(StaffType.Research, countdown: 1, count: 245);
            researchers.Leader = "Science"; researchers.AddAction(5);
            earth.ResearchStaff = researchers;
            var training = earth.TrainingData;
            training.ResearcherLocked = true; training.ResearcherDayStart = 99;
            training.ResearcherTrainingTime = 1; training.ResearcherTrainingCount = 5; training.AvailableTrainees = 10;
            var research = core.GameData.GetItem(ItemTypes.meh_fuel).Research;
            research.Researched = false; research.TechLevel = 1; research.ResearchLimit = 100;
            research.ResearchPercentageComplete = 99; research.ResearchValue = 255;
            earth.CurrentResearchItem = research;
            var builder = AttritionTeam(StaffType.Production, countdown: 1, count: 250);
            builder.Leader = "Builder"; builder.AddAction(5);
            earth.Factory.Builder = builder; earth.Factory.Ground = true; earth.Factory.AOC = false;
            earth.Factory.ProductionQueue.Clear();
            earth.Factory.ProductionQueue.Add(new ProductionItem(Product())
                { Active = true, Production_Value = 255, Production_Complete = 3 });
            earth.PlanetResources.Stores[ItemTypes.derrick] = 0;
            var ship = new Shuttle
            {
                ShipType = Ship_Types.Shuttle, ShipState = Ship_States.Landing, OnGround = false,
                StartTravelDay = 98, PlanetLocation = StellarBodies.earth, DestinationPlanetLocation = StellarBodies.earth,
                StarLocation = StellarBodies.the_sun, DestinationStarLocation = StellarBodies.the_sun,
                Engine = true, Fuel = 50, FuelType = ItemTypes.meh_fuel,
                Pilot = AttritionTeam(countdown: 1),
                Modules = new List<ShipModule> { new ShipModule { ModuleType = Module_Types.Supply } }
            };
            ship.Pilot.Leader = "Pilot"; ship.Pilot.AddAction(9);
            ship.ACC = new ACC { Ship = ship, Active = true, Source = StellarBodies.earth, Destination = StellarBodies.earth,
                SourceItems = new List<ItemTypes>(), DestinationItems = new List<ItemTypes>(),
                CurrentSource = ItemTypes.iron, CurrentDestination = ItemTypes.iron };
            Save.Ships.Add(ship);
            earth.PlanetResources.Stores[ItemTypes.meh_fuel] = 100;
            var completed = 0;
            void ResearchCompleted(ResearchItem item)
            {
                completed++;
                Equal(Ship_States.Landing, ship.ShipState, "research completion precedes the due arrival and automatic departure");
                Equal(250, researchers.Count, "same-update graduates contribute before research");
                Equal(250, mining.ResearchersAtMining, "training precedes local mining even when that planet is enumerated before Earth");
                Equal(2, earth.PlanetResources.Stores[ItemTypes.iron], "mining precedes research");
                Equal(1, earth.PlanetResources.Stores[ItemTypes.derrick], "factory completion precedes research");
                Equal(1, researchers.AttritionCountdown, "research precedes attrition");
            }
            core.ResearchFinished += ResearchCompleted;
            try { AdvanceTickDay(); }
            finally { core.ResearchFinished -= ResearchCompleted; }
            Equal(1, completed, "research completes once");
            Equal(true, research.Researched, "research remains complete");
            Equal(0, researchers.AttritionCountdown, "research crew ages once after working");
            Equal(0, builder.AttritionCountdown, "production crew ages once after working");
            Equal(Ship_States.TakingOff, ship.ShipState, "arrival ACC refuels and starts next leg");
            var promotions = Save.News.GetNews(12).Where(n => n.Contains("New Rank:")).ToList();
            Equal(3, promotions.Count, "three due promotions recorded once");
            Equal(true, promotions[0].Contains("Builder") && promotions[1].Contains("Science") && promotions[2].Contains("Pilot"),
                "News preserves production, research, then ship action order");
        }

        private sealed class ArrivalCrewProbe : Shuttle
        {
            public int CrewPhaseAtArrival = -1;
            public override int TravelTimeRemain()
            {
                CrewPhaseAtArrival = Pilot.AttritionCountdown;
                return base.TravelTimeRemain();
            }
        }

        private void AttritionBeforeShipArrival()
        {
            ClearAttritionWorld();
            DisableFuelRefining();
            Save.CurrentDay = 99;
            Save.Clock.DateCentidays = 9900;
            var frozen = AttritionTeam(countdown: 1);
            var ship = new ArrivalCrewProbe
            {
                ShipType = Ship_Types.Shuttle, ShipState = Ship_States.Landing, OnGround = false,
                StartTravelDay = 98, PlanetLocation = StellarBodies.earth, Engine = true, Fuel = 10,
                Pilot = AttritionTeam(countdown: 1),
                Modules = new List<ShipModule> { new ShipModule { ModuleType = Module_Types.Cryo, StaffStored = frozen } }
            };
            Save.Ships.Add(ship);
            AdvanceTickDay();
            Equal(0, ship.CrewPhaseAtArrival, "arrival logic sees crew already aged by the original preceding phase");
            Equal(Ship_States.Docked, ship.ShipState, "actual shuttle arrival completes");
            Equal(true, ship.OnGround, "arrival updates ground location");
            Equal(1, frozen.AttritionCountdown, "transported cryopod remains frozen across arrival");
            Equal(9, ship.Fuel, "one ship update consumes fuel once");
        }
    }
}
