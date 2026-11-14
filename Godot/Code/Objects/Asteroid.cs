using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Deuteros.Code.Enums;

namespace Deuteros.Code.Objects
{
	public class Asteroid : GrappleItem
	{
        public Enums.GrappleItemTypes GrappleItemType { get; set; }

        public static List<Enums.ItemTypes> ResourceTypeList = new List<Enums.ItemTypes>() { Enums.ItemTypes.titanium, Enums.ItemTypes.aluminium, Enums.ItemTypes.carbon, ItemTypes.copper, ItemTypes.paladium, ItemTypes.platinum, ItemTypes.silver, ItemTypes.silica };
		public static List<int> ResourceMassList = new List<int>() { 50, 100, 250, 1000, 5000, 10000, 25000, 60000 };
		public static List<int> ResourceClassList = new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8 };
		public static List<string> ResourceMassNameList = new List<string>() { "Small", "Small", "Small", "Medium", "Medium", "Large", "Large", "Large" };

		public Enums.ItemTypes Type { get; set; }
		public int Mass { get; set; }
		public bool HasBeenMined { get; set; }
		public int Class { get; set; }
		public string MassName { get; set; }
		public int DayCount { get; set; }

        public static Asteroid ScanAsteroids(InterStellarShip ship) =>
            ship.AsteroidClockPhase(7) ? GenerateAsteroid() : ship.AsteroidScanResults;

		public static Asteroid GenerateAsteroid(Random random = null)
		{
			random ??= Random.Shared;
			var asteroidType = random.Next(ResourceTypeList.Count);
			var asteroidSize = random.Next(ResourceClassList.Count);

			var newAsteroid = new Asteroid();
			newAsteroid.Type = ResourceTypeList[asteroidType];
			newAsteroid.Mass = ResourceMassList[asteroidSize];
			newAsteroid.HasBeenMined = false;
			newAsteroid.Class = ResourceClassList[asteroidSize];
			newAsteroid.MassName = ResourceMassNameList[asteroidSize];
			return newAsteroid;
		}
	}
}