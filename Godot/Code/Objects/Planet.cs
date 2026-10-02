using Godot;
using System;
using System.Collections.Generic;

namespace Deuteros.Code.Objects
{
    [Serializable]
    public partial class Planet : Interfaces.IPlanet
    {
        public PlanetResource PlanetResources { get; set; }
        public bool ActivePlayer { get; set; }
        public bool IsMoon { get; set; }
        public int MethanoidAttackedCount { get; set; }
        public bool ActiveMethanoid { get; set; }
        public bool Segment { get; set; }
        public SpaceStation Station { get; set; }
        public Enums.StellarBodies PlanetId { get; set; }
        public Enums.StellarBodies MoonParentPlanetId { get; set; }
        public Enums.StellarBodies ParentStar { get; set; }
        public Enums.PlanetColor PlanetColor { get; set; }
        public Enums.PlanetStyle PlanetStyle { get; set; }
        public int ShuttleState { get; set; }
        public int StarShipState { get; set; }
        public int Order { get; set; }
        public List<int> MoonList { get; set; }
        public int BaseBuildParts { get; set; }
        public bool BaseDamaged { get; set; }

        public Planet(Enums.StellarBodies planetId, int order)
        {
            PlanetId = planetId;
            Order = order;
            Station = new SpaceStation(planetId);
            ActiveMethanoid = false;
            MoonParentPlanetId = Enums.StellarBodies.none;
        }

        public virtual void AddItems(Enums.ItemTypes itemToAdd, int count)
        {
            Station.Resources.Stores[itemToAdd] += count;
        }

        public string PlanetImageName()
        {
            return PlanetColor.ToString().ToPascalCase() + "_" + PlanetStyle.ToString().ToPascalCase();
        }

        public virtual void DayTick(uint previousDay, uint currentDay)
            => MineGround(previousDay, currentDay, Random.Shared.Next);

        internal void MineGround(uint previousDay, uint currentDay, Func<int> random)
        {
            if (currentDay <= previousDay) return;
            var earth = PlanetId == Enums.StellarBodies.earth;
            // ponytail: retain whole-day Earth cadence until fractional clocks are restored.
            if (earth ? currentDay % 2 != 0 : ActiveMethanoid || BaseBuildParts != 2 || BaseDamaged) return;

            var data = GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData;
            var orbital = Station.MtxInstalled && (!earth || Station.Type == 8);
            var stores = orbital ? Station.Resources.Stores : PlanetResources.Stores;
            foreach (var material in PlanetResources.Materials)
            {
                var multiplier = data.ResourceLevels_Survey_Multiplier[material.MaterialType];
                if (material.IsSurveying)
                {
                    if (material.SurveyTicks > 1) material.SurveyTicks--;
                    else
                    {
                        material.GroundAmount = (((random() & 0x7FFF) * multiplier) & 0x7FFF) | 0x32;
                        material.SurveyTicks = 0;
                    }
                    continue;
                }

                var batch = (long)PlanetResources.Derricks * data.ResourceRate_Per_Derrick[material.MaterialType];
                if (batch > material.GroundAmount)
                {
                    // The original discards an insufficient remainder without producing ore.
                    material.GroundAmount = 0;
                    material.SurveyTicks = (random() & 7) * multiplier;
                    continue;
                }
                material.GroundAmount -= (int)batch;
                material.SurveyTicks = material.GroundAmount == 0 ? Material.KnownEmpty : 0;
                stores[material.MaterialType] = (int)Math.Min(50000L, stores[material.MaterialType] + batch);
            }
        }
    }
}
