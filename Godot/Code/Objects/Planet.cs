using Godot;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

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

        //Triggered from gamecore
        public virtual void DayTick(uint previousDay, uint currentDay)
        {
            int daysDifference = (int)Math.Floor((decimal)(currentDay - previousDay));

            //TODO This isn't right - We need to mine every other day, but mining can start on any day - Or can it?
            if (PlanetId == Enums.StellarBodies.earth && currentDay % 2 != 0)
            {
                return;
            }

            var randomGen = new Random((int)Time.GetTicksMsec());

            if (PlanetResources.Derricks > 0 && BaseBuildParts == 2 && !BaseDamaged)
            {
                foreach (var material in PlanetResources.Materials)
                {
                    if (material.GroundAmount < 1 && material.SurveyTicks == 0)
                    {
                        material.SurveyTicks = randomGen.Next(0, 8) * GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ResourceLevels_Survey_Multiplier[material.MaterialType];
                    }
                    else if (material.GroundAmount < 1 && material.SurveyTicks > 0)
                    {
                        material.SurveyTicks--;

                        if (material.SurveyTicks == 0)
                            material.GroundAmount = (randomGen.Next(0, 32768) * GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ResourceLevels_Survey_Multiplier[material.MaterialType]) & 0x7FFF;
                    }
                    else
                    {
                        int amountRemoved = (PlanetResources.Derricks * GameCore.SingletonInstance.GameData.ActiveSaveFile.BaseGameData.ResourceRate_Per_Derrick[material.MaterialType]) * daysDifference;
                        material.GroundAmount -= amountRemoved;

                        
                        if (Station.MtxInstalled)
                        {
                            //mtx mines directly to the station
                            if (Station.Resources.Stores[material.MaterialType] < 50000)
                                Station.Resources.Stores[material.MaterialType] += amountRemoved;
                        }
                        else
                        {
                            if (PlanetResources.Stores[material.MaterialType] < 50000)
                                PlanetResources.Stores[material.MaterialType] += amountRemoved;
                        }
                    }
                }
            }


        }
    }
}
