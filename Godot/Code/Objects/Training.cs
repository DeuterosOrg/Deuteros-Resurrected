using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using static Deuteros.Code.Enums;

namespace Deuteros.Code.Objects
{
	[Serializable]
	public partial class Training
	{
		public int AvailableTrainees { get; set; }

		public int ResearcherMaxCount { get; set; }
		public int ProductionMaxCount { get; set; }

		public int ResearcherTrainingMax { get; set; }
		public int ProductionTrainingMax { get; set; }
		public int MarinesTrainingMax { get; set; }

		public bool ResearcherLocked { get; set; }
		public bool ProductionLocked { get; set; }
		public bool MarinesLocked { get; set; }

		public int ResearcherTrainingCount { get; set; }
		public int ProductionTrainingCount { get; set; }
		public int MarinesTrainingCount { get; set; }
		public uint ResearcherDayStart { get; set; }
		public uint ProductionDayStart { get; set; }
		public uint MarinesDayStart { get; set; }
		public int ResearcherTrainingTime { get; set; }
		public int ProductionTrainingTime { get; set; }
		public int MarinesTrainingTime { get; set; }

		public int ReferenceTeamSize { get; set; }
		public double ReferenceLevelMultiplier { get; set; }
		public double ReferenceDuration { get; set; }

		public double ReferenceCost { get { return ReferenceTeamSize * ReferenceLevelMultiplier * ReferenceDuration; } }

		//Called from Earth
		public void ChildDayTick(uint previousDay, uint currentDay)
		{
			var earth = GameCore.GetPlanet<Earth>(Enums.StellarBodies.earth);

			if (!GameCore.Earth.TrainingData.ResearcherLocked && GameCore.Earth.TrainingData.ResearcherTrainingCount > 0)
			{
				GameCore.Earth.TrainingData.ResearcherLocked = true;
				GameCore.Earth.TrainingData.ResearcherDayStart = previousDay;

			}
			else if (GameCore.Earth.TrainingData.ResearcherLocked && (currentDay - GameCore.Earth.TrainingData.ResearcherDayStart) >= GameCore.Earth.TrainingData.ResearcherTrainingTime)
			{
				GameCore.Earth.TrainingData.ResearcherLocked = false;

				if (earth.ResearchStaff == null)
				{
					var newResearcher = new Staff();

					newResearcher.Leader = GameCore.SingletonInstance.GameData.GetNextPersonName();
					newResearcher.Count = 0;
					newResearcher.Type = Enums.StaffType.Research;

					earth.ResearchStaff = newResearcher;
				}

				earth.ResearchStaff.Count += GameCore.Earth.TrainingData.ResearcherTrainingCount;
				GameCore.Earth.TrainingData.AvailableTrainees -= GameCore.Earth.TrainingData.ResearcherTrainingCount;
				GameCore.Earth.TrainingData.ResearcherTrainingCount = 0;

			}

			if (!GameCore.Earth.TrainingData.ProductionLocked && GameCore.Earth.TrainingData.ProductionTrainingCount > 0)
			{
				GameCore.Earth.TrainingData.ProductionLocked = true;
				GameCore.Earth.TrainingData.ProductionDayStart = previousDay;
			}
			else if (GameCore.Earth.TrainingData.ProductionLocked && (currentDay - GameCore.Earth.TrainingData.ProductionDayStart) >= GameCore.Earth.TrainingData.ProductionTrainingTime)
			{
				GameCore.Earth.TrainingData.ProductionLocked = false;

				if (earth.Factory.Builder == null)
				{
					var newProduction = new Staff();

					newProduction.Leader = GameCore.SingletonInstance.GameData.GetNextPersonName();
					newProduction.Count = GameCore.Earth.TrainingData.ProductionTrainingCount;
					newProduction.Type = Enums.StaffType.Production;
					earth.Factory.Builder = newProduction;
				}
				else
				{
					earth.Factory.Builder.Count += GameCore.Earth.TrainingData.ProductionTrainingCount;
				}

				GameCore.Earth.TrainingData.AvailableTrainees -= GameCore.Earth.TrainingData.ProductionTrainingCount;
				GameCore.Earth.TrainingData.ProductionTrainingCount = 0;
			}

			if (!GameCore.Earth.TrainingData.MarinesLocked && GameCore.Earth.TrainingData.MarinesTrainingCount > 0)
			{
				GameCore.Earth.TrainingData.MarinesLocked = true;
				GameCore.Earth.TrainingData.MarinesDayStart = previousDay;
			}
			else if (GameCore.Earth.TrainingData.MarinesLocked && (currentDay - GameCore.Earth.TrainingData.MarinesDayStart) >= GameCore.Earth.TrainingData.MarinesTrainingTime)
			{
				//Only produce marines if we have space for them
				if (GameCore.Earth.PlanetResources.Staff.Any(T => T == null))
				{
					GameCore.Earth.TrainingData.MarinesLocked = false;

					var newMarine = new Staff();

					newMarine.Leader = GameCore.SingletonInstance.GameData.GetNextPersonName();
					newMarine.Count = GameCore.Earth.TrainingData.MarinesTrainingCount;
					newMarine.Type = Enums.StaffType.Marines;

					earth.PlanetResources.AddStaff(newMarine);

					GameCore.Earth.TrainingData.AvailableTrainees -= GameCore.Earth.TrainingData.MarinesTrainingCount;
					GameCore.Earth.TrainingData.MarinesTrainingCount = 0;
				}
			}
		}
	}
}