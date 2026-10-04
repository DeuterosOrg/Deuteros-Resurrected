using Godot;
using System.Collections.Generic;

namespace Deuteros.Code.Objects.Interfaces
{
    public interface IPlanet
    {
        PlanetResource PlanetResources { get; set; }
        bool IsMoon { get; set; }

        int MethanoidAttackedCount { get; set; }
        bool ActiveMethanoid { get; set; }
        SpaceStation Station { get; set; }
        Enums.StellarBodies PlanetId { get; set; }
        Enums.StellarBodies MoonParentPlanetId { get; set; }
        Enums.StellarBodies ParentStar { get; set; }
        Enums.PlanetColor PlanetColor { get; set; }
        Enums.PlanetStyle PlanetStyle { get; set; }
        public int ShuttleState { get; set; }
        public int StarShipState { get; set; }
        public int Order { get; set; }
        public List<int> MoonList { get; set; }
        public int BaseBuildParts { get; set; }
        public bool BaseDamaged { get; set; }
        public bool CaptiveBase { get; set; }

        public string PlanetImageName();
    }
}
