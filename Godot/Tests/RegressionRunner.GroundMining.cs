using System;
using System.IO;
using System.Linq;
using Deuteros.Code.Utility;
using Newtonsoft.Json.Linq;
using Deuteros.Code;
using Deuteros.Code.Objects;
using static Deuteros.Code.Enums;

namespace Deuteros.Tests
{
    public partial class RegressionRunner
    {
        private void GroundSurveyRandomBoundaries()
        {
            var planet = (Planet)Save.BaseGameData.Planets[StellarBodies.the_moon];
            PrepareTickMining(planet);
            var material = planet.PlanetResources.Materials[0];
            foreach (var rule in Save.BaseGameData.ResourceLevels_Survey_Multiplier)
            {
                material.MaterialType = rule.Key;
                foreach (var sample in new[] { 0, 1, 7, 8192, 32767, 65535 })
                {
                    var draws = 0;
                    int RandomWord() { draws++; return sample; }
                    material.GroundAmount = 0; material.SurveyTicks = Material.KnownEmpty;
                    planet.PlanetResources.Stores[rule.Key] = 0;
                    planet.MineGround(0, 1, RandomWord);
                    Equal((sample & 7) * rule.Value, material.SurveyTicks, "survey duration uses low three bits times mineral multiplier");
                    Equal(1, draws, "one random draw to begin survey");
                    var countdown = material.SurveyTicks;
                    for (var tick = countdown; tick > 1; tick--) planet.MineGround(1, 2, RandomWord);
                    Equal(1, draws, "countdown does not draw randomness");
                    planet.MineGround(2, 3, RandomWord);
                    Equal((((sample & 32767) * rule.Value) & 32767) | 0x32, material.GroundAmount, "original masked survey yield");
                    Equal(2, draws, "completion draws once");
                    Equal(0, planet.PlanetResources.Stores[rule.Key], "discovery produces no stock");
                }
            }
            material.MaterialType = ItemTypes.iron;
            material.GroundAmount = 2; material.SurveyTicks = 0;
            planet.PlanetResources.Stores[ItemTypes.iron] = 0;
            planet.MineGround(0, 20, () => throw new InvalidOperationException("exact depletion must not draw randomness"));
            Equal(2, planet.PlanetResources.Stores[ItemTypes.iron], "one consumed update produces one batch even with a day gap");
            Equal(Material.KnownEmpty, material.SurveyTicks, "exact depletion retains known zero");
            planet.PlanetResources.Derricks = 0;
            planet.MineGround(20, 21, () => throw new InvalidOperationException("zero extraction cannot exhaust a known zero"));
            Equal(false, material.IsSurveying, "zero derricks leave known zero untouched");
            planet.PlanetResources.Derricks = int.MaxValue;
            material.GroundAmount = 32767;
            planet.MineGround(21, 22, () => 0);
            Equal(0, material.GroundAmount, "large legacy derrick counts cannot overflow extraction arithmetic");
            Equal(2, planet.PlanetResources.Stores[ItemTypes.iron], "oversized batch cannot create ore");
        }

        private void GroundMiningSavedStates()
        {
            var planet = (Planet)Save.BaseGameData.Planets[StellarBodies.the_moon];
            PrepareTickMining(planet);
            var material = planet.PlanetResources.Materials[0];
            material.GroundAmount = 2;
            planet.MineGround(0, 1, () => 0);
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            planet = (Planet)Save.BaseGameData.Planets[StellarBodies.the_moon];
            material = planet.PlanetResources.Materials[0];
            Equal(false, material.IsSurveying, "known zero survives save/load");
            planet.MineGround(1, 2, () => 0);
            Equal(true, material.IsSurveying, "next insufficient extraction starts zero-delay survey");
            GameCore.SingletonInstance.GameData.ActiveSaveFile = SaveStorage.Deserialize(SaveStorage.Serialize(Save));
            planet = (Planet)Save.BaseGameData.Planets[StellarBodies.the_moon];
            material = planet.PlanetResources.Materials[0];
            Equal(true, material.IsSurveying, "zero-delay survey survives save/load separately from known zero");
            planet.MineGround(2, 3, () => 0);
            Equal(50, material.GroundAmount, "zero random completion yields original minimum");
            Equal(2, planet.PlanetResources.Stores[ItemTypes.iron], "load and survey never duplicate output");
            var baseline = SaveStorage.Serialize(Save);
            foreach (var ticks in new[] { 0, 3 })
            {
                var legacy = JObject.Parse(baseline);
                var deposit = legacy.Descendants().OfType<JProperty>().First(p => p.Name == "GroundAmount");
                deposit.Value = -2;
                deposit.Parent["SurveyTicks"] = ticks;
                var restored = SaveStorage.Deserialize(legacy.ToString());
                var first = restored.BaseGameData.Planets.Values.First(p => p.PlanetResources.Materials.Count > 0).PlanetResources.Materials[0];
                Equal(0, first.GroundAmount, "legacy negative vein repaired without inventing resources");
                Equal(ticks == 0 ? Material.KnownEmpty : ticks, first.SurveyTicks, "legacy active countdown preserved");
            }
            foreach (var rule in new[] { "ResourceRate_Per_Derrick", "ResourceLevels_Survey_Multiplier" })
            foreach (var value in new[] { -1, 256 })
            {
                var invalid = JObject.Parse(baseline);
                var rules = (JObject)invalid["Game"]["BaseGameData"][rule];
                rules.Properties().First(p => p.Name != "$id").Value = value;
                var rejected = false;
                try { SaveStorage.Deserialize(invalid.ToString()); }
                catch (InvalidDataException) { rejected = true; }
                Equal(true, rejected, "invalid rule value rejected before simulation");
            }
            foreach (var (field, value) in new[] { ("GroundAmount",32768), ("SurveyTicks",-2), ("SurveyTicks",1786), ("Derricks",-1) })
            {
                var invalid = JObject.Parse(baseline);
                invalid.Descendants().OfType<JProperty>().First(p => p.Name == field).Value = value;
                var rejected = false;
                try { SaveStorage.Deserialize(invalid.ToString()); }
                catch (Exception error) when (error is InvalidDataException || error is Newtonsoft.Json.JsonException) { rejected = true; }
                Equal(true, rejected, "invalid mining " + field + " rejected");
            }
        }

        private async System.Threading.Tasks.Task GroundMiningSurveyDisplay()
        {
            InitializeUi();
            Save.CurrentPlanet = StellarBodies.earth;
            var material = GameCore.Earth.PlanetResources.Materials[0];
            material.GroundAmount = 0; material.SurveyTicks = Material.KnownEmpty;
            GameCore.SingletonInstance.ChangeScene(Scenes.GroundMaterials, new System.Collections.Generic.List<SceneVariables> { SceneVariables.Ground });
            var screen = ActiveScreen<Deuteros.Code.Platform.Screens.GroundMaterials>();
            screen.QueueRedraw();
            await ToSignal(GetTree(), Godot.SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), Godot.SceneTree.SignalName.ProcessFrame);
            Equal("0", screen.MaterialAmounts.Text.Split('\n')[0], "known exhausted vein displays zero");
            material.SurveyTicks = 0;
            screen.QueueRedraw();
            await ToSignal(GetTree(), Godot.SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), Godot.SceneTree.SignalName.ProcessFrame);
            Equal("SURVEY", screen.MaterialAmounts.Text.Split('\n')[0], "zero-delay survey is displayed as survey");
            material.GroundAmount = 50;
            screen.QueueRedraw();
            await ToSignal(GetTree(), Godot.SceneTree.SignalName.ProcessFrame);
            await ToSignal(GetTree(), Godot.SceneTree.SignalName.ProcessFrame);
            Equal("50", screen.MaterialAmounts.Text.Split('\n')[0], "discovered amount replaces survey text");
        }

        private void GroundSurveyWithoutDerricks()
        {
            foreach (var body in new[] { StellarBodies.earth, StellarBodies.the_moon })
            {
                var planet = (Planet)Save.BaseGameData.Planets[body];
                PrepareTickMining(planet);
                planet.ActiveMethanoid = false;
                planet.PlanetResources.Derricks = 0;
                var material = planet.PlanetResources.Materials[0];
                material.GroundAmount = 0;
                material.SurveyTicks = 2;
                planet.DayTick(1, 2);
                Equal(1, material.SurveyTicks, "surveys advance without installed derricks " + body);
                foreach (var countdown in new[] { 0, 1 })
                {
                    material.GroundAmount = 0; material.SurveyTicks = countdown;
                    planet.DayTick(3, 4);
                    Equal(true, material.GroundAmount >= 50 && material.GroundAmount <= 32767,
                        "zero and one survey counters both discover a nonempty deposit");
                    Equal(0x32, material.GroundAmount & 0x32, "discovery preserves original forced low bits");
                    Equal(0, planet.PlanetResources.Stores[ItemTypes.iron], "survey completion does not also extract");
                }
            }
        }

        private void GroundMiningEligibility()
        {
            var planet = (Planet)Save.BaseGameData.Planets[StellarBodies.the_moon];
            PrepareTickMining(planet);
            var material = planet.PlanetResources.Materials[0];
            foreach (var gate in new[] { "hostile", "damaged", "unfinished", "same update", "backwards update" })
            {
                planet.ActiveMethanoid = gate == "hostile";
                planet.BaseDamaged = gate == "damaged";
                planet.BaseBuildParts = gate == "unfinished" ? 1 : 2;
                material.GroundAmount = 100; material.SurveyTicks = 0;
                planet.PlanetResources.Stores[ItemTypes.iron] = 0;
                var current = gate == "same update" ? 2u : gate == "backwards update" ? 1u : 3u;
                planet.DayTick(2, current);
                Equal(100, material.GroundAmount, gate + " cannot extract");
                Equal(0, planet.PlanetResources.Stores[ItemTypes.iron], gate + " cannot create stock");
            }
            planet.ActiveMethanoid = false; planet.BaseDamaged = false; planet.BaseBuildParts = 2;
            planet.Station.Built = false; planet.Station.BuildParts = 0;
            planet.DayTick(0, 1);
            Equal(2, planet.PlanetResources.Stores[ItemTypes.iron], "working local ground needs no completed orbital station");
            var earth = GameCore.Earth;
            PrepareTickMining(earth);
            earth.Station.MtxInstalled = true;
            earth.Station.Type = 7;
            earth.Station.Resources.Stores[ItemTypes.iron] = 0;
            earth.DayTick(1, 2);
            Equal(2, earth.PlanetResources.Stores[ItemTypes.iron], "Earth MTX requires original completed type eight station");
            Equal(0, earth.Station.Resources.Stores[ItemTypes.iron], "unfinished Earth orbit cannot receive mining output");
        }

        private void GroundMiningBoundaries()
        {
            foreach (var body in new[] { StellarBodies.earth, StellarBodies.the_moon })
            foreach (var mtx in new[] { false, true })
            {
                var planet = (Planet)Save.BaseGameData.Planets[body];
                PrepareTickMining(planet);
                planet.ActiveMethanoid = false;
                planet.Station.Built = true;
                planet.Station.BuildParts = 8;
                planet.Station.Type = 8;
                planet.Station.MtxInstalled = mtx;
                var material = planet.PlanetResources.Materials[0];
                var stores = mtx ? planet.Station.Resources.Stores : planet.PlanetResources.Stores;
                material.GroundAmount = 1;
                stores[ItemTypes.iron] = 100;
                planet.DayTick(1, 2);
                Equal(100, stores[ItemTypes.iron], "insufficient whole batch produces no ore " + body + " MTX=" + mtx);
                Equal(0, material.GroundAmount, "depleted deposit cannot become negative");
                material.GroundAmount = 10; material.SurveyTicks = 0;
                stores[ItemTypes.iron] = 49999;
                planet.DayTick(3, 4);
                Equal(50000, stores[ItemTypes.iron], "full batch clamps at storage limit");
                Equal(8, material.GroundAmount, "original extraction consumes whole batch even when output is capped");
                planet.DayTick(5, 6);
                Equal(50000, stores[ItemTypes.iron], "full stores remain capped");
                Equal(6, material.GroundAmount, "full stores do not stop original extraction");
            }
        }
    }
}
