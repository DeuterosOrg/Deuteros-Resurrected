using Godot;
using System;

namespace Deuteros.Code.Objects
{
	public partial class SpaceStation
	{
		public StationResource Resources { get; set; }
		public int BuildParts { get; set; }
		public int Type { get; set; }
		public bool Built { get; set; }
		public Enums.StellarBodies PlanetId { get; set; }
		public Factory Factory { get; set; }
		public int ShuttleState { get; set; }
		public int StarShipState { get; set; }
		public bool SdmInstalled { get; set; }
        public int SdmCountdown { get; set; }
		public bool MtxInstalled { get; set; }
        public int StationOrdinal { get; set; }
        public int RefiningSlot { get; set; } = -1;

        public SpaceStation(Enums.StellarBodies planetId)
		{
			Resources = new StationResource();
			Factory = new Factory();
			PlanetId = planetId;
		}
	}
}
