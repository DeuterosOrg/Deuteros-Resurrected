using System;
using System.IO;
using System.Linq;
using Deuteros.Code.Utility;
using Newtonsoft.Json.Linq;
using Deuteros.Code;
using Deuteros.Code.Objects;
using Deuteros.Code.Platform.Screens;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private SCG NewInterstellarRoute()
        {
            var ship = (SCG)HullShip(true);
            ship.Name = "Interstellar regression";
            ship.PlanetLocation = StellarBodies.mercury;
            ship.DestinationPlanetLocation = StellarBodies.atlantic;
            ship.Fuel = 250;
            Save.Clock.DateCentidays = 10000;
            var research = Save.BaseGameData.ItemList.Find(item => item.ItemType == ItemTypes.hyperlight).Research;
            research.Researched = true;
            research.ResearchPercentageComplete = 100;
            research.Locked = false;
            return ship;
        }

        private void InterstellarNotInstant()
        {
            var ship = NewInterstellarRoute();
            Equal(0, Save.BaseGameData.Planets[ship.PlanetLocation].Order, "source orbit index");
            Equal(0, Save.BaseGameData.Planets[ship.DestinationPlanetLocation].Order, "destination shares local index, not location");
            Equal(true, ship.EngageEngine(), "SCG accepts cross-star course");
            var previous = Save.CurrentDay++;
            ShipInterior.UpdateShips(previous, Save.CurrentDay);
            Equal(Ship_States.InTransit, ship.ShipState, "interstellar distance cannot disappear when orbit indices match");
            Equal(StellarBodies.mercury, ship.PlanetLocation, "requested remote body is not reached after one update");
        }

        private void InterstellarPhaseFuel()
        {
            var ship = NewInterstellarRoute();
            Equal(true, ship.EngageEngine(), "SCG accepts accelerated route");
            var previous = Save.CurrentDay++;
            ShipInterior.UpdateShips(previous, Save.CurrentDay);
            Equal(248, ship.Fuel, "first acceleration selects phase2 and charges two units without generic burn");
            previous = Save.CurrentDay++;
            ShipInterior.UpdateShips(previous, Save.CurrentDay);
            Equal(245, ship.Fuel, "next acceleration selects phase3 and charges three units");
        }

        private void AdvanceInterstellar(bool manual = true)
        {
            if (manual) Save.Clock.QueueManual(); else Save.Clock.AdvanceNormal(GameClock.NormalIntervalSeconds);
            Equal(true, Save.Clock.Consume(), "one clock increment consumed");
            var previous = Save.CurrentDay++;
            ShipInterior.UpdateShips(previous, Save.CurrentDay);
        }

        private void InterstellarTables()
        {
            var expected = new[] {
                0,4300,4400,6000,8200,8700,11000,11300,11800,
                4300,0,200,7000,11000,16300,15000,12700,13200,
                4400,200,0,8000,12000,15300,16000,12800,13300,
                6000,7000,8000,0,2500,2720,4550,23000,19400,
                8200,11000,12000,2500,0,1900,5500,7200,16100,
                8700,16300,15300,2720,1900,0,3800,14200,7600,
                11000,15000,16000,4550,5500,3800,0,450,6100,
                11300,12700,12800,23000,7200,14200,450,0,650,
                11800,13200,13300,19400,16100,7600,6100,650,0 };
            var clocks = new[] {0,430000,440000,600000,820000,870000,1100000,1130000,1180000};
            for (var origin = 0; origin < 9; origin++)
            {
                var star = (StellarBodies)((origin + 1) * 100000);
                Equal(clocks[origin], InterstellarFlight.StarOffset(star), "original initial star clock");
                for (var destination = 0; destination < 9; destination++)
                    Equal(expected[origin * 9 + destination], InterstellarFlight.Distance(star,
                        (StellarBodies)((destination + 1) * 100000)), "original star distance");
            }
            Equal(4, InterstellarFlight.LocalDistance(StellarBodies.earth), "Earth is original first Solar body");
            Equal(5, InterstellarFlight.LocalDistance(StellarBodies.the_moon), "first group's moon follows parent");
            Equal(8, InterstellarFlight.LocalDistance(StellarBodies.mercury), "Mercury is original second group");
            Equal(12, InterstellarFlight.LocalDistance(StellarBodies.venus), "Venus is original third group");
            Equal(4, InterstellarFlight.LocalDistance(StellarBodies.atlantic), "Proxima star-to-first-body leg");
        }

        private void InterstellarSaveJourney()
        {
            var ship = NewInterstellarRoute();
            Equal(16, ship.TravelTimeRemain(), "preview includes twelve star updates and four local updates");
            Equal<InterstellarFlight>(null, ship.Flight, "preview cannot create saved progress");
            ship.EngageEngine();
            var id = ship.ShipID;
            for (var update = 1; update <= 16; update++)
            {
                var before = SaveStorage.Serialize(Save);
                Equal(17 - update, ship.TravelTimeRemain(), "projection tracks all remaining legs");
                Equal(before, SaveStorage.Serialize(Save), "projection cannot mutate fuel clock or flight");
                AdvanceInterstellar(update % 2 == 0);
                if (update == 1) { Equal(4298, ship.Flight.Remaining, "first step advances two distance units"); Equal(200L, ship.Flight.ClockOffset, "private clock gains two days"); }
                if (update == 11) { Equal(InterstellarFlight.FlightLeg.Hyperlight, ship.Flight.Leg, "one-update Hyperlight transition"); Equal(0L, ship.Flight.AbsoluteClock(Save.Clock), "original transition clears private clock"); }
                if (update == 12) { Equal(InterstellarFlight.FlightLeg.Local, ship.Flight.Leg, "star arrival begins body approach"); Equal(141, ship.Fuel, "original star-route fuel trace"); Equal(430000L, ship.Flight.ClockOffset, "Hyperlight syncs destination clock"); }
                GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                ship = (SCG)Save.Ships.Single(s => s.ShipID == id);
                Equal(update == 16 ? Ship_States.UnDocked : Ship_States.InTransit, ship.ShipState, "reload preserves boundary without replay");
            }
            Equal(StellarBodies.atlantic, ship.PlanetLocation, "only final leg reaches requested body");
            Equal(137, ship.Fuel, "body leg costs four additional ordinary units");
            Equal<InterstellarFlight>(null, ship.Flight, "completed flight cleared before save");
            Equal(12, ship.Modules.Single().ItemCount, "cargo conserved across flight and every reload");
        }

        private void InterstellarOrdinaryClocks()
        {
            foreach (var route in new[] {0, 1, 2})
            {
                var ship = NewInterstellarRoute();
                Save.News = new Deuteros.Code.Objects.News();
                Save.BaseGameData.ItemList.Single(i => i.ItemType == ItemTypes.hyperlight).Research.Researched = false;
                if (route == 1) { ship.PlanetLocation = StellarBodies.atlantic; ship.DestinationPlanetLocation = StellarBodies.earth; }
                if (route == 2) { ship.PlanetLocation = StellarBodies.atlantic; ship.DestinationPlanetLocation = StellarBodies.chiron; }
                ship.EngageEngine();
                var starUpdates = route == 2 ? 26 : 208;
                for (var update = 1; update <= starUpdates; update++) AdvanceInterstellar(update % 2 == 0);
                Equal(route == 0, Save.Ships.Contains(ship), "ordinary arrival requires exact destination clock, not direction alone");
                if (route == 0)
                {
                    Equal(InterstellarFlight.FlightLeg.Local, ship.Flight.Leg, "matching star clock permits approach");
                    Equal(53, ship.Fuel, "ordinary original fuel trace");
                    for (var update = 0; update < 4; update++) AdvanceInterstellar();
                    Equal(StellarBodies.atlantic, ship.PlanetLocation, "ordinary matching flight arrives");
                }
                else
                {
                    Equal(1, Save.News.GetNews(100).Count(n => n.Contains(ship.Name + " Destroyed.")), "clock loss reported once");
                    AdvanceInterstellar();
                    Equal(1, Save.News.GetNews(100).Count(n => n.Contains(ship.Name + " Destroyed.")), "removed flight cannot repeat loss");
                }
            }
        }

        private void InterstellarFuelBoundaries()
        {
            foreach (var initial in new[] {1, 2, 108, 109})
            {
                var ship = NewInterstellarRoute(); ship.Fuel = initial; ship.EngageEngine();
                AdvanceInterstellar();
                Equal(initial != 1, Save.Ships.Contains(ship), "phase underflow loses immediately; exact phase cost does not");
                if (initial == 2) { Equal(0, ship.Fuel, "exact fuel remains valid at phase boundary"); AdvanceInterstellar(); Equal(false, Save.Ships.Contains(ship), "next unaffordable phase loses ship"); }
                if (initial < 100) continue;
                for (var update = 2; update <= 11; update++) AdvanceInterstellar();
                Equal(initial - 108, ship.Fuel, "ten phase changes cost108");
                AdvanceInterstellar();
                Equal(initial == 109 ? InterstellarFlight.FlightLeg.Hyperlight : InterstellarFlight.FlightLeg.Stranded,
                    ship.Flight.Leg, "burning final ordinary unit doubles pending Hyperlight countdown");
                if (initial == 109) AdvanceInterstellar();
                Equal(InterstellarFlight.FlightLeg.Stranded, ship.Flight.Leg, "zero-fuel transition coasts to star then strands");
            }
        }

        private void InterstellarMalformedSave()
        {
            var ship = NewInterstellarRoute(); ship.EngageEngine(); AdvanceInterstellar();
            var original = SaveStorage.Serialize(Save); var active = Save;
            foreach (var corrupt in new Action<JObject>[] {
                f => f["Leg"] = 9, f => f["Phase"] = 0, f => f["Phase"] = 12,
                f => f["Remaining"] = 0, f => f["Remaining"] = 50000,
                f => f["Fraction"] = -1, f => f["Fraction"] = 256,
                f => f["ClockOffset"] = long.MaxValue, f => f["ClockOffset"] = 100,
                f => f.Remove("Phase"), f => f["Remaining"] = null })
            {
                var document = JObject.Parse(original); corrupt((JObject)document["Game"]["Ships"][0]["Flight"]);
                var rejected = false;
                try { GameCore.SingletonInstance.LoadSavedGame(SaveStorage.Deserialize(document.ToString())); }
                catch (Exception e) when (e is IOException || e is InvalidDataException || e is Newtonsoft.Json.JsonException) { rejected = true; }
                Equal(true, rejected, "malformed flight rejected before world activation");
                Equal(true, ReferenceEquals(active, Save), "rejected save cannot replace current world");
            }
        }

        private void InterstellarEmptyStarArrival()
        {
            var ship = NewInterstellarRoute(); ship.Fuel = 108; ship.EngageEngine();
            for (var update = 0; update < 12; update++) AdvanceInterstellar();
            Equal(6, ship.Flight.Remaining, "empty-fuel star arrival starts original six-update loss countdown");
            for (var update = 1; update <= 6; update++)
            {
                AdvanceInterstellar();
                Equal(update < 6, Save.Ships.Contains(ship), "stranded star arrival expires once after six updates");
                Equal(StellarBodies.mercury, ship.PlanetLocation, "empty star arrival cannot invent a body approach");
                if (update < 6)
                {
                    var id = ship.ShipID;
                    GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
                    ship = (SCG)Save.Ships.Single(s => s.ShipID == id);
                }
            }
        }

        private void InterstellarDisengagement()
        {
            var ship = NewInterstellarRoute(); ship.EngageEngine();
            ship.DisengageEngine();
            Equal(true, ship.EngineEngaged, "original disengagement rejects accelerated interstellar state");
            for (var update = 0; update < 11; update++) AdvanceInterstellar();
            ship.DisengageEngine();
            Equal(true, ship.EngineEngaged, "Hyperlight transition cannot be interrupted");
            AdvanceInterstellar();
            ship.DisengageEngine();
            Equal(false, ship.EngineEngaged, "ordinary destination approach can drift");
            AdvanceInterstellar();
            Equal(3, ship.Flight.Remaining, "drift still consumes a travel update");
            Equal(140, ship.Fuel, "ordinary drift retains original per-update fuel cost");
            Equal(true, ship.EngageEngine(), "drifting local leg may re-engage");
            Equal(4, ship.Flight.Remaining, "original re-engagement recalculates the current star-to-body route");
            Equal(true, ship.EngineEngaged, "resume sets engine flag");
            Equal(430000L, ship.Flight.ClockOffset, "resume never resets to origin star clock");
        }
    }
}
